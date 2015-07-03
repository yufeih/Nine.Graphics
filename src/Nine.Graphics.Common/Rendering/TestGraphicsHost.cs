#if DX
namespace Nine.Graphics.DirectX
#else
namespace Nine.Graphics.OpenGL
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using Nine.Imaging;
    using Nine.Graphics.Rendering;

    public class GraphicsTestFailedException : Exception
    {
        public GraphicsTestFailedException(string message) : base(message) { }
    }

    /// <remarks>
    /// Enables a couple of key graphics testing scenarios:
    /// 
    /// CORRECTNESS:
    ///   - The image should be idential to an expected image.
    ///     The expected image is usually pre-rendered and verified manually.
    ///   - The same input state should always produce the same result between 2 frames.
    ///   - The same input state should produce nearly idential images between different renderers.
    ///   - The renderer should only read the input state and never alter the state.
    /// 
    /// PERFORMANCE:
    ///   - The input state is rendered multiple times to abtain time info.
    ///   - The time info is compared against a baseline to warn if something become drastically slower.
    ///   - The time info is compared against multiple renderers for comparison.
    /// 
    /// DEBUGGING:
    ///   - Be able to render a frame to an image.
    ///   - Be able to render a frame to the output window.
    /// 
    /// </remarks>
    public sealed partial class TestGraphicsHost : ITestGraphicsHost
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Duration = 1000;
        public readonly string OutputPath = "TestResults";

        private readonly byte[] framePixelsA;
        private readonly byte[] framePixelsB;
        private readonly Stopwatch watch = new Stopwatch();
        private readonly Dictionary<string, int> frameCounters = new Dictionary<string, int>();

        public bool DrawFrame(Action<int, int> draw) => DrawFrame(draw, 1);
        public bool DrawFrame(Action<int, int> draw, float epsilon, [CallerMemberName]string frameName = null)
        {
            var frameIdentifier = GetFrameIdentifier(frameName);

            CompareTwoFrames(draw, epsilon);
            CompareAgainstExpectedImageIfExists(framePixelsA, epsilon, frameIdentifier);
            ComparePerformance(draw, frameName);

            return false;
        }

        private void CompareTwoFrames(Action<int, int> draw, float epsilon)
        {
            PlatformBeginFrame();
            draw(Width, Height);
            PlatformEndFrame(framePixelsA);

            PlatformBeginFrame();
            draw(Width, Height);
            PlatformEndFrame(framePixelsB);

            CompareImage(framePixelsA, framePixelsB, epsilon);
        }

        private object GetFrameIdentifier(string frameName)
        {
            int counter;
            if (!frameCounters.TryGetValue(frameName, out counter))
            {
                frameCounters[frameName] = 0;
            }

            frameCounters[frameName] = ++counter;

            return $"{ GetType().Name }/{ frameName }" + (counter > 0 ? $"-{ counter }" : "");
        }

        private void CompareAgainstExpectedImageIfExists(byte[] pixels, float epsilon, object frameIdentifier)
        {
            var expectedFile = $"{ OutputPath }/{ frameIdentifier }.png";

            if (File.Exists(expectedFile))
            {
                using (var expectedStream = File.OpenRead(expectedFile))
                {
                    var expectedImage = new Image(expectedStream);
                    CompareImage(expectedImage.Pixels, pixels, epsilon);
                }
            }
        }

        private void CompareImage(byte[] expected, byte[] actual, float epsilon)
        {
            if (expected.Length != actual.Length)
            {
                throw new GraphicsTestFailedException("Image size does not equal");
            }

            var diff = 0;

            for (var i = 0; i < expected.Length; i++)
            {
                diff += Math.Abs(expected[i] - actual[i]);
            }

            if (diff > epsilon * expected.Length)
            {
                throw new GraphicsTestFailedException("Images are different then the expected size does not equal");
            }
        }

        private void ComparePerformance(Action<int, int> draw, string frameName)
        {
            watch.Restart();

            var frameCount = 0;

            while (watch.Elapsed.TotalSeconds < Duration)
            {
                PlatformBeginFrame();
                draw(Width, Height);
                PlatformEndFrame(null);
                frameCount++;
            }

            watch.Stop();

            SaveAndVerifyPerf(frameCount, watch.Elapsed.TotalMilliseconds, frameName);
        }

        private void SaveAndVerifyPerf(int count, double ms, string frameName)
        {
            var previousRunFile = $"{ OutputPath }/{frameName}.perf.txt";
            var previousTime = File.Exists(previousRunFile) ? double.Parse(File.ReadAllText(previousRunFile)) : 99999999;
            var previousFps = 1000 * count / previousTime;

            var fps = 1000 * count / ms;
            var isRunningSlower = ms > previousTime * 1.25;

            var color = Console.ForegroundColor;
            var highlight = isRunningSlower ? ConsoleColor.Red : ConsoleColor.DarkGreen;

            Console.Write(frameName);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" finished { count } frames in ");
            Console.ForegroundColor = highlight;
            Console.Write(ms.ToString("N4"));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (isRunningSlower) Console.Write($"({ previousTime.ToString("N4") })");
            Console.Write(" ms, ");
            Console.ForegroundColor = highlight;
            Console.Write(fps.ToString("N4"));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (isRunningSlower) Console.Write($"({ previousFps.ToString("N4") })");
            Console.WriteLine(" fps");
            Console.ForegroundColor = color;

            File.WriteAllText(previousRunFile, ms.ToString());
        }
    }
}
