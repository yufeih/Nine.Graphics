namespace Nine.Graphics
{
    using Microsoft.Framework.Runtime;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Interop;

    class Host
    {
        private readonly IApplicationShutdown shutdown;
        private Process guestProcess;

        public Host(IApplicationShutdown shutdown)
        {
            if (shutdown == null) throw new ArgumentNullException(nameof(shutdown));

            this.shutdown = shutdown;
        }

        public void Run(int width, int height)
        {
            var channel = Guid.NewGuid().ToString("N");
            var thread = new Thread(() => RunWindow(width, height));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            new Thread(() => HostGuestProcess(channel)).Start();

            Console.ReadLine();
        }

        private void RunWindow(int width, int height)
        {
            var window = new Window { Topmost = false, Width = width, Height = height };

            window.SourceInitialized += (sender, e) =>
            {
                var hwnd = new WindowInteropHelper(window).Handle;
            };

            window.ShowDialog();

            Environment.Exit(0);
        }

        private void HostGuestProcess(string channel)
        {
            var reloadWatch = new Stopwatch();
            shutdown.ShutdownRequested.Register(() => reloadWatch.Restart());

            var args = Environment.GetCommandLineArgs();
            var ps = new ProcessStartInfo
            {
                FileName = args[0],
                Arguments = $"{ string.Join(" ", args.Skip(1)) } --channel { channel }",
                UseShellExecute = false,
            };

            while (true)
            {
                guestProcess = Process.Start(ps);

                KillChildProcess.AddProcess(guestProcess.Handle);

                guestProcess.WaitForExit();

                // Min time to wait
            }
        }
    }
}
