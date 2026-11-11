using System;
using ArchLight;

class RecoveryWorkloadTests
{
    static int Main()
    {
        var desired = new GammaController.RAMP { Red = new ushort[256], Green = new ushort[256], Blue = new ushort[256] };
        DateTime now = new DateTime(2026, 11, 11, 0, 0, 0, DateTimeKind.Utc);
        GammaController.SetExpected(desired);
        int reads = 0, writes = 0, failures = 0;
        for (int i = 0; i < 120; i++)
            GammaController.Recover(() => { reads++; return desired; }, r => { writes++; return true; }, now.AddMilliseconds(i * 500));
        Console.WriteLine("Stable display, simulated 60 seconds: reads=" + reads + ", writes=" + writes);
        if (reads != 30 || writes != 0) { Console.WriteLine("FAIL stable polling budget"); failures++; }
        else Console.WriteLine("PASS stable polling budget");

        GammaController.SetExpected(desired);
        reads = 0;
        bool escaped = false;
        for (int i = 0; i < 20; i++)
        {
            try { GammaController.Recover(() => { reads++; throw new InvalidOperationException("mock driver transition"); }, r => true, now.AddMilliseconds(i * 500)); }
            catch { escaped = true; }
        }
        Console.WriteLine("Throwing display, simulated 10 seconds: attempts=" + reads + ", exception escaped=" + escaped);
        if (escaped || reads > 4) { Console.WriteLine("FAIL driver exception backoff"); failures++; }
        else Console.WriteLine("PASS driver exception backoff");
        return failures == 0 ? 0 : 1;
    }
}
