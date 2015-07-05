namespace Nine.Graphics.Runner
{
    using MemoryMessagePipe;
    using Microsoft.Framework.Runtime;
    using System;

    class Guest : IHostWindow
    {
        private readonly IApplicationShutdown shutdown;
        private MemoryMappedFileMessageSender host;
        private IntPtr childWindow;
        private IntPtr parentWindow;

        public Guest(IApplicationShutdown shutdown)
        {
            if (shutdown == null) throw new ArgumentNullException(nameof(shutdown));

            this.shutdown = shutdown;
        }

        public void Run(string channel)
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
    }
}
