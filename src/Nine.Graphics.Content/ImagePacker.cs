namespace Nine.Graphics.Content
{
    using System;
    using System.Drawing;
    using System.Diagnostics;

    class ImagePacker
    {
        private readonly RectanglePacker packer;
        private readonly int bytesPerPixel;

        public readonly byte[] Pixels;

        public ImagePacker(int width, int height, int bytesPerPixel = 4)
        {
            Debug.Assert(width > 0);
            Debug.Assert(height > 0);
            Debug.Assert(bytesPerPixel > 0);

            this.bytesPerPixel = bytesPerPixel;
            this.packer = new RectanglePacker(width, height);
            this.Pixels = new byte[width * height * bytesPerPixel];
        }

        public ImagePacker Pack(int rectangleWidth, int rectangleHeight, int borderThickness, Action<byte[], int, int> fillPixels)
        {
            Debug.Assert(rectangleWidth + borderThickness * 2 <= packer.PackingAreaWidth);
            Debug.Assert(rectangleHeight + borderThickness * 2 <= packer.PackingAreaHeight);

            Point placement;

            if (packer.TryPack(rectangleWidth, rectangleHeight, borderThickness, out placement))
            {
                fillPixels(Pixels, placement.X, placement.Y);
                return this;
            }

            return new ImagePacker(packer.PackingAreaWidth, packer.PackingAreaHeight, bytesPerPixel)
                .Pack(rectangleWidth, rectangleHeight, borderThickness, fillPixels);
        }
    }
}
