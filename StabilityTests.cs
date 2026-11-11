using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using ArchLight;

class StabilityTests
{
    sealed class Display : IDisplayFilter
    {
        internal int Applies, Restores, Captures;
        public void Capture() { Captures++; }
        public void Apply(int k, int b) { Applies++; }
        public void Restore() { Restores++; }
        public void Recover() { }
    }

    static int passed, failed;
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static T Field<T>(object target, string name) { return (T)target.GetType().GetField(name, Private).GetValue(target); }
    static void Call(object target, string name) { target.GetType().GetMethod(name, Private).Invoke(target, null); }
    static void Check(bool condition, string name) { if (!condition) throw new Exception(name); passed++; Console.WriteLine("PASS " + name); }
    static void Run(string name, Action action)
    {
        try { action(); }
        catch (Exception ex) { failed++; Console.WriteLine("FAIL " + name + ": " + ex); }
    }
    static void Pump(int milliseconds)
    {
        Stopwatch clock = Stopwatch.StartNew();
        while (clock.ElapsedMilliseconds < milliseconds) { Application.DoEvents(); Thread.Sleep(10); }
    }
    static Settings Manual(bool enabled) { return new Settings { ScheduleMode = 2, IsEnabled = enabled }; }

    [STAThread]
    static int Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Run("disabled startup", delegate
        {
            var display = new Display();
            using (var form = new MainForm(Manual(false), display, s => { }))
            {
                Check(display.Captures == 1 && display.Applies == 0 && display.Restores == 1, "disabled startup never applies approximate daylight ramp");
                Check(!Field<System.Windows.Forms.Timer>(form, "_scheduleTimer").Enabled, "manual mode stops schedule timer");
                Check(!Field<System.Windows.Forms.Timer>(form, "_gammaRecoveryTimer").Enabled, "disabled filter stops recovery timer");
            }
        });
        Run("disable fade", delegate
        {
            var display = new Display();
            using (var form = new MainForm(Manual(true), display, s => { }))
            {
                IntPtr handle = form.Handle;
                Call(form, "ToggleNightMode");
                Pump(1200);
                Check(display.Restores == 1, "disabling ends fade by restoring saved calibration");
                Check(!Field<System.Windows.Forms.Timer>(form, "_fadeTimer").Enabled, "completed fade stops its timer");
            }
        });
        Run("event storm and disposal", delegate
        {
            var display = new Display(); int saves = 0;
            var form = new MainForm(Manual(true), display, s => { saves++; });
            IntPtr handle = form.Handle;
            var timer = Field<System.Windows.Forms.Timer>(form, "_displayRefreshTimer");
            display.Applies = 0;
            for (int i = 0; i < 100; i++) Field<EventHandler>(form, "_displayHandler")(null, EventArgs.Empty);
            Pump(1500);
            Check(display.Applies == 1, "100 display notifications coalesce into one filter update");
            Check(Object.ReferenceEquals(timer, Field<System.Windows.Forms.Timer>(form, "_displayRefreshTimer")) && !timer.Enabled, "display refresh reuses and stops one timer");
            Field<EventHandler>(form, "_displayHandler")(null, EventArgs.Empty);
            Application.DoEvents();
            form.Dispose(); form.Dispose();
            Pump(1400);
            Check(display.Applies == 1 && saves == 1, "pending display refresh is cancelled and settings flush exactly once on disposal");
            Check(Field<System.Windows.Forms.Timer>(form, "_displayRefreshTimer") == null && Field<EventHandler>(form, "_displayHandler") == null && Field<NotifyIcon>(form, "_trayIcon") == null, "disposal releases timer, system subscription and tray icon");
        });
        Run("resume across schedule boundary", delegate
        {
            DateTime now = DateTime.Now;
            var settings = new Settings { ScheduleMode = 1, CustomStartTime = now.AddHours(1).TimeOfDay, CustomEndTime = now.AddHours(2).TimeOfDay };
            var display = new Display();
            using (var form = new MainForm(settings, display, s => { }))
            {
                IntPtr handle = form.Handle;
                Check(!settings.IsEnabled, "schedule starts outside night interval");
                settings.CustomStartTime = now.AddHours(-1).TimeOfDay;
                settings.CustomEndTime = now.AddHours(1).TimeOfDay;
                Field<PowerModeChangedEventHandler>(form, "_powerHandler")(null, new PowerModeChangedEventArgs(PowerModes.Resume));
                Field<EventHandler>(form, "_displayHandler")(null, EventArgs.Empty);
                Pump(1500);
                Check(settings.IsEnabled && display.Applies == 1, "resume recomputes schedule even when followed by display event");
            }
        });
        Run("normal exit", delegate
        {
            int saves = 0; var display = new Display();
            var form = new MainForm(Manual(true), display, s => { saves++; });
            IntPtr handle = form.Handle;
            Call(form, "ExitApplication");
            Check(form.IsDisposed && display.Restores == 1 && saves == 1, "Exit closes normally even when minimize-to-tray is enabled");
        });
        Run("language/theme compatibility", delegate
        {
            foreach (string language in new[] { "tr", "en", "es", "fr", "zh", "ja", "ko" })
            {
                using (var form = new MainForm(new Settings { Language = language, ScheduleMode = 2 }, new Display(), s => { }))
                {
                    var picker = Field<ComboBox>(form, "_themePicker");
                    for (int i = 0; i < 5; i++) { picker.SelectedIndex = i; form.PerformLayout(); }
                }
            }
            Check(true, "all 35 language/theme combinations initialize and dispose");
        });
        Console.WriteLine("RESULT " + passed + " passed, " + failed + " failed; display and settings writes mocked.");
        return failed == 0 ? 0 : 1;
    }
}
