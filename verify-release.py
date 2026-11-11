"""Validate cross-architecture packages without executing display-changing software."""
import argparse
import hashlib
import io
import pathlib
import struct
import subprocess
import tarfile
import zipfile


def digest(data):
    return hashlib.sha256(data).hexdigest()


def pe(data, expected):
    assert data[:2] == b'MZ', 'Missing DOS header'
    start = struct.unpack_from('<I', data, 0x3c)[0]
    assert data[start:start + 4] == b'PE\0\0', 'Missing PE signature'
    machine, sections = struct.unpack_from('<HH', data, start + 4)
    assert machine == expected, f'Wrong PE machine: {machine:#06x}, expected {expected:#06x}'
    optional_size = struct.unpack_from('<H', data, start + 20)[0]
    optional = start + 24
    magic = struct.unpack_from('<H', data, optional)[0]
    assert magic == (0x10b if expected == 0x14c else 0x20b), 'Wrong PE bitness'
    table = optional + optional_size

    def offset(rva):
        for index in range(sections):
            section = table + index * 40
            virtual_size, virtual_address, raw_size, raw_offset = struct.unpack_from('<IIII', data, section + 8)
            if virtual_address <= rva < virtual_address + max(virtual_size, raw_size):
                return raw_offset + rva - virtual_address
        raise AssertionError(f'RVA not mapped: {rva:#x}')

    directories = optional + (96 if magic == 0x10b else 112)
    clr_rva = struct.unpack_from('<I', data, directories + 14 * 8)[0]
    clr = offset(clr_rva)
    flags = struct.unpack_from('<I', data, clr + 16)[0]
    assert flags & 1, 'Expected managed IL-only assembly'
    if expected == 0x14c:
        assert flags & 2, 'x86 build must require 32-bit execution'
    resource_rva, resource_size = struct.unpack_from('<II', data, clr + 24)
    resources = []
    if resource_size:
        raw = offset(resource_rva)
        position = 0
        while position + 4 <= resource_size:
            length = struct.unpack_from('<I', data, raw + position)[0]
            assert position + 4 + length <= resource_size, 'Invalid managed resource length'
            resources.append(data[raw + position + 4:raw + position + 4 + length])
            position = (position + 4 + length + 7) & ~7
    return resources


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--release-directory', required=True, type=pathlib.Path)
    parser.add_argument('--build-directory', required=True, type=pathlib.Path)
    args = parser.parse_args()
    release, build = args.release_directory, args.build_directory
    lines = []

    def passed(message):
        lines.append('PASS ' + message)
        print(lines[-1])

    for architecture, machine in [('x64', 0x8664), ('x86', 0x14c), ('arm64', 0xaa64)]:
        app = (build / architecture / 'ArchLight.exe').read_bytes()
        uninstall = (build / architecture / 'Uninstall.exe').read_bytes()
        installer = (release / f'ArchLight-Installer.win-{architecture}.exe').read_bytes()
        pe(app, machine)
        pe(uninstall, machine)
        resources = pe(installer, machine)
        passed(f'{architecture}: application, installer and uninstaller PE machine = {machine:#06x}; managed IL flags verified')
        hashes = {digest(resource) for resource in resources}
        assert digest(app) in hashes, f'{architecture}: installer application differs'
        assert digest(uninstall) in hashes, f'{architecture}: installer uninstaller differs'
        with zipfile.ZipFile(release / f'ArchLight.win-{architecture}.zip') as archive:
            assert archive.read('ArchLight.exe') == app, f'{architecture}: portable payload differs'
            assert 'README.md' in archive.namelist(), 'Portable README missing'
        passed(f'{architecture}: embedded installer payloads and portable application match staged binaries')

    root = pathlib.Path(__file__).resolve().parent
    source = subprocess.check_output(['git', 'archive', '--format=zip', '--prefix=ArchLight/', 'HEAD'], cwd=root)
    with zipfile.ZipFile(io.BytesIO(source)) as expected, zipfile.ZipFile(release / 'Source.code.zip') as actual:
        expected_files = {name: digest(expected.read(name)) for name in expected.namelist() if not name.endswith('/')}
        actual_files = {name: digest(actual.read(name)) for name in actual.namelist() if not name.endswith('/')}
        assert actual_files == expected_files, 'Source ZIP does not match HEAD'
    with tarfile.open(release / 'Source.code.tar.gz', 'r:gz') as archive:
        actual_tar = {member.name: digest(archive.extractfile(member).read()) for member in archive.getmembers() if member.isfile()}
        assert actual_tar == expected_files, 'Source TAR does not match HEAD'
    commit = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=root, text=True).strip()
    passed(f'source ZIP and TAR match every tracked file in commit {commit}')

    for line in (release / 'SHA256SUMS.txt').read_text(encoding='utf-8-sig').splitlines():
        expected_hash, name = line.split('  ', 1)
        assert digest((release / name).read_bytes()) == expected_hash, f'Checksum mismatch: {name}'
    passed('all existing SHA256SUMS entries match')
    lines.extend([
        '', 'Runtime validation:',
        'x64: application launched on the local Windows x64 computer; installed settings preserved.',
        'x86: cross-compiled and package-validated; not executed on target hardware.',
        'ARM64: cross-compiled and package-validated; not executed on target hardware.',
        'ARM32: not supported. ARM64 requires native .NET Framework 4.8.1.',
        'Actual sleep/resume, HDR and multiple display-driver configurations were not hardware-tested.',
        'The 120-to-30 reads/minute result is a simulated driver-call count, not overall CPU speedup.',
    ])
    (release / 'Validation.txt').write_text('\n'.join(lines) + '\n', encoding='utf-8')
    sums = [f'{digest(path.read_bytes())}  {path.name}' for path in sorted(release.iterdir()) if path.is_file() and path.name != 'SHA256SUMS.txt']
    (release / 'SHA256SUMS.txt').write_text('\n'.join(sums) + '\n', encoding='utf-8')


if __name__ == '__main__':
    main()
