namespace Nine.Graphics.Rendering
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Nine.Imaging;

    public class GraphicsTestFailedException : Exception
    {
        public GraphicsTestFailedException(string message) : base(message) { }
    }

    /// <remarks>
    /// Enables a couple of key graphics testing scenarios:
    /// 
    /// CORRECTNESS:
    ///   - The image should be identical to an expected image.
    ///     The expected image is usually pre-rendered and verified manually.
    ///   - The same input state should always produce the same result between 2 frames.
    ///   - The same input state should produce nearly identical images between different renderers.
    ///   - The renderer should only read the input state and never alter the state.
    /// 
    /// PERFORMANCE:
    ///   - The input state is rendered multiple times to obtain time info.
    ///   - The time info is compared against a baseline to warn if something become drastically slower.
    ///   - The time info is compared against multiple renderers for comparison.
    /// 
    /// DEBUGGING:
    ///   - Be able to render a frame to an image.
    ///   - Be able to render a frame to the output window.
    /// 
    /// </remarks>
    public abstract class TestGraphicsHost : IGraphicsHost
    {
        public readonly string Name;
        public readonly int Width;
        public readonly int Height;

        private readonly float _epsilon;
        private readonly string _outputPath;
        private readonly int _testDuration;
        private readonly byte[] _framePixelsA;
        private readonly byte[] _framePixelsB;

        private readonly Stopwatch _watch = new Stopwatch();
        private readonly Dictionary<string, int> _frameCounters = new Dictionary<string, int>();

        private readonly string _perfFile;
        private readonly Dictionary<string, double> _perf = new Dictionary<string, double>();

        public TestGraphicsHost(string name, int width, int height, int testDuration, float epsilon, string outputPath)
        {
            Name = name;
            Width = width;
            Height = height;
            _epsilon = epsilon;
            _outputPath = outputPath ?? "TestResults";

            _testDuration = testDuration;
            _framePixelsA = new byte[width * height * 4];
            _framePixelsB = new byte[width * height * 4];

            _perfFile = Path.Combine(_outputPath, "perf.log");
            if (File.Exists(_perfFile))
            {
                _perf = File.ReadAllLines(_perfFile)
                            .Select(line => line.Split('\t'))
                            .ToDictionary(k => k[0], v => double.Parse(v[1]));
            }
        }

        public bool DrawFrame(Action<int, int> draw, [CallerMemberName]string frameName = null)
        {
            var frameIdentifier = GetFrameIdentifier(frameName);
            var framePath = Path.Combine(_outputPath, frameIdentifier);

            CompareTwoFrames(draw, framePath + ".png");
            CompareWithExpectedImage(_framePixelsA, framePath + ".png");
            ComparePerformance(draw, frameIdentifier, framePath + ".perf");

            return false;
        }

        private string GetFrameIdentifier(string frameName)
        {
            int counter;
            if (!_frameCounters.TryGetValue(frameName, out counter))
            {
                _frameCounters[frameName] = 0;
            }

            _frameCounters[frameName] = ++counter;

            return $"{frameName}-{counter}-{Name}";
        }

        private void CompareTwoFrames(Action<int, int> draw, string framePath)
        {
            BeginFrame();
            draw(Width, Height);
            EndFrame(_framePixelsA);

            BeginFrame();
            draw(Width, Height);
            EndFrame(_framePixelsB);

            CompareImage(_framePixelsA, _framePixelsB, framePath, true);
        }

        protected abstract void BeginFrame();
        protected abstract void EndFrame(byte[] pixels);

        private void CompareWithExpectedImage(byte[] pixels, string framePath)
        {
            if (File.Exists(framePath))
            {
                var expectedImage = Image.Load(framePath);
                CompareImage(expectedImage.Pixels, pixels, framePath, false);
            }
        }

        private void CompareImage(byte[] expected, byte[] actual, string framePath, bool saveExpected)
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

            if (diff > _epsilon * expected.Length)
            {
                if (saveExpected)
                {
                    SaveImage(expected, framePath, "expected");
                }

                SaveImage(actual, framePath, "actual");
                SaveImage(CreateImageDiff(expected, actual), framePath, "diff");

                throw new GraphicsTestFailedException("Images are different then the expected");
            }

            if (!File.Exists(framePath))
            {
                SaveImage(actual, framePath, null);
            }
        }

        private void SaveImage(byte[] pixels, string path, string tag)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                var extension = Path.GetExtension(path);
                var directory = Path.GetDirectoryName(path);
                var filename = Path.GetFileNameWithoutExtension(path);

                path = Path.Combine(directory, $"{filename}.{tag}{extension}");
            }

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            using (var stream = File.OpenWrite(path))
            {
                var img = new Image(Width, Height, pixels);
                img.SaveAsPng(stream);
            }
        }

        private byte[] CreateImageDiff(byte[] expected, byte[] actual)
        {
            var diff = new byte[expected.Length];

            for (var i = 0; i < expected.Length; i += 4)
            {
                var equals = (expected[i] != actual[i]
                           || expected[i + 1] != actual[i + 1]
                           || expected[i + 2] != actual[i + 2]
                           || expected[i + 3] != actual[i + 3]);

                diff[i] = diff[i + 1] = diff[i + 2] = equals ? byte.MaxValue : byte.MinValue;
                diff[i + 3] = byte.MaxValue;
            }

            return diff;
        }

        private void ComparePerformance(Action<int, int> draw, string frameName, string framePath)
        {
            if (_testDuration <= 0)
            {
                return;
            }

            _watch.Restart();

            var frameCount = 0;

            while (_watch.Elapsed.TotalMilliseconds < _testDuration)
            {
                BeginFrame();
                draw(Width, Height);
                EndFrame(null);
                frameCount++;
            }

            _watch.Stop();

            SaveAndVerifyPerf(frameCount, _watch.Elapsed.TotalMilliseconds, frameName);
        }

        private void SaveAndVerifyPerf(int count, double time, string frameName)
        {
            var previousFps = 0.0;

            _perf.TryGetValue(frameName, out previousFps);

            var fps = 1000 * count / time;
            var isRunningSlower = fps < previousFps * 0.75;

            var color = Console.ForegroundColor;
            var highlight = isRunningSlower ? ConsoleColor.Red : ConsoleColor.DarkGreen;

            Console.Write(frameName);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" finished with ");
            Console.ForegroundColor = highlight;
            Console.Write(fps.ToString("N2"));
            Console.ForegroundColor = isRunningSlower ? ConsoleColor.Red : ConsoleColor.DarkGray;
            Console.WriteLine(" fps " + (fps > previousFps ? '>' : '<') + " " + previousFps.ToString("N2"));
            Console.ForegroundColor = color;

            if (!isRunningSlower)
            {
                _perf[frameName] = fps;

                File.WriteAllLines(_perfFile, _perf.OrderBy(p => p.Key).Select(p => string.Concat(p.Key, "\t",p.Value)));
            }
        }
    }
}
