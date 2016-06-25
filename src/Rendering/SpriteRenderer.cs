namespace Nine.Graphics.Rendering
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public abstract class SpriteRenderer<T> : ISpriteRenderer
    {
        private readonly TextureFactory<T> _textureFactory;
        private readonly IEqualityComparer<T> _equaltyComparer = EqualityComparer<T>.Default;

        private Vertex2D[] _vertexData;

        public SpriteRenderer(TextureFactory<T> textureFactory, int initialSpriteCapacity)
        {
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));

            _textureFactory = textureFactory;

            _vertexData = new Vertex2D[initialSpriteCapacity * 4];
        }

        public unsafe void Draw(Matrix4x4 projection, Slice<Sprite> sprites, Slice<Matrix3x2>? transforms = null)
        {
            if (sprites.Length <= 0)
            {
                return;
            }

            if (sprites.Length * 4 > _vertexData.Length)
            {
                Array.Resize(ref _vertexData, sprites.Length * 4);
            }

            fixed (ushort* pIndex = QuadListIndexData.GetIndices(sprites.Length))
            fixed (Vertex2D* pVertex = _vertexData)
            fixed (Sprite* pSprite = &sprites.Items[sprites.Begin])
            {
                var vertexCount = 0;
                var drawing = false;
                var isTransparent = false;
                var previousTexture = (Texture<T>)null;

                Vertex2D* vertex = pVertex;
                Sprite* sprite = pSprite;

                for (var i = 0; i < sprites.Length; i++)
                {
                    var currentTexture = _textureFactory.GetTexture(sprite->Texture);
                    if (currentTexture == null)
                    {
                        continue;
                    }

                    if (previousTexture == null)
                    {
                        previousTexture = currentTexture;
                    }
                    else if (!_equaltyComparer.Equals(currentTexture.PlatformTexture, previousTexture.PlatformTexture))
                    {
                        if (!drawing)
                        {
                            drawing = true;
                            BeginDraw(ref projection, pIndex, sprites.Length * 6);
                        }

                        Draw(pVertex, vertexCount, previousTexture.PlatformTexture, isTransparent);

                        vertexCount = 0;
                        vertex = pVertex;
                        previousTexture = currentTexture;
                        isTransparent = false;
                    }

                    var isSpriteTransparent = currentTexture.IsTransparent || sprite->Color.IsTransparent;
                    isTransparent |= isSpriteTransparent;

                    if (sprite->Rotation == 0)
                    {
                        if (transforms == null)
                        {
                            PopulateVertex(sprite, currentTexture, 
                                vertex++, vertex++, vertex++, vertex++);
                        }
                        else
                        {
                            PopulateVertexWithTransform(sprite, currentTexture, 
                                vertex++, vertex++, vertex++, vertex++, transforms.Value[i]);
                        }
                    }
                    else
                    {
                        if (transforms == null)
                        {
                            PopulateVertexWithRotation(sprite, currentTexture, 
                                vertex++, vertex++, vertex++, vertex++);
                        }
                        else
                        {
                            PopulateVertexWithRotationAndTransform(sprite, currentTexture, 
                                vertex++, vertex++, vertex++, vertex++, transforms.Value[i]);
                        }
                    }

                    vertexCount += 4;
                    sprite++;
                }

                if (vertexCount > 0)
                {
                    if (!drawing)
                    {
                        drawing = true;
                        BeginDraw(ref projection, pIndex, sprites.Length * 6);
                    }

                    Draw(pVertex, vertexCount, previousTexture.PlatformTexture, isTransparent);
                }

                if (drawing)
                {
                    EndDraw();
                }
            }
        }

        protected abstract unsafe void BeginDraw(ref Matrix4x4 projection, ushort* pIndex, int indexCount);
        protected abstract unsafe void Draw(Vertex2D* pVertex, int vertexCount, T texture, bool isTransparent);
        protected abstract void EndDraw();

        private unsafe void PopulateVertex(
            Sprite* sprite, Texture<T> texture,
            Vertex2D* tl, Vertex2D* tr, Vertex2D* bl, Vertex2D* br)
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
            Sprite* sprite, Texture<T> texture,
            Vertex2D* tl, Vertex2D* tr, Vertex2D* bl, Vertex2D* br, Matrix3x2 transform)
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
            Sprite* sprite, Texture<T> texture,
            Vertex2D* tl, Vertex2D* tr, Vertex2D* bl, Vertex2D* br)
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
            Sprite* sprite, Texture<T> texture,
            Vertex2D* tl, Vertex2D* tr, Vertex2D* bl, Vertex2D* br, Matrix3x2 transform)
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

        public override string ToString() => $"{GetType().Name}: {_vertexData.Length / 4} sprites";
    }
}
