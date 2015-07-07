namespace Nine.Graphics.Runner
{
    using MemoryMessagePipe;
    using Microsoft.Framework.Runtime;
    using Microsoft.Framework.Runtime.Common;
    using System;
    using System.Diagnostics;

    class Guest : IHostWindow, IServiceProvider
    {
        private readonly IApplicationShutdown shutdown;
        private readonly IServiceProvider serviceProvider;
        private MemoryMappedFileMessageSender host;
        private IntPtr childWindow;
        private IntPtr parentWindow;
        private bool childAttached;
        private int hostWidth;
        private int hostHeight;

        public Guest(IApplicationShutdown shutdown, IServiceProvider serviceProvider)
        {
            this.shutdown = shutdown;
            this.serviceProvider = serviceProvider;
        }

        public void Run(string channel, string[] args)
        {
            shutdown.ShutdownRequested.Register(() =>
            {
                host.SendMessage(new Message { MessageType = MessageType.GuestShutdown }.ToBytes());
            });

            var hostListener = new MemoryMappedFileMessageReceiver(channel);
            hostListener.ReceiveMessage((bytes, count) => OnMessage(Message.FromBytes(bytes, count)));
            host = new MemoryMappedFileMessageSender(channel + "*");

            var appEnv = (IApplicationEnvironment)serviceProvider.GetService(typeof(IApplicationEnvironment));
            var accessor = (IAssemblyLoadContextAccessor)serviceProvider.GetService(typeof(IAssemblyLoadContextAccessor));
            var assembly = accessor.Default.Load(appEnv.ApplicationName);

            EntryPointExecutor.Execute(assembly, args, this);
        }

        private void OnMessage(Message message)
        {
            switch (message.MessageType)
            {
                case MessageType.HostWindow:
                    parentWindow = message.Pointer;
                    Debug.Assert(parentWindow != IntPtr.Zero);
                    if (childWindow != IntPtr.Zero && !childAttached)
                    {
                        childAttached = true;
                        hostWidth = message.Width;
                        hostHeight = message.Height;
                        WindowHelper.EmbedWindow(childWindow, parentWindow);
                        WindowHelper.Resize(childWindow, message.Width, message.Height);
                        host.SendMessage(new Message { MessageType = MessageType.GuestWindowAttached }.ToBytes());
                    }
                    break;
                case MessageType.HostResize:
                    hostWidth = message.Width;
                    hostHeight = message.Height;
                    if (childWindow != IntPtr.Zero)
                    {
                        WindowHelper.Resize(childWindow, hostWidth, hostHeight);
                    }
                    break;
            }
        }

        public void Attach(IntPtr childWindow)
        {
            if (childWindow == IntPtr.Zero) throw new ArgumentException(nameof(childWindow));
            if (this.childWindow != IntPtr.Zero) throw new InvalidOperationException("A child window is already attached.");

            this.childWindow = childWindow;

            if (parentWindow != IntPtr.Zero && !childAttached)
            {
                childAttached = true;
                WindowHelper.EmbedWindow(childWindow, parentWindow);
                WindowHelper.Resize(childWindow, hostWidth, hostHeight);
                host.SendMessage(new Message { MessageType = MessageType.GuestWindowAttached }.ToBytes());
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
    }
}
