namespace Nine.Graphics.Runner
{
    using Microsoft.Framework.Runtime;
    using Microsoft.Framework.Runtime.Common.CommandLine;
    using System;
    using System.Diagnostics;

    public class Program
    {
        private readonly IApplicationShutdown shutdown;
        private readonly IServiceProvider serviceProvider;

        public Program(IApplicationShutdown shutdown, IServiceProvider serviceProvider)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            if (shutdown == null) throw new ArgumentNullException(nameof(shutdown));
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            this.shutdown = shutdown;
            this.serviceProvider = serviceProvider;
        }

        public void Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false);
            app.Name = app.FullName = "Nine.Graphics.Test";
            app.HelpOption("-?|--help");

            var width = app.Option("--width <WIDTH>", "Set the width of the host window", CommandOptionType.SingleValue);
            var height = app.Option("--height <HEIGHT>", "Set the height of the host window", CommandOptionType.SingleValue);
            var topMost = app.Option("--pin", "Enables the host window to be top most", CommandOptionType.NoValue);
            var channel = app.Option("--channel <CHANNEL>", "", CommandOptionType.SingleValue);

            app.Execute(args);

            if (app.IsShowingInformation)
            {
                return;
            }
            
            if (!channel.HasValue())
            {
                new Host(shutdown, serviceProvider).Run(
                    width.HasValue() ? int.Parse(width.Value()) : 1024,
                    width.HasValue() ? int.Parse(height.Value()) : 768,
                    topMost.HasValue(),
                    app.RemainingArguments.ToArray());
            }
            else
            {
                new Guest(channel.Value(), shutdown, serviceProvider).Run(app.RemainingArguments.ToArray());
            }
        }
    }
}
