namespace Nine.Graphics.Runner
{
    using MemoryMessagePipe;
    using Microsoft.Framework.Runtime;
    using Microsoft.Framework.Runtime.Common;
    using System;

    class Guest : IHostWindow, IServiceProvider
    {
        private readonly IApplicationShutdown shutdown;
        private readonly IServiceProvider serviceProvider;
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
            Console.WriteLine("Application reloaded in 100ms");

            shutdown.ShutdownRequested.Register(() =>
            {
                // TODO: SendMessage has to be called in the same thread ???
                Console.WriteLine("shutting down...");
                host.SendMessage(new Message { MessageType = MessageType.GuestShutdown }.ToBytes());
                Environment.Exit(0);
            });

            var hostListener = new MemoryMappedFileMessageReceiver(channel);
            hostListener.ReceiveMessage((bytes, count) => OnMessage(Message.FromBytes(bytes, count)));
            host = new MemoryMappedFileMessageSender(channel + "*");

            var appEnv = (IApplicationEnvironment)serviceProvider.GetService(typeof(IApplicationEnvironment));
            var accessor = (IAssemblyLoadContextAccessor)serviceProvider.GetService(typeof(IAssemblyLoadContextAccessor));
            var assembly = accessor.Default.Load(appEnv.ApplicationName);

            EntryPointExecutor.Execute(assembly, args, this);

            Console.ReadLine();
        }

        private void OnMessage(Message message)
        {
            switch (message.MessageType)
            {
                case MessageType.AttachWindow:
                    if (childWindow != IntPtr.Zero)
                    {
                        parentWindow = message.Pointer;
                        WindowHelper.EmbedWindow(childWindow, parentWindow);
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
