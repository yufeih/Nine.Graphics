#if DX
namespace Nine.Graphics.Rendering.DirectX
{
    using Nine.Graphics.Content.DirectX;
#else
namespace Nine.Graphics.Rendering.OpenGL
{
    using Nine.Graphics.Content.OpenGL;
#endif
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;

    public sealed partial class SpriteRenderer : IRenderer<Sprite>, IDisposable
    {
        struct Vertex
        {
            public Vector2 Position;
            public int Color;
            public Vector2 TextureCoordinate;

            public const int SizeInBytes = 8 + 4 + 8;
        }

        private readonly TextureFactory textureFactory;

        private Vertex[] vertexData;

        private static ushort[] indexData;
        private static object indexDataLock = new object();

        public SpriteRenderer(TextureFactory textureFactory, int initialSpriteCapacity = 2048)
        {
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));

            this.textureFactory = textureFactory;
            this.CreateBuffers(initialSpriteCapacity);
            this.PlatformCreateBuffers();
            this.PlatformCreateShaders();
        }

        public unsafe void Draw(Slice<Sprite> sprites)
        {
            var spriteCount = sprites.Count;

            EnsureBufferCapacity(spriteCount);

            var vertexCount = 0;

            fixed (Vertex* pVertex = vertexData)
            {
                TextureSlice texture = null; // TODO:

                fixed (Sprite* pSprite = &sprites.Items[sprites.Begin])
                {
                    Sprite* sprite = pSprite;
                    Vertex* vertex = pVertex;

                    for (int i = 0; i < sprites.Count; i++)
                    {
                        if (!sprite->IsVisible || sprite->Texture.Id == 0) continue;

                        texture = textureFactory.GetTexture(sprite->Texture);

                        if (texture == null) continue;

                        if (sprite->Rotation == 0)
                        {
                            PopulateVertex(sprite, texture, vertex++, vertex++, vertex++, vertex++);
                        }
                        else
                        {
                            PopulateVertexWithRotation(sprite, texture, vertex++, vertex++, vertex++, vertex++);
                        }

                        vertexCount += 4;
                        sprite++;
                    }
                }

                if (vertexCount <= 0) return;

                fixed (ushort* pIndex = indexData)
                {
                    PlatformDraw(pVertex, pIndex, vertexCount, vertexCount / 4 * 6, texture);
                }
            }
        }

        private void CreateBuffers(int initialSpriteCapacity)
        {
            this.vertexData = new Vertex[initialSpriteCapacity * 4];
        }

        private void EnsureBufferCapacity(int spriteCount)
        {
            if (spriteCount * 4 > vertexData.Length)
            {
                Array.Resize(ref vertexData, spriteCount * 4);
            }

            if (indexData == null || spriteCount * 6 > indexData.Length)
            {
                lock (indexDataLock)
                {
                    var start = 0;

                    if (indexData != null)
                    {
                        start = indexData.Length / 6;
                    }

                    Array.Resize(ref indexData, spriteCount * 6);

                    PopulateIndex(start, spriteCount);
                }
            }
        }

        private unsafe void PopulateVertex(
            Sprite* sprite, TextureSlice texture,
            Vertex* tl, Vertex* tr, Vertex* bl, Vertex* br)
        {
            var color = sprite->Opacity >= 1.0 ? sprite->Color : sprite->Color * sprite->Opacity;

            var w = (sprite->Size.X > 0 ? sprite->Size.X : texture.Width) * sprite->Scale.X;
            var h = (sprite->Size.Y > 0 ? sprite->Size.Y : texture.Height) * sprite->Scale.Y;

            var x = sprite->Position.X - sprite->Origin.X * w;
            var y = sprite->Position.Y - sprite->Origin.Y * h;

            tl->Position.X = x;
            tl->Position.Y = y;
            tl->Color = color.Bgra;
            tl->TextureCoordinate.X = texture.Left;
            tl->TextureCoordinate.Y = texture.Top;

            tr->Position.X = x + w;
            tr->Position.Y = y;
            tr->Color = color.Bgra;
            tr->TextureCoordinate.X = texture.Right;
            tr->TextureCoordinate.Y = texture.Top;

            bl->Position.X = x;
            bl->Position.Y = y + h;
            bl->Color = color.Bgra;
            bl->TextureCoordinate.X = texture.Left;
            bl->TextureCoordinate.Y = texture.Bottom;

            br->Position.X = x + w;
            br->Position.Y = y + h;
            br->Color = color.Bgra;
            br->TextureCoordinate.X = texture.Right;
            br->TextureCoordinate.Y = texture.Bottom;

            if (sprite->HasTransform)
            {
                // https://github.com/dotnet/corefx/issues/313 
                //tl->Position = Vector2.Transform(tl->Position, sprite->Transform);
                //tr->Position = Vector2.Transform(tr->Position, sprite->Transform);
                //bl->Position = Vector2.Transform(bl->Position, sprite->Transform);
                //br->Position = Vector2.Transform(br->Position, sprite->Transform);
            }
        }

        private unsafe void PopulateVertexWithRotation(
            Sprite* sprite, TextureSlice texture,
            Vertex* tl, Vertex* tr, Vertex* bl, Vertex* br)
        {
            var color = sprite->Opacity >= 1.0 ? sprite->Color : sprite->Color * sprite->Opacity;

            var w = (sprite->Size.X > 0 ? sprite->Size.X : texture.Width) * sprite->Scale.X;
            var h = (sprite->Size.Y > 0 ? sprite->Size.Y : texture.Height) * sprite->Scale.Y;

            var x = sprite->Position.X;
            var y = sprite->Position.Y;

            var dx = -sprite->Origin.X * w;
            var dy = -sprite->Origin.Y * h;

            var cos = Math.Cos(sprite->Rotation);
            var sin = Math.Sin(sprite->Rotation);

            tl->Position.X = (float)(x + dx * cos - dy * sin);
            tl->Position.Y = (float)(y + dx * sin + dy * cos);
            tl->Color = color.Bgra;
            tl->TextureCoordinate.X = texture.Left;
            tl->TextureCoordinate.Y = texture.Top;

            tr->Position.X = (float)(x + (dx + w) * cos - dy * sin);
            tr->Position.Y = (float)(y + (dx + w) * sin + dy * cos);
            tl->Color = color.Bgra;
            tr->TextureCoordinate.X = texture.Right;
            tr->TextureCoordinate.Y = texture.Top;

            bl->Position.X = (float)(x + dx * cos - (dy + h) * sin);
            bl->Position.Y = (float)(y + dx * sin + (dy + h) * cos);
            tl->Color = color.Bgra;
            bl->TextureCoordinate.X = texture.Left;
            bl->TextureCoordinate.Y = texture.Bottom;

            br->Position.X = (float)(x + (dx + w) * cos - (dy + h) * sin);
            br->Position.Y = (float)(y + (dx + w) * sin + (dy + h) * cos);
            tl->Color = color.Bgra;
            br->TextureCoordinate.X = texture.Right;
            br->TextureCoordinate.Y = texture.Bottom;

            if (sprite->HasTransform)
            {
                // https://github.com/dotnet/corefx/issues/313 
                //tl->Position = Vector2.Transform(tl->Position, sprite->Transform);
                //tr->Position = Vector2.Transform(tr->Position, sprite->Transform);
                //bl->Position = Vector2.Transform(bl->Position, sprite->Transform);
                //br->Position = Vector2.Transform(br->Position, sprite->Transform);
            }
        }

        private static void PopulateIndex(int start, int spriteCount)
        {
            for (var i = start; i < spriteCount; i++)
            {
                indexData[i * 6 + 0] = (ushort)(i * 4);
                indexData[i * 6 + 1] = (ushort)(i * 4 + 1);
                indexData[i * 6 + 2] = (ushort)(i * 4 + 2);

                indexData[i * 6 + 3] = (ushort)(i * 4 + 1);
                indexData[i * 6 + 4] = (ushort)(i * 4 + 3);
                indexData[i * 6 + 5] = (ushort)(i * 4 + 2);
            }
        }

        public void Dispose()
        {
            PlatformDispose();
        }

        public override string ToString()
        {
            return $"{ nameof(SpriteRenderer) }: { vertexData.Length } vertices, { indexData.Length } shared indices";
        }
    }
}
