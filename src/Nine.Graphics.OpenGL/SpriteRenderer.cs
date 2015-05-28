namespace Nine.Graphics.OpenGL
{
    using System;
    using System.Numerics;

    public class SpriteRenderer : IRenderer<Sprite>
    {
        struct Vertex
        {
            public Vector3 Position;
            public int Color;
            public Vector2 TextureCoordinate;

            public const int SizeInBytes = 6 + 4 + 4;
        }

        private readonly TextureFactory textureFactory;

        private Vertex[] vertices;
        private ushort[] indices; // TODO: Index data never change so it can be shared globally using static

        public SpriteRenderer(TextureFactory textureFactory, int initialSpriteCapacity = 2048)
        {
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));

            this.textureFactory = textureFactory;
            this.vertices = new Vertex[initialSpriteCapacity * 4];
            this.indices = new ushort[initialSpriteCapacity * 6];

            PopulateIndex(0, initialSpriteCapacity);
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
                    ref vertices[i + 0], ref vertices[i + 1], 
                    ref vertices[i + 2], ref vertices[i + 3]);

                i += 4;
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

        private void EnsureCapacity(int spriteCount)
        {
            if (spriteCount * 4 > vertices.Length)
            {
                var start = vertices.Length / 4;

                Array.Resize(ref vertices, spriteCount * 4);
                Array.Resize(ref indices, spriteCount * 6);

                PopulateIndex(start, spriteCount);
            }
        }

        private void PopulateIndex(int start, int spriteCount)
        {
            for (var i = start; i < spriteCount; i++)
            {
                indices[i * 6 + 0] = (ushort)(i * 4);
                indices[i * 6 + 1] = (ushort)(i * 4 + 1);
                indices[i * 6 + 2] = (ushort)(i * 4 + 2);

                indices[i * 6 + 3] = (ushort)(i * 4 + 1);
                indices[i * 6 + 4] = (ushort)(i * 4 + 3);
                indices[i * 6 + 5] = (ushort)(i * 4 + 2);
            }
        }
    }
}
