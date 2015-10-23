#if DX
namespace Nine.Graphics.DirectX
{
#else
namespace Nine.Graphics.OpenGL
{
#endif
    using Rendering;
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    enum Flip
    {
        None,
        Horizontally,
        Vertically,
        Both
    }

    /// <summary>
    /// Contains extension method for <see cref="IDynamicPrimitiveRenderer"/>.
    /// </summary>
    //[EditorBrowsable(EditorBrowsableState.Never)]
    public static class DynamicPrimitiveExtensions
    {
        // TODO: Add more advanced texture rectangles
        //       Like texture region and texture rotation

        public static void AddRectangle(this IDynamicPrimitiveRenderer dynamicPrimitive, Vector2 min, Vector2 max, Color color, Matrix4x4? world = null, float lineWidth = 1)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.Lines, null, world, lineWidth);
            {
                dynamicPrimitive.AddVertex(new Vector3(min.X, min.Y, 0), color);
                dynamicPrimitive.AddVertex(new Vector3(min.X, max.Y, 0), color);
                dynamicPrimitive.AddVertex(new Vector3(max.X, max.Y, 0), color);
                dynamicPrimitive.AddVertex(new Vector3(max.X, min.Y, 0), color);

                dynamicPrimitive.AddIndex(0);
                dynamicPrimitive.AddIndex(1);
                dynamicPrimitive.AddIndex(1);
                dynamicPrimitive.AddIndex(2);
                dynamicPrimitive.AddIndex(2);
                dynamicPrimitive.AddIndex(3);
                dynamicPrimitive.AddIndex(3);
                dynamicPrimitive.AddIndex(0);
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddRectangle(this IDynamicPrimitiveRenderer dynamicPrimitive, Vector2 min, Vector2 max, Vector3 up, Color color, Matrix4x4? world = null, float lineWidth = 1)
        {
            var transform = MathHelper.CreateRotation(new Vector3(0, 1, 0), up);

            dynamicPrimitive.BeginPrimitive(PrimitiveType.Lines, null, world, lineWidth);
            {
                dynamicPrimitive.AddVertex(Vector3.TransformNormal(new Vector3(min.X, min.Y, 0), transform), color);
                dynamicPrimitive.AddVertex(Vector3.TransformNormal(new Vector3(min.X, max.Y, 0), transform), color);
                dynamicPrimitive.AddVertex(Vector3.TransformNormal(new Vector3(max.X, max.Y, 0), transform), color);
                dynamicPrimitive.AddVertex(Vector3.TransformNormal(new Vector3(max.X, min.Y, 0), transform), color);

                dynamicPrimitive.AddIndex(0);
                dynamicPrimitive.AddIndex(1);
                dynamicPrimitive.AddIndex(1);
                dynamicPrimitive.AddIndex(2);
                dynamicPrimitive.AddIndex(2);
                dynamicPrimitive.AddIndex(3);
                dynamicPrimitive.AddIndex(3);
                dynamicPrimitive.AddIndex(0);
            }
            dynamicPrimitive.EndPrimitive();
        }

        // TODO: Add with Nine.Geometry
        //public static void AddSolidRectangle(this IDynamicPrimitiveRenderer dynamicPrimitive, BoundingRectangle rectangle, Color color, TextureId? texture = null, Matrix4x4? world = null)
        //    => AddSolidRectangle(dynamicPrimitive, new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom), color, texture, world);

        public static void AddSolidRectangle(this IDynamicPrimitiveRenderer dynamicPrimitive, Imaging.Rectangle rectangle, Color color, TextureId? texture = null, Matrix4x4? world = null)
            => AddSolidRectangle(dynamicPrimitive, new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom), color, texture, world);

        public static void AddSolidRectangle(this IDynamicPrimitiveRenderer dynamicPrimitive, Vector2 min, Vector2 max, Color color, TextureId? texture = null, Matrix4x4? world = null)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.Triangles, texture, world);
            {
                dynamicPrimitive.AddVertex(new Vector3(min.X, min.Y, 0), color, new Vector2(0, 0));
                dynamicPrimitive.AddVertex(new Vector3(min.X, max.Y, 0), color, new Vector2(0, 1));
                dynamicPrimitive.AddVertex(new Vector3(max.X, max.Y, 0), color, new Vector2(1, 1));
                dynamicPrimitive.AddVertex(new Vector3(max.X, min.Y, 0), color, new Vector2(1, 0));

                dynamicPrimitive.AddIndex(0);
                dynamicPrimitive.AddIndex(1);
                dynamicPrimitive.AddIndex(2);

                dynamicPrimitive.AddIndex(2);
                dynamicPrimitive.AddIndex(3);
                dynamicPrimitive.AddIndex(0);
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddCircle(this IDynamicPrimitiveRenderer dynamicPrimitive, Vector3 center, float radius, int tessellation, Color color, Matrix4x4? world = null, float lineWidth = 1)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineStrip, null, world, lineWidth);
            {
                if (tessellation < 3)
                    throw new ArgumentOutOfRangeException("tessellation");

                int horizontalSegments = tessellation;

                // Create a single ring of vertices at this latitude.
                for (int j = 0; j <= horizontalSegments; j++)
                {
                    float longitude = j * MathHelper.TwoPI / horizontalSegments;

                    float dx = (float)Math.Cos(longitude);
                    float dy = (float)Math.Sin(longitude);

                    Vector3 normal = new Vector3(dx, dy, 0);

                    dynamicPrimitive.AddVertex(normal * radius + center, color);
                }
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddCircle(this IDynamicPrimitiveRenderer dynamicPrimitive, Vector3 center, float radius, Vector3 up, int tessellation, Color color, Matrix4x4? world = null, float lineWidth = 1)
        {
            var transform = Matrix4x4.CreateScale(radius) *
                            MathHelper.CreateRotation(new Vector3(0, 1, 0), up) *
                            Matrix4x4.CreateTranslation(center);

            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineStrip, null, world, lineWidth);
            {
                if (tessellation < 3)
                    throw new ArgumentOutOfRangeException("tessellation");

                int horizontalSegments = tessellation;

                // Create a single ring of vertices at this latitude.
                for (int j = 0; j <= horizontalSegments; j++)
                {
                    float longitude = j * MathHelper.TwoPI / horizontalSegments;

                    float dx = (float)Math.Cos(longitude);
                    float dy = (float)Math.Sin(longitude);

                    Vector3 normal = new Vector3(dx, dy, 0);

                    dynamicPrimitive.AddVertex(Vector3.Transform(normal, transform), color);
                }
            }
            dynamicPrimitive.EndPrimitive();
        }

        // TODO: AddGrid(...) is stepping in Z, should be Y

        public static void AddGrid(this IDynamicPrimitiveRenderer dynamicPrimitive, float step, int countX, int countZ, Color color, Matrix4x4? world = null, float lineWidth = 1)
            => AddGrid(dynamicPrimitive, -step * countX * 0.5f, 0, -step * countZ * 0.5f, step * countX, step * countZ, countX, countZ, color, world, lineWidth);

        public static void AddGrid(this IDynamicPrimitiveRenderer dynamicPrimitive, float x, float y, float z, float step, int countX, int countZ, Color color, Matrix4x4? world = null, float lineWidth = 1)
            => AddGrid(dynamicPrimitive, x, y, z, step * countX, step * countZ, countX, countZ, color, world, lineWidth);

        public static void AddGrid(this IDynamicPrimitiveRenderer dynamicPrimitive, float x, float y, float z, float width, float height, int countX, int countZ, Color color, Matrix4x4? world = null, float lineWidth = 1)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.Lines, null, world, lineWidth);
            {
                float incU = width / countX;
                float incV = height / countZ;

                for (int u = 0; u <= countX; u++)
                {
                    dynamicPrimitive.AddVertex(new Vector3(x + 0, y, z + u * incU), color);
                    dynamicPrimitive.AddVertex(new Vector3(x + height, y, z + u * incU), color);
                }

                for (int v = 0; v <= countZ; v++)
                {
                    dynamicPrimitive.AddVertex(new Vector3(x + v * incV, y, z + 0), color);
                    dynamicPrimitive.AddVertex(new Vector3(x + v * incV, y, z + width), color);
                }
            }
            dynamicPrimitive.EndPrimitive();
        }
        
        // TODO: Plane is in 3D

        public static void AddPlane(this IDynamicPrimitiveRenderer dynamicPrimitive, Plane plane, float size, int tessellation, Color color, Matrix4x4? world = null, float lineWidth = 1)
        {
            var transform = MathHelper.CreateRotation(new Vector3(0, 1, 0), plane.Normal) *
                            Matrix4x4.CreateTranslation(plane.Normal * plane.D);

            if (world.HasValue)
                transform *= world.Value;

            AddGrid(dynamicPrimitive, 0, 0, 0, size, size, tessellation, tessellation, color, transform, lineWidth);
        }
        
        public static void AddLine(this IDynamicPrimitiveRenderer dynamicPrimitive, Vector3 v1, Vector3 v2, Color color, Matrix4x4? world = null, float lineWidth = 1)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.Lines, null, world, lineWidth);
            {
                dynamicPrimitive.AddVertex(v1, color);
                dynamicPrimitive.AddVertex(v2, color);
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddLine(this IDynamicPrimitiveRenderer dynamicPrimitive, IEnumerable<Vector3> lineStrip, Color color, Matrix4x4? world = null, float lineWidth = 1)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineStrip, null, world, lineWidth);
            {
                foreach (Vector3 position in lineStrip)
                {
                    dynamicPrimitive.AddVertex(position, color);
                }
            }
            dynamicPrimitive.EndPrimitive();
        }

        //static Vector2[] GetTextureCoords(Flip flip)
        //{
        //    switch (flip)
        //    {
        //        case Flip.None:
        //            return new Vector2[] {
        //                new Vector2(1, 0),
        //                new Vector2(1, 1),
        //                new Vector2(0, 1),
        //                new Vector2(0, 0)
        //            };
        //        case Flip.Horizontally:
        //            return new Vector2[] {
        //                new Vector2(0, 0),
        //                new Vector2(0, 1),
        //                new Vector2(1, 1),
        //                new Vector2(1, 0)
        //            };
        //        case Flip.Vertically:
        //            return new Vector2[] {
        //                new Vector2(1, 1),
        //                new Vector2(1, 0),
        //                new Vector2(0, 0),
        //                new Vector2(0, 1)
        //            };
        //        case Flip.Both:
        //            return new Vector2[] {
        //                new Vector2(0, 1),
        //                new Vector2(0, 0),
        //                new Vector2(1, 0),
        //                new Vector2(1, 1)
        //            };
        //    }
        //    throw new System.ArgumentNullException("flip");
        //}
    }
}
