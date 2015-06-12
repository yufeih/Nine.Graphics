namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Xunit;

    public class PerformanceTest
    {
        [Fact]
        public unsafe void ways_to_enumerate_struct_array()
        {
            var iterations = 1000;
            var end = 8000;
            var data = new Sprite[10000];
            var slice = new Slice<Sprite>(data);
            var list = new List<Sprite>(data);
            var watch = new Stopwatch();
            watch.Start();

            var sum = 0.0f;
            for (var n = 0; n < iterations; n++)
            {
                for (var i = 0; i < end; i++)
                {
                    sum += data[i].Depth;
                }
            }

            watch.Stop();
            Console.WriteLine($"raw    { watch.ElapsedMilliseconds }ms");

            sum = 0.0f;
            watch.Start();

            for (var n = 0; n < iterations; n++)
            {
                for (var i = 0; i < end; i++)
                {
                    sum += list[i].Depth;
                }
            }

            watch.Stop();
            Console.WriteLine($"list    { watch.ElapsedMilliseconds }ms");

            sum = 0.0f;
            watch.Start();

            for (var n = 0; n < iterations; n++)
            {
                for (var i = 0; i < end; i++)
                {
                    sum += slice.Items[i].Depth;
                }
            }

            watch.Stop();
            Console.WriteLine($"slice   { watch.ElapsedMilliseconds }ms");

            sum = 0.0f;
            watch.Start();

            for (var n = 0; n < iterations; n++)
            {
                fixed (Sprite* ptr = data)
                {
                    Sprite* cur = ptr;
                    for (var i = 0; i < end; i++)
                    {
                        sum += (cur + i)->Depth;
                    }
                }
            }

            watch.Stop();
            Console.WriteLine($"ptr +  { watch.ElapsedMilliseconds }ms");

            sum = 0.0f;
            watch.Start();

            for (var n = 0; n < iterations; n++)
            {
                fixed (Sprite* ptr = data)
                {
                    Sprite* cur = ptr;
                    for (var i = 0; i < end; i++)
                    {
                        sum += cur->Depth;
                        cur++;
                    }
                }
            }

            watch.Stop();
            Console.WriteLine($"ptr += { watch.ElapsedMilliseconds }ms");
        }
    }
}
