namespace Nine.Graphics.Content
{
    using Assimp;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Threading.Tasks;
    using System.IO;

    public enum ImportQuality
    {
        Low,
        Medium,
        High,
    }

    public class ModelLoader : IModelLoader
    {
        public ImportQuality Quality { get; set; } = ImportQuality.High;

        private readonly IContentProvider _contentProvider;
        private readonly Lazy<AssimpContext> _assimp;

        public ModelLoader(IContentProvider contentLocator)
        {
            if (contentLocator == null) throw new ArgumentNullException(nameof(contentLocator));

            _contentProvider = contentLocator;

            _assimp = new Lazy<AssimpContext>(() => new AssimpContext());
            //_assimp.SetConfig(new NormalSmoothingAngleConfig(66.0f));
        }

        public async Task<ModelContent> Load(string name)
        {
            if (name.StartsWith("n:"))
            {
                if (name == ModelId.Missing.Name)
                    return new ModelContent(null);
                if (name == ModelId.Error.Name)
                    return new ModelContent(null);
            }

            var quality = PostProcessPreset.TargetRealTimeFast;

            switch (Quality)
            {
                case ImportQuality.Low:
                    quality = PostProcessPreset.TargetRealTimeFast;
                    break;
                case ImportQuality.Medium:
                    quality = PostProcessPreset.TargetRealTimeQuality;
                    break;
                case ImportQuality.High:
                    quality = PostProcessPreset.TargetRealTimeMaximumQuality;
                    break;
            }

            using (var stream = await _contentProvider.Open(name).ConfigureAwait(false))
            {
                if (stream == null) return null;

                var meshes = new List<ModelMeshContent>();

                var fileExtension = Path.GetExtension(name);
                var scene = _assimp.Value.ImportFileFromStream(stream, quality, fileExtension);

                foreach (var mesh in scene.Meshes)
                {
                    var vertices = new List<VertexPositionNormalTexture>();
                    var faces = new List<ModelFaceContent>();

                    int vertexCount = mesh.Vertices.Count;

                    for (int i = 0; i < mesh.Vertices.Count; i++)
                    {
                        Vector3D position = mesh.Vertices[i];
                        Vector3D normal = new Vector3D(0, 0, 0);
                        Vector2D texCoords = new Vector2D(0, 0);

                        if (mesh.HasNormals)
                            normal = mesh.Normals[i];

                        vertices.Add(new VertexPositionNormalTexture(
                            new Vector3(position.X, position.Y, position.Z),
                            new Vector3(normal.X, normal.Y, normal.Z),
                            new Vector2(texCoords.X, texCoords.Y)));
                    }

                    foreach (var face in mesh.Faces)
                    {
                        faces.Add(new ModelFaceContent(face.Indices.ToArray()));
                    }

                    meshes.Add(new ModelMeshContent(mesh.Name, mesh.MaterialIndex,
                        vertices.ToArray(), faces.ToArray()));
                }

                return new ModelContent(meshes.ToArray());
            }
        }
    }
}
