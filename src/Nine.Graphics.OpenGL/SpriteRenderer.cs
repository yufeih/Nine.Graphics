namespace Nine.Graphics.OpenGL
{
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using OpenTK.Graphics.OpenGL4;

    public sealed partial class SpriteRenderer : IRenderer<Sprite>, IDisposable
    {
        struct Vertex
        {
            public Vector3 Position;
            public int Color;
            public Vector2 TextureCoordinate;

            public const int SizeInBytes = 6 + 4 + 4;
        }

        private readonly TextureFactory textureFactory;

        private Vertex[] vertexBuffer;
        private GCHandle pinnedVertexBuffer;

        private static ushort[] indexBuffer;

        // Pin indexBuffer to the memory for the whole lifetime of the app.
        private static GCHandle pinnedIndexBuffer;
        private static object indexBufferLock = new object();

        public SpriteRenderer(TextureFactory textureFactory, int initialSpriteCapacity = 2048)
        {
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));

            this.textureFactory = textureFactory;
            this.vertexBuffer = new Vertex[initialSpriteCapacity * 4];
            this.pinnedVertexBuffer = GCHandle.Alloc(vertexBuffer, GCHandleType.Pinned);
        }

        public void Draw(Slice<Sprite> sprites, Slice<Matrix3x2> transforms)
        {
            Debug.Assert(sprites.Count == transforms.Count);
        }

        public void Draw(Slice<Sprite> sprites)
        {
            var spriteCount = sprites.Count;

            EnsureCapacity(spriteCount);

            var i = 0;

            for (var iSprite = sprites.Begin; iSprite < sprites.End; iSprite++)
            {
                var sprite = sprites.Items[iSprite];

                if (!sprite.IsVisible || sprite.Texture.Id == 0) continue;

                var texture = textureFactory.GetTexture(sprite.Texture);

                if (texture == null) continue;

                ExtractVertex(sprite, texture,
                    ref vertexBuffer[i + 0], ref vertexBuffer[i + 1], 
                    ref vertexBuffer[i + 2], ref vertexBuffer[i + 3]);

                i += 4;
            }

            if (i <= 0) return;

            // TODO: Bind buffer.

            GL.DrawElements(BeginMode.Triangles, i / 4 * 6, DrawElementsType.UnsignedShort, 0);
        }

        private void EnsureCapacity(int spriteCount)
        {
            if (spriteCount * 4 > vertexBuffer.Length)
            {
                pinnedVertexBuffer.Free();

                Array.Resize(ref vertexBuffer, spriteCount * 4);

                pinnedVertexBuffer = GCHandle.Alloc(vertexBuffer, GCHandleType.Pinned);
            }

            if (indexBuffer == null || spriteCount * 6 > indexBuffer.Length)
            {
                lock (indexBufferLock)
                {
                    var start = 0;

                    if (indexBuffer != null)
                    {
                        start = indexBuffer.Length / 6;
                        pinnedIndexBuffer.Free();
                    }
                    
                    Array.Resize(ref indexBuffer, spriteCount * 6);

                    pinnedIndexBuffer = GCHandle.Alloc(indexBuffer, GCHandleType.Pinned);

                    PopulateIndex(start, spriteCount);
                }
            }
        }

        private void ExtractVertex(
            Sprite sprite, TextureSlice texture,
            ref Vertex tl, ref Vertex tr, ref Vertex bl, ref Vertex br)
        {
            var x = sprite.Position.X + (sprite.Origin.X * texture.Width) * sprite.Scale.X;
            var y = sprite.Position.Y + (sprite.Origin.Y * texture.Height) * sprite.Scale.Y;

            var w = texture.Width * sprite.Scale.X;
            var h = texture.Height * sprite.Scale.Y;

            // TODO: Rotate

            tl.Position.X = x;
            tl.Position.Y = y;
            tl.Position.Z = sprite.Depth;
            tl.Color = sprite.Color;
            tl.TextureCoordinate.X = texture.Left;
            tl.TextureCoordinate.Y = texture.Top;

            tr.Position.X = x + w;
            tr.Position.Y = y;
            tr.Position.Z = sprite.Depth;
            tr.Color = sprite.Color;
            tr.TextureCoordinate.X = texture.Right;
            tr.TextureCoordinate.Y = texture.Top;

            bl.Position.X = x;
            bl.Position.Y = y + h;
            bl.Position.Z = sprite.Depth;
            bl.Color = sprite.Color;
            bl.TextureCoordinate.X = texture.Left;
            bl.TextureCoordinate.Y = texture.Bottom;

            br.Position.X = x + w;
            br.Position.Y = y + h;
            br.Position.Z = sprite.Depth;
            br.Color = sprite.Color;
            br.TextureCoordinate.X = texture.Right;
            br.TextureCoordinate.Y = texture.Bottom;
        }

        private static void PopulateIndex(int start, int spriteCount)
        {
            for (var i = start; i < spriteCount; i++)
            {
                indexBuffer[i * 6 + 0] = (ushort)(i * 4);
                indexBuffer[i * 6 + 1] = (ushort)(i * 4 + 1);
                indexBuffer[i * 6 + 2] = (ushort)(i * 4 + 2);

                indexBuffer[i * 6 + 3] = (ushort)(i * 4 + 1);
                indexBuffer[i * 6 + 4] = (ushort)(i * 4 + 3);
                indexBuffer[i * 6 + 5] = (ushort)(i * 4 + 2);
            }
        }

        public void Dispose()
        {
            if (vertexBuffer != null)
            {
                pinnedVertexBuffer.Free();
            }
        }
    }
}
