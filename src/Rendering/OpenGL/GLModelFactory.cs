namespace Nine.Graphics.Rendering
{
    using Nine.Graphics.Content;
    using OpenTK.Graphics.OpenGL;
    using System;
    using System.Collections.Generic;

    public class GLModelFactory : ModelFactory<int>
    {
        public GLModelFactory(IGraphicsHost graphicsHost, IModelLoader loader, int capacity = 1024)
            : base(graphicsHost, loader, capacity)
        { }

        public override unsafe Model CreateModel(ModelContent data)
        {
            GLDebug.CheckAccess();

            var meshes = new List<ModelMesh>();
            foreach (var mesh in data.Meshes)
            {
                var faces = new List<ModelFace>();
                foreach (var face in mesh.Faces)
                {
                    var indexBuffer = GL.GenBuffer();

                    GL.BindBuffer(BufferTarget.ArrayBuffer, indexBuffer);
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)face.Indices.Length, face.Indices, BufferUsageHint.StaticDraw);

                    faces.Add(new ModelFace(indexBuffer, face.Indices));
                }

                var vertexBuffer = GL.GenBuffer();

                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mesh.Vertices.Length * VertexPositionNormalTexture.SizeInBytes), mesh.Vertices, BufferUsageHint.StaticDraw);

                meshes.Add(new ModelMesh(vertexBuffer, 
                    mesh.Name, mesh.MaterialIndex, mesh.Vertices, faces.ToArray()));
            }

            return new Model(meshes.ToArray());
        }
    }
}
