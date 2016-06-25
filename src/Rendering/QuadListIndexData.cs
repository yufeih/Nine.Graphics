namespace Nine.Graphics.Rendering
{
    using System;
    using System.Diagnostics;

    static class QuadListIndexData
    {
        private static ushort[] s_indexData;

        public static ushort[] GetIndices(int quadCount)
        {
            Debug.Assert(quadCount >= 0);

            var indexData = s_indexData;

            if (indexData == null || quadCount * 6 > indexData.Length)
            {
                var start = 0;

                if (indexData != null)
                {
                    start = indexData.Length / 6;
                }

                Array.Resize(ref indexData, quadCount * 6);

                PopulateIndex(indexData, start, quadCount);

                s_indexData = indexData;
            }

            return indexData;
        }

        private static void PopulateIndex(ushort[] indexData, int start, int quadCount)
        {
            for (var i = start; i < quadCount; i++)
            {
                indexData[i * 6 + 0] = (ushort)(i * 4);
                indexData[i * 6 + 1] = (ushort)(i * 4 + 1);
                indexData[i * 6 + 2] = (ushort)(i * 4 + 2);

                indexData[i * 6 + 3] = (ushort)(i * 4 + 1);
                indexData[i * 6 + 4] = (ushort)(i * 4 + 3);
                indexData[i * 6 + 5] = (ushort)(i * 4 + 2);
            }
        }
    }
}
