namespace Nine.Graphics.Runner
{
    using MemoryMessagePipe;
    using Microsoft.Framework.Runtime;
    using Microsoft.Framework.Runtime.Common;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;

    class Guest : IHostWindow, IServiceProvider
    {
        private readonly IApplicationShutdown shutdown;
        private readonly IServiceProvider serviceProvider;
        private readonly Application app = new Application();
        private MemoryMappedFileMessageSender host;
        private IntPtr childWindow;
        private IntPtr parentWindow;

        public Guest(IApplicationShutdown shutdown, IServiceProvider serviceProvider)
        {
            this.shutdown = shutdown;
            this.serviceProvider = serviceProvider;
        }

        public void Run(string channel, string[] args)
        {
            app.Startup += (sender, e) => StartApp(channel, args);
            app.Run();
        }

        private void StartApp(string channel, string[] args)
        {
            Debug.Assert(SynchronizationContext.Current != null);

            var syncContext = SynchronizationContext.Current;

            shutdown.ShutdownRequested.Register(() =>
            {
                syncContext.Post(_ =>
                {
                    host.SendMessage(new Message { MessageType = MessageType.GuestShutdown }.ToBytes());
                    app.Shutdown();
                }, null);
            });

            var hostListener = new MemoryMappedFileMessageReceiver(channel);
            hostListener.ReceiveMessage((bytes, count) =>
            {
                syncContext.Post(_ =>
                {
                    OnMessage(Message.FromBytes(bytes, count));
                }, null);
            });
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
                case MessageType.AttachWindow:
                    parentWindow = message.Pointer;
                    if (childWindow != IntPtr.Zero)
                    {
                        WindowHelper.EmbedWindow(childWindow, parentWindow);
                        host.SendMessage(new Message { MessageType = MessageType.GuestWindowAttached }.ToBytes());
                    }
                    break;
            }
        }

        public void Attach(IntPtr childWindow)
        {
            this.childWindow = childWindow;
            if (parentWindow != IntPtr.Zero)
            {
                WindowHelper.EmbedWindow(childWindow, parentWindow);
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
