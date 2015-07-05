namespace Nine.Graphics
{
    using System;
    using System.Diagnostics;
    using Microsoft.Framework.Runtime;
    using Microsoft.Framework.Runtime.Common.CommandLine;
    using Nine.Injection;
    using Nine.Graphics.Runner;

    public class Program
    {
        private readonly Xunit.Runner.Dnx.Program xunitRunner;

        public Program(
            IApplicationEnvironment appEnv, IServiceProvider services, NuGetDependencyResolver nuget,
            IHostWindow hostWindow)
        {
            GraphicsTest.Setup = container =>
            {
                container.Map(nuget).Map(appEnv);
            };

            Console.WriteLine(hostWindow?.GetType().FullName ?? ".....");

            xunitRunner = new Xunit.Runner.Dnx.Program(appEnv, services);
        }

        public void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            if (!CheckIfGacIsNotPatched()) return;

            var app = new CommandLineApplication(throwOnUnexpectedArg: false);
            app.Name = app.FullName = "Nine.Graphics.Test";
            app.HelpOption("-?|--help");

            var width = app.Option("-w|--width <WIDTH>", "Set the width of the rendering window", CommandOptionType.SingleValue);
            var height = app.Option("-h|--height <HEIGHT>", "Set the height of the rendering window", CommandOptionType.SingleValue);
            var test = app.Option("--test|-t", "Runs test cases", CommandOptionType.NoValue);

            app.Execute(args);

            if (app.IsShowingInformation) return;

            GraphicsTest.IsTest = test.HasValue();

            if (width.HasValue())
                GraphicsTest.Width = Math.Max(1, int.Parse(width.Value()));
            if (height.HasValue())
                GraphicsTest.Height = Math.Max(1, int.Parse(height.Value()));

            xunitRunner.Main(app.RemainingArguments.ToArray());
        }

        private bool CheckIfGacIsNotPatched()
        {
            try
            {
                UseMatrix4x4();
                return true;
            }
            catch (MissingMethodException)
            {
                Trace.TraceError(
                    "Please patch System.Numerics.Vectors.dll using the following command:\n" +
                    @"    gacutil / i % DNX_HOME %\packages\System.Numerics.Vectors\4.0.0\lib\win8\System.Numerics.Vectors.dll / f" + "\n\n" +
                    @"See https://github.com/dotnet/corefx/issues/313 for details");
                return false;
            }
        }

        private void UseMatrix4x4()
        {
            System.Numerics.Matrix4x4.CreateOrthographicOffCenter(0, 1, 1, 0, 0, 1);
        }
    }
}
