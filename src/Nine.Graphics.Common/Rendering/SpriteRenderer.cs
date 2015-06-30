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
    using System.Numerics;
    using Nine.Graphics.Primitives;

    public sealed partial class SpriteRenderer : ISpriteRenderer, IDisposable
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

        public unsafe void Draw(Slice<Sprite> sprites, Slice<Matrix3x2>? transforms = null, Slice<int>? indices = null, TextureId texture = default(TextureId))
        {
            var spriteCount = (indices != null ? indices.Value.Count : sprites.Count);
            if (spriteCount <= 0)
            {
                return;
            }

            EnsureBufferCapacity(spriteCount);

            fixed (Vertex* pVertex = vertexData)
            fixed (ushort* pIndex = indexData)
            fixed (Sprite* pSprite = &sprites.Items[sprites.Begin])
            {
                var vertexCount = 0;
                var previousTexture = (Texture)null;

                Vertex* vertex = pVertex;
                Sprite* sprite = pSprite;

                for (var i = 0; i < spriteCount; i++)
                {
                    var iIndexed = i;
                    if (indices != null)
                    {
                        iIndexed = indices.Value[i];
                        sprite += iIndexed;
                    }

                    var textureToUse = (texture.Id != 0 ? texture : sprite->Texture);
                    if (textureToUse.Id == 0)
                    {
                        continue;
                    }

                    var currentTexture = textureFactory.GetTexture(sprite->Texture);
                    if (currentTexture == null)
                    {
                        continue;
                    }

                    if (previousTexture == null)
                    {
                        previousTexture = currentTexture;
                    }
                    else if (currentTexture.PlatformTexture != previousTexture.PlatformTexture)
                    {
                        PlatformDraw(pVertex, pIndex, vertexCount, vertexCount / 4 * 6, previousTexture);

                        vertexCount = 0;
                        vertex = pVertex;
                        previousTexture = currentTexture;
                    }

                    if (sprite->Rotation == 0)
                    {
                        if (transforms == null)
                        {
                            PopulateVertex(sprite, currentTexture, vertex++, vertex++, vertex++, vertex++);
                        }
                        else
                        {
                            PopulateVertexWithTransform(sprite, currentTexture, vertex++, vertex++, vertex++, vertex++, transforms.Value[iIndexed]);
                        }
                    }
                    else
                    {
                        if (transforms == null)
                        {
                            PopulateVertexWithRotation(sprite, currentTexture, vertex++, vertex++, vertex++, vertex++);
                        }
                        else
                        {
                            PopulateVertexWithRotationAndTransform(sprite, currentTexture, vertex++, vertex++, vertex++, vertex++, transforms.Value[iIndexed]);
                        }
                    }

                    vertexCount += 4;

                    if (indices == null)
                    {
                        sprite++;
                    }
                }

                if (vertexCount > 0)
                {
                    PlatformDraw(pVertex, pIndex, vertexCount, vertexCount / 4 * 6, previousTexture);
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
            Sprite* sprite, Texture texture,
            Vertex* tl, Vertex* tr, Vertex* bl, Vertex* br)
        {
            var w = (sprite->Size.X > 0 ? sprite->Size.X : texture.Width) * sprite->Scale.X;
            var h = (sprite->Size.Y > 0 ? sprite->Size.Y : texture.Height) * sprite->Scale.Y;

            var x = sprite->Position.X - sprite->Origin.X * w;
            var y = sprite->Position.Y - sprite->Origin.Y * h;

            tl->Position.X = x;
            tl->Position.Y = y;
            tl->Color = sprite->Color.Bgra;
            tl->TextureCoordinate.X = texture.Left;
            tl->TextureCoordinate.Y = texture.Top;

            tr->Position.X = x + w;
            tr->Position.Y = y;
            tr->Color = sprite->Color.Bgra;
            tr->TextureCoordinate.X = texture.Right;
            tr->TextureCoordinate.Y = texture.Top;

            bl->Position.X = x;
            bl->Position.Y = y + h;
            bl->Color = sprite->Color.Bgra;
            bl->TextureCoordinate.X = texture.Left;
            bl->TextureCoordinate.Y = texture.Bottom;

            br->Position.X = x + w;
            br->Position.Y = y + h;
            br->Color = sprite->Color.Bgra;
            br->TextureCoordinate.X = texture.Right;
            br->TextureCoordinate.Y = texture.Bottom;
        }

        private unsafe void PopulateVertexWithTransform(
            Sprite* sprite, Texture texture,
            Vertex* tl, Vertex* tr, Vertex* bl, Vertex* br, Matrix3x2 transform)
        {
            var w = (sprite->Size.X > 0 ? sprite->Size.X : texture.Width) * sprite->Scale.X;
            var h = (sprite->Size.Y > 0 ? sprite->Size.Y : texture.Height) * sprite->Scale.Y;

            var x = sprite->Position.X - sprite->Origin.X * w;
            var y = sprite->Position.Y - sprite->Origin.Y * h;

            tl->Position.X = x;
            tl->Position.Y = y;
            tl->Color = sprite->Color.Bgra;
            tl->TextureCoordinate.X = texture.Left;
            tl->TextureCoordinate.Y = texture.Top;

            tr->Position.X = x + w;
            tr->Position.Y = y;
            tr->Color = sprite->Color.Bgra;
            tr->TextureCoordinate.X = texture.Right;
            tr->TextureCoordinate.Y = texture.Top;

            bl->Position.X = x;
            bl->Position.Y = y + h;
            bl->Color = sprite->Color.Bgra;
            bl->TextureCoordinate.X = texture.Left;
            bl->TextureCoordinate.Y = texture.Bottom;

            br->Position.X = x + w;
            br->Position.Y = y + h;
            br->Color = sprite->Color.Bgra;
            br->TextureCoordinate.X = texture.Right;
            br->TextureCoordinate.Y = texture.Bottom;

            tl->Position = Vector2.Transform(tl->Position, transform);
            tr->Position = Vector2.Transform(tr->Position, transform);
            bl->Position = Vector2.Transform(bl->Position, transform);
            br->Position = Vector2.Transform(br->Position, transform);
        }

        private unsafe void PopulateVertexWithRotation(
            Sprite* sprite, Texture texture,
            Vertex* tl, Vertex* tr, Vertex* bl, Vertex* br)
        {
            var w = (sprite->Size.X > 0 ? sprite->Size.X : texture.Width) * sprite->Scale.X;
            var h = (sprite->Size.Y > 0 ? sprite->Size.Y : texture.Height) * sprite->Scale.Y;

            var x = sprite->Position.X;
            var y = sprite->Position.Y;

            var dx = -sprite->Origin.X * w;
            var dy = -sprite->Origin.Y * h;

            var radius = MathHelper.ToRadius(sprite->Rotation);

            var cos = Math.Cos(radius);
            var sin = Math.Sin(radius);

            tl->Position.X = (float)(x + dx * cos - dy * sin);
            tl->Position.Y = (float)(y + dx * sin + dy * cos);
            tl->Color = sprite->Color.Bgra;
            tl->TextureCoordinate.X = texture.Left;
            tl->TextureCoordinate.Y = texture.Top;

            tr->Position.X = (float)(x + (dx + w) * cos - dy * sin);
            tr->Position.Y = (float)(y + (dx + w) * sin + dy * cos);
            tr->Color = sprite->Color.Bgra;
            tr->TextureCoordinate.X = texture.Right;
            tr->TextureCoordinate.Y = texture.Top;

            bl->Position.X = (float)(x + dx * cos - (dy + h) * sin);
            bl->Position.Y = (float)(y + dx * sin + (dy + h) * cos);
            bl->Color = sprite->Color.Bgra;
            bl->TextureCoordinate.X = texture.Left;
            bl->TextureCoordinate.Y = texture.Bottom;

            br->Position.X = (float)(x + (dx + w) * cos - (dy + h) * sin);
            br->Position.Y = (float)(y + (dx + w) * sin + (dy + h) * cos);
            br->Color = sprite->Color.Bgra;
            br->TextureCoordinate.X = texture.Right;
            br->TextureCoordinate.Y = texture.Bottom;
        }

        private unsafe void PopulateVertexWithRotationAndTransform(
            Sprite* sprite, Texture texture,
            Vertex* tl, Vertex* tr, Vertex* bl, Vertex* br, Matrix3x2 transform)
        {
            var w = (sprite->Size.X > 0 ? sprite->Size.X : texture.Width) * sprite->Scale.X;
            var h = (sprite->Size.Y > 0 ? sprite->Size.Y : texture.Height) * sprite->Scale.Y;

            var x = sprite->Position.X;
            var y = sprite->Position.Y;

            var dx = -sprite->Origin.X * w;
            var dy = -sprite->Origin.Y * h;

            var radius = MathHelper.ToRadius(sprite->Rotation);

            var cos = Math.Cos(radius);
            var sin = Math.Sin(radius);

            tl->Position.X = (float)(x + dx * cos - dy * sin);
            tl->Position.Y = (float)(y + dx * sin + dy * cos);
            tl->Color = sprite->Color.Bgra;
            tl->TextureCoordinate.X = texture.Left;
            tl->TextureCoordinate.Y = texture.Top;

            tr->Position.X = (float)(x + (dx + w) * cos - dy * sin);
            tr->Position.Y = (float)(y + (dx + w) * sin + dy * cos);
            tr->Color = sprite->Color.Bgra;
            tr->TextureCoordinate.X = texture.Right;
            tr->TextureCoordinate.Y = texture.Top;

            bl->Position.X = (float)(x + dx * cos - (dy + h) * sin);
            bl->Position.Y = (float)(y + dx * sin + (dy + h) * cos);
            bl->Color = sprite->Color.Bgra;
            bl->TextureCoordinate.X = texture.Left;
            bl->TextureCoordinate.Y = texture.Bottom;

            br->Position.X = (float)(x + (dx + w) * cos - (dy + h) * sin);
            br->Position.Y = (float)(y + (dx + w) * sin + (dy + h) * cos);
            br->Color = sprite->Color.Bgra;
            br->TextureCoordinate.X = texture.Right;
            br->TextureCoordinate.Y = texture.Bottom;

            tl->Position = Vector2.Transform(tl->Position, transform);
            tr->Position = Vector2.Transform(tr->Position, transform);
            bl->Position = Vector2.Transform(bl->Position, transform);
            br->Position = Vector2.Transform(br->Position, transform);
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
