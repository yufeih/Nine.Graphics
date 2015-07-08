namespace Nine.Graphics.Runner
{
    using Microsoft.Framework.Runtime;
    using Microsoft.Framework.Runtime.Common;
    using SharedMemory;
    using System;
    using System.Diagnostics;
    using System.Threading;

    class Guest : IHostWindow, IServiceProvider
    {
        private readonly IApplicationShutdown shutdown;
        private readonly IServiceProvider serviceProvider;
        private readonly CircularBuffer guestBuffer;
        private readonly CircularBuffer hostBuffer;

        private IntPtr childWindow;
        private IntPtr parentWindow;
        private bool childAttached;

        public Guest(string channel, IApplicationShutdown shutdown, IServiceProvider serviceProvider)
        {
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

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IHostWindow))
            {
                return this;
            }
            return serviceProvider.GetService(serviceType);
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
    }
}
