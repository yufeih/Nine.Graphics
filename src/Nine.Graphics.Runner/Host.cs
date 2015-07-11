namespace Nine.Graphics.Runner
{
    using Microsoft.Framework.Runtime;
    using SharedMemory;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.IO.MemoryMappedFiles;

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
        private readonly ConcurrentDictionary<string, MemoryMappedFile> mmfMap = new ConcurrentDictionary<string, MemoryMappedFile>();

        private HostForm form;
        private Process guestProcess;

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
                RedirectStandardError = true,
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
            form = new HostForm(topMost) { Width = width, Height = height };
            form.SetText("Loading");
            form.HandleCreated += (sender, e) => SendHostWindow();
            form.SizeChanged += (sender, e) => SendHostResize();
            form.ShowDialog();

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
                var process = guestProcess;
                if (process != null)
                {
                    process.Kill();
                }

                process = Process.Start(processStart);
                ProcessHelper.AddChildProcessToKill(process.Handle);
                WatchProcessExit(process);

                guestProcess = process;
            }

            if (form != null && form.Handle != IntPtr.Zero)
            {
                SendHostWindow();
            }
        }

        private unsafe void OnMessage(ref Message message)
        {
            switch (message.MessageType)
            {
                case MessageType.GuestShutdown:
                    StartGuestProcess();
                    break;
                case MessageType.GuestWindowAttached:
                    SendHostResize();
                    reloadWatch.Stop();
                    var runtimeOptions = (IRuntimeOptions)serviceProvider.GetService(typeof(IRuntimeOptions));
                    if (runtimeOptions.CompilationServerPort.HasValue)
                    {
                        Console.WriteLine($"Application loaded in { reloadWatch.ElapsedMilliseconds }ms using compilation server port { runtimeOptions.CompilationServerPort }");
                    }
                    else
                    {
                        Console.WriteLine($"Application loaded in { reloadWatch.ElapsedMilliseconds }ms");
                    }
                    break;
                case MessageType.GuestRequestSharedMemory:
                    mmfMap.GetOrAdd(channel + message.GetName(), MemoryMappedFile.OpenExisting);
                    break;
                case MessageType.GuestRemoveSharedMemory:
                    MemoryMappedFile mmf;
                    if (mmfMap.TryRemove(channel + message.GetName(), out mmf)) mmf.Dispose();
                    break;
            }
        }

        private void WatchProcessExit(Process process)
        {
            new Thread(() =>
            {
                process.WaitForExit();

                var error = process.StandardError.ReadToEnd();
                form.SetText(error);
                Console.Error.Write(error);

                if (process == guestProcess)
                {
                    guestProcess = null;
                }
            }) { Name = "Guest Watch" }.Start();
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
            if (guestProcess != null)
            {
            var message = new Message
            {
                MessageType = MessageType.HostResize,
                    Width = form.Width,
                    Height = form.Height,
            };

            guestBuffer.Write(ref message);
        }
        }

        private void SendHostWindow()
        {
            if (guestProcess != null)
            {
            var message = new Message
            {
                MessageType = MessageType.HostWindow,
                    Pointer = form.Handle
            };

            guestBuffer.Write(ref message);
        }
    }
}
}
