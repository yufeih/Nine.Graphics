namespace Nine.Graphics.Runner
{
    using MemoryMessagePipe;
    using Microsoft.Framework.Runtime;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Interop;

    class Host
    {
        private readonly IApplicationShutdown shutdown;
        private readonly string channel = Guid.NewGuid().ToString("N");
        private readonly string[] args = Environment.GetCommandLineArgs();
        private readonly ProcessStartInfo processStart;
        private readonly Stopwatch reloadWatch = new Stopwatch();

        private MemoryMappedFileMessageSender guest;
        private Process guestProcess;
        private IntPtr hwnd;

        private int width;
        private int height;

        public Host(IApplicationShutdown shutdown)
        {
            this.shutdown = shutdown;
            this.processStart = new ProcessStartInfo
            {
                FileName = args[0],
                Arguments = $"{ string.Join(" ", args.Skip(1)) } --channel { channel }",
                UseShellExecute = false,
                WorkingDirectory = Environment.CurrentDirectory,
            };
        }

        public void Run(int width, int height)
        {
            var guestListener = new MemoryMappedFileMessageReceiver(channel + "*");
            guestListener.ReceiveMessage((bytes, count) => OnMessage(Message.FromBytes(bytes, count)));
            guest = new MemoryMappedFileMessageSender(channel);

            StartGuestProcess();

            var thread = new Thread(() => RunWindow(width, height));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            Console.ReadLine();
        }

        private void RunWindow(int width, int height)
        {
            var window = new Window { Topmost = false, Width = width, Height = height };

            window.SourceInitialized += (sender, e) =>
            {
                width = (int)window.ActualWidth;
                height = (int)window.ActualHeight;
                hwnd = new WindowInteropHelper(window).EnsureHandle();
                guest.SendMessage(new Message { MessageType = MessageType.HostWindow, Pointer = hwnd, Width = width, Height = height }.ToBytes());
            };

            window.SizeChanged += (sender, e) =>
            {
                // TODO: Optimize send message
                width = (int)window.ActualWidth;
                height = (int)window.ActualHeight;
                guest.SendMessage(new Message { MessageType = MessageType.HostResize, Width = width, Height = height }.ToBytes());
            };

            window.ShowDialog();

            Environment.Exit(0);
        }

        private void StartGuestProcess()
        {
            reloadWatch.Restart();
            if (guestProcess != null)
            {
                guestProcess.Kill();
            }

            guestProcess = Process.Start(processStart);

            ProcessHelper.AddChildProcessToKill(guestProcess.Handle);

            if (hwnd != IntPtr.Zero)
            {
                guest.SendMessage(new Message { MessageType = MessageType.HostWindow, Pointer = hwnd, Width = width, Height = height }.ToBytes());
            }
        }

        private void OnMessage(Message message)
        {
            switch (message.MessageType)
            {
                case MessageType.GuestShutdown:
                    StartGuestProcess();
                    break;
                case MessageType.GuestWindowAttached:
                    // TODO:
                    // guest.SendMessage(new Message { MessageType = MessageType.HostResize, Width = width, Height = height }.ToBytes());
                    reloadWatch.Stop();
                    Console.WriteLine($"Application reloaded in { reloadWatch.ElapsedMilliseconds }ms");
                    break;
            }
        }
    }
}
