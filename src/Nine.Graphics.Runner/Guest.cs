namespace Nine.Graphics
{
    using Microsoft.Framework.Runtime;
    using System;

    class Guest
    {
        private readonly IApplicationShutdown shutdown;

        public Guest(IApplicationShutdown shutdown)
        {
            if (shutdown == null) throw new ArgumentNullException(nameof(shutdown));

            this.shutdown = shutdown;
        }
        
        public void Run(string channel)
        {
            Console.WriteLine("Application reloaded in 100ms");

            shutdown.ShutdownRequested.Register(() => Environment.Exit(0));

            Console.ReadLine();
        }
    }
}
