namespace Nine.Graphics.Runner
{
    using Microsoft.Framework.Runtime;
    using Microsoft.Framework.Runtime.Common.CommandLine;
    using System;
    using System.Diagnostics;

    public class Program
    {
        private readonly IApplicationShutdown shutdown;

        public Program(IApplicationShutdown shutdown)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            Debug.Listeners.Add(new ConsoleTraceListener());

            if (shutdown == null) throw new ArgumentNullException(nameof(shutdown));

            this.shutdown = shutdown;
        }

        public void Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false);
            app.Name = app.FullName = "Nine.Graphics.Test";
            app.HelpOption("-?|--help");

            var width = app.Option("--width <WIDTH>", "Set the width of the rendering window", CommandOptionType.SingleValue);
            var height = app.Option("--height <HEIGHT>", "Set the height of the rendering window", CommandOptionType.SingleValue);
            var channel = app.Option("--channel <CHANNEL>", "", CommandOptionType.SingleValue);

            app.Execute(args);

            if (app.IsShowingInformation)
            {
                return;
            }
            
            if (!channel.HasValue())
            {
                new Host(shutdown).Run(
                    width.HasValue() ? int.Parse(width.Value()) : 1024,
                    width.HasValue() ? int.Parse(height.Value()) : 768);
            }
            else
            {
                new Guest(shutdown).Run(channel.Value());
            }
        }
    }
}
