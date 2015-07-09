namespace Nine.Graphics.Runner
{
    using Microsoft.Framework.Runtime;
    using Microsoft.Framework.Runtime.Common;
    using SharedMemory;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading;
    using System.IO;
    using System.IO.MemoryMappedFiles;

    class Guest : IHostWindow, ISharedMemory, IServiceProvider
    {
        private readonly string channel;
        private readonly IApplicationShutdown shutdown;
        private readonly IServiceProvider serviceProvider;
        private readonly CircularBuffer guestBuffer;
        private readonly CircularBuffer hostBuffer;
        private readonly ConcurrentDictionary<string, MemoryMappedFile> mmfMap = new ConcurrentDictionary<string, MemoryMappedFile>();

        private IntPtr childWindow;
        private IntPtr parentWindow;
        private bool childAttached;

        public Guest(string channel, IApplicationShutdown shutdown, IServiceProvider serviceProvider)
        {
            this.channel = channel;
            this.shutdown = shutdown;
            this.serviceProvider = serviceProvider;
            this.guestBuffer = new CircularBuffer(channel);
            this.hostBuffer = new CircularBuffer(channel + "*");
        }

        public void Run(string[] args)
        {
            if (!Debugger.IsAttached)
            {
                shutdown.ShutdownRequested.Register(() =>
                {
                    var message = new Message { MessageType = MessageType.GuestShutdown };
                    hostBuffer.Write(ref message);
                });
            }

            new Thread(ListenHostEvents) { Name = "Host Listener" }.Start();

            var appEnv = (IApplicationEnvironment)serviceProvider.GetService(typeof(IApplicationEnvironment));
            var accessor = (IAssemblyLoadContextAccessor)serviceProvider.GetService(typeof(IAssemblyLoadContextAccessor));
            var assembly = accessor.Default.Load(appEnv.ApplicationName);

            EntryPointExecutor.Execute(assembly, args, this);
        }

        private void OnMessage(ref Message message)
        {
            switch (message.MessageType)
            {
                case MessageType.HostWindow:
                    parentWindow = message.Pointer;
                    Debug.Assert(parentWindow != IntPtr.Zero);
                    TryAttach();
                    break;
                case MessageType.HostResize:
                    if (childWindow != IntPtr.Zero)
                    {
                        WindowHelper.Resize(childWindow, message.Width, message.Height);
                    }
                    break;
            }
        }

        public void Attach(IntPtr childWindow)
        {
            if (childWindow == IntPtr.Zero) throw new ArgumentException(nameof(childWindow));
            if (this.childWindow != IntPtr.Zero) throw new InvalidOperationException("A child window is already attached.");

            this.childWindow = childWindow;

            TryAttach();
        }

        private void TryAttach()
        {
            if (parentWindow != IntPtr.Zero && childWindow != IntPtr.Zero && !childAttached)
            {
                childAttached = true;
                WindowHelper.EmbedWindow(childWindow, parentWindow);

                var message = new Message { MessageType = MessageType.GuestWindowAttached };
                hostBuffer.Write(ref message);
            }
        }

        private void ListenHostEvents()
        {
            while (true)
            {
                Message message;
                if (guestBuffer.Read(out message, Timeout.Infinite) > 0)
                {
                    OnMessage(ref message);
                }
            }
        }

        public unsafe Stream GetStream(string name, int sizeInBytes)
        {
            var mmf = mmfMap.GetOrAdd(name, key =>
            {
                if (sizeInBytes <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
                }

                var message = new Message(MessageType.GuestRequestSharedMemory).WithName(name);

                lock (mmfMap)
                {
                    hostBuffer.Write(ref message);

                    return MemoryMappedFile.CreateNew(channel + name, sizeInBytes);
                }
            });

            return mmf.CreateViewStream();
        }

        public bool Remove(string name)
        {
            MemoryMappedFile mmf;
            if (mmfMap.TryRemove(name, out mmf))
            {
                var message = new Message(MessageType.GuestRemoveSharedMemory).WithName(name);
                hostBuffer.Write(ref message);
                mmf.Dispose();
                return true;
            }

            return false;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IHostWindow) || serviceType == typeof(ISharedMemory))
            {
                return this;
            }
            return serviceProvider.GetService(serviceType);
        }
    }
}
