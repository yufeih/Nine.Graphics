namespace Nine.Graphics
{
    using System;
    using System.Diagnostics;
    using Microsoft.Framework.Runtime;
    using Microsoft.Framework.Runtime.Common.CommandLine;
    using Nine.Injection;

    public class Program
    {
        private readonly Xunit.Runner.Dnx.Program xunitRunner;

        public Program(
            IApplicationEnvironment appEnv, IServiceProvider services, NuGetDependencyResolver nuget)
        {
            GraphicsTest.Setup = container =>
            {
                container.Map(nuget).Map(appEnv);
            };

            xunitRunner = new Xunit.Runner.Dnx.Program(appEnv, services);
        }

        public void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            var app = new CommandLineApplication(throwOnUnexpectedArg: false);
            app.Name = app.FullName = "Nine.Graphics.Test";
            app.HelpOption("-?|--help");

            var width = app.Option("-w|--width <WIDTH>", "Set the width of the rendering window", CommandOptionType.SingleValue);
            var height = app.Option("-h|--height <HEIGHT>", "Set the height of the rendering window", CommandOptionType.SingleValue);
            var delay = app.Option("-d|--delay <SECONDS>", "Set the delay in seconds for each frame", CommandOptionType.SingleValue);
            var repeat = app.Option("-r|--repeat <REPEAT>", "Repeat each frame multiple times", CommandOptionType.SingleValue);
            var hide = app.Option("--hide", "Hide the rendering window", CommandOptionType.NoValue);
            var verify = app.Option("-v|--verify", "Verifies the content of each frame", CommandOptionType.NoValue);

            app.Execute(args);

            if (app.IsShowingInformation) return;

            GraphicsTest.Hide = hide.HasValue();
            GraphicsTest.Verify = verify.HasValue();
            if (repeat.HasValue())
                GraphicsTest.Repeat = Math.Max(1, int.Parse(repeat.Value()));
            if (width.HasValue())
                GraphicsTest.Width = Math.Max(1, int.Parse(width.Value()));
            if (height.HasValue())
                GraphicsTest.Height = Math.Max(1, int.Parse(height.Value()));
            if (delay.HasValue())
                GraphicsTest.Delay = int.Parse(delay.Value());

            xunitRunner.Main(app.RemainingArguments.ToArray());
        }
    }
}
