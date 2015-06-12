namespace Nine.Graphics.OpenGL
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;
    using OpenTK.Graphics.OpenGL;

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

        public SpriteRenderer(TextureFactory textureFactory, int initialSpriteCapacity = 2048)
        {
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));

            this.textureFactory = textureFactory;
            this.CreateBuffers(initialSpriteCapacity);
            this.CreateShaders();
        }

        public void Draw(IEnumerable<Sprite> input, ObjectPool output)
        {
            var array = input as Sprite[];
            if (array != null)
                Draw(array);
            else if (input is Slice<Sprite>)
                Draw((Slice<Sprite>)input);
            else
                throw new NotImplementedException();
        }

        public void Draw(Slice<Sprite> sprites, Slice<Matrix3x2> transforms)
        {
            Debug.Assert(sprites.Count == transforms.Count);
        }

        public void Draw(Slice<Sprite> sprites)
        {
            var spriteCount = sprites.Count;

            EnsureBufferCapacity(spriteCount);

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

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, 0, 0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, 0, 0);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, true, 0, 0);

            GL.DrawElements(BeginMode.Triangles, i / 4 * 6, DrawElementsType.UnsignedShort, 0);
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
            DisposeBuffers();
        }
    }
}
