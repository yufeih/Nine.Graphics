namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Xunit;

    class StructEnumerationTest
    {
        [Fact]
        public unsafe void ways_to_enumerate_struct_array()
        {
            var iterations = 1000;
            var data = new Sprite[10000];
            var slice = new Slice<Sprite>(data);
            var list = new List<Sprite>(data);
            var count = data.Length;

            var sum = 0.0f;
            var watch = new Stopwatch();

            for (int nn = 0; nn < 2; nn++)
            {
                Console.WriteLine();
                watch.Restart();

                sum = 0.0f;
                for (var n = 0; n < iterations; n++)
                {
                    for (var i = 0; i < count; i++)
                    {
                        sum += data[i].Depth;
                        sum += data[i].Depth;
                        sum += data[i].Depth;
                        sum += data[i].Depth;
                    }
                }

                watch.Stop();
                Console.WriteLine($"raw    { watch.ElapsedMilliseconds }ms");

                watch.Restart();

                sum = 0.0f;
                for (var n = 0; n < iterations; n++)
                {
                    for (var i = 0; i < data.Length; i++)
                    {
                        sum += data[i].Depth;
                        sum += data[i].Depth;
                        sum += data[i].Depth;
                        sum += data[i].Depth;
                    }
                }

                watch.Stop();
                Console.WriteLine($"raw L  { watch.ElapsedMilliseconds }ms");

                sum = 0.0f;
                watch.Restart();

                for (var n = 0; n < iterations; n++)
                {
                    for (var i = 0; i < count; i++)
                    {
                        sum += list[i].Depth;
                        sum += list[i].Depth;
                        sum += list[i].Depth;
                        sum += list[i].Depth;
                    }
                }

                watch.Stop();
                Console.WriteLine($"list    { watch.ElapsedMilliseconds }ms");

                sum = 0.0f;
                watch.Restart();

                for (var n = 0; n < iterations; n++)
                {
                    for (var i = 0; i < count; i++)
                    {
                        sum += slice[i].Depth;
                        sum += slice[i].Depth;
                        sum += slice[i].Depth;
                        sum += slice[i].Depth;
                    }
                }

                watch.Stop();
                Console.WriteLine($"slice   { watch.ElapsedMilliseconds }ms");

                sum = 0.0f;
                watch.Restart();

                for (var n = 0; n < iterations; n++)
                {
                    for (var i = 0; i < slice.Count; i++)
                    {
                        sum += slice.Items[i].Depth;
                        sum += slice.Items[i].Depth;
                        sum += slice.Items[i].Depth;
                        sum += slice.Items[i].Depth;
                    }
                }

                watch.Stop();
                Console.WriteLine($"slice I { watch.ElapsedMilliseconds }ms");

                sum = 0.0f;
                watch.Restart();

                for (var n = 0; n < iterations; n++)
                {
                    for (var i = 0; i < slice.Count; i++)
                    {
                        var item = slice.Items[i];
                        sum += item.Depth;
                        sum += item.Depth;
                        sum += item.Depth;
                        sum += item.Depth;
                    }
                }

                watch.Stop();
                Console.WriteLine($"slice C { watch.ElapsedMilliseconds }ms");

                sum = 0.0f;
                watch.Restart();

                for (var n = 0; n < iterations; n++)
                {
                    fixed (Sprite* ptr = data)
                    {
                        Sprite* cur = ptr;
                        for (var i = 0; i < count; i++)
                        {
                            sum += (cur + i)->Depth;
                            sum += (cur + i)->Depth;
                            sum += (cur + i)->Depth;
                            sum += (cur + i)->Depth;
                        }
                    }
                }

                watch.Stop();
                Console.WriteLine($"ptr +  { watch.ElapsedMilliseconds }ms");

                sum = 0.0f;
                watch.Restart();

                for (var n = 0; n < iterations; n++)
                {
                    fixed (Sprite* ptr = data)
                    {
                        Sprite* cur = ptr;
                        for (var i = 0; i < count; i++)
                        {
                            sum += cur->Depth;
                            sum += cur->Depth;
                            sum += cur->Depth;
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
}
