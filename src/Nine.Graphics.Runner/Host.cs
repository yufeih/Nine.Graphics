namespace Nine.Graphics.Runner
{
    using Microsoft.Framework.Runtime;
    using SharedMemory;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Interop;

    class Host
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IApplicationShutdown shutdown;
        private readonly string channel = Guid.NewGuid().ToString("N");
        private readonly string[] args = Environment.GetCommandLineArgs();
        private readonly ProcessStartInfo processStart;
        private readonly CircularBuffer guestBuffer;
        private readonly CircularBuffer hostBuffer;
        private readonly Stopwatch reloadWatch = new Stopwatch();

        private Window window;
        private Process guestProcess;
        private IntPtr hwnd;

        public Host(IApplicationShutdown shutdown, IServiceProvider serviceProvider)
        {
            this.shutdown = shutdown;
            this.serviceProvider = serviceProvider;
            this.guestBuffer = new CircularBuffer(channel, 100, Marshal.SizeOf(typeof(Message)));
            this.hostBuffer = new CircularBuffer(channel + "*", 100, Marshal.SizeOf(typeof(Message)));
            this.processStart = new ProcessStartInfo
            {
                FileName = args[0],
                Arguments = $"{ string.Join(" ", args.Skip(1)) } --channel { channel }",
                UseShellExecute = false,
                WorkingDirectory = Environment.CurrentDirectory,
            };
        }

        public void Run(int width, int height, bool topMost, string[] args)
        {
            StartGuestProcess(args);

            var uiThread = new Thread(() => RunWindow(width, height, topMost));
            uiThread.Name = "Host UI";
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();

            new Thread(ListenGuestEvents) { Name = "Guest Listener" }.Start();

            Console.ReadLine();
        }

        private void RunWindow(int width, int height, bool topMost)
        {
            window = new Window { Topmost = topMost, Width = width, Height = height };
            window.SourceInitialized += (sender, e) =>
            {
                hwnd = new WindowInteropHelper(window).EnsureHandle();
                SendHostWindow();
            };

            window.SizeChanged += (sender, e) => SendHostResize();
            window.ShowDialog();

            Environment.Exit(0);
        }

        private void StartGuestProcess(params string[] args)
        {
            reloadWatch.Restart();

            if (Debugger.IsAttached)
            {
                new Thread(() => new Guest(channel, shutdown, serviceProvider).Run(args)) { Name = "Guest UI" }.Start();
            }
            else
            {
                if (guestProcess != null)
                {
                    guestProcess.Kill();
                }

                guestProcess = Process.Start(processStart);

                ProcessHelper.AddChildProcessToKill(guestProcess.Handle);
            }

            if (hwnd != IntPtr.Zero)
            {
                SendHostWindow();
            }
        }

        private void OnMessage(ref Message message)
        {
            switch (message.MessageType)
            {
                case MessageType.GuestShutdown:
                    StartGuestProcess();
                    break;
                case MessageType.GuestWindowAttached:
                    SendHostResize();
                    reloadWatch.Stop();
                    Console.WriteLine($"Application reloaded in { reloadWatch.ElapsedMilliseconds }ms");
                    break;
            }
        }

        private void ListenGuestEvents()
        {
            while (true)
            {
                Message message;
                if (hostBuffer.Read(out message, Timeout.Infinite) > 0)
                {
                    OnMessage(ref message);
                }
            }
        }

        private void SendHostResize()
        {
            var message = new Message
            {
                MessageType = MessageType.HostResize,
                Width = (int)window.ActualWidth,
                Height = (int)window.ActualHeight,
            };

            guestBuffer.Write(ref message);
        }

        private void SendHostWindow()
        {
            var message = new Message
            {
                MessageType = MessageType.HostWindow,
                Pointer = hwnd
            };

            guestBuffer.Write(ref message);
        }
    }
}
