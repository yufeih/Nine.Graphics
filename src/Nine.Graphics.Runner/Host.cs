namespace Nine.Graphics.Runner
{
    using MemoryMessagePipe;
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
        private readonly string channel = Guid.NewGuid().ToString("N");
        private readonly string[] args = Environment.GetCommandLineArgs();
        private readonly ProcessStartInfo processStart;
        private readonly Stopwatch reloadWatch = new Stopwatch();

        private MemoryMappedFileMessageSender guest;

        private IntPtr hwnd;

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
                hwnd = new WindowInteropHelper(window).EnsureHandle();
                guest.SendMessage(new Message { MessageType = MessageType.AttachWindow, Pointer = hwnd }.ToBytes());
            };

            window.ShowDialog();

            Environment.Exit(0);
        }

        private void StartGuestProcess()
        {
            // TODO: session
            reloadWatch.Restart();
            var guestProcess = Process.Start(processStart);

            ProcessHelper.AddChildProcessToKill(guestProcess.Handle);

            if (hwnd != IntPtr.Zero)
            {
                guest.SendMessage(new Message { MessageType = MessageType.AttachWindow, Pointer = hwnd }.ToBytes());
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
                    reloadWatch.Stop();
                    Console.WriteLine($"Application reloaded in { reloadWatch.ElapsedMilliseconds }ms");
                    break;
            }
        }
    }
}
