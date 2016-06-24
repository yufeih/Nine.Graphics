namespace Nine.Graphics.Rendering
{
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using Nine.Graphics.Content;
    
    public partial class ModelBone
    {
        public readonly string Name;
        public readonly Matrix4x4 Offset;
        public readonly VertexWeight[] VertexWeights;

        public ModelBone(
            string name,
            Matrix4x4? offset = null,
            VertexWeight[] vertexWeights = null)
        {
            this.Name = name;
            this.Offset = offset ?? Matrix4x4.Identity;
            this.VertexWeights = vertexWeights ?? new VertexWeight[0];
        }

        public override string ToString()
        {
            return $"Name: { Name }, VertexWeights Count: { VertexWeights.Length }, Offset: { Offset }";
        }
    }

    public partial class ModelFace
    {
        public readonly PlatformBuffer PlatformBuffer;

        public readonly int[] Indices;

        public ModelFace(PlatformBuffer platformBuffer, int[] indices)
        {
            this.PlatformBuffer = platformBuffer;
            this.Indices = indices;
        }

        public override string ToString()
        {
            return $"Indices Count: { Indices.Length }";
        }
    }

    public partial class ModelMesh
    {
        public readonly PlatformBuffer PlatformBuffer;

        public readonly string Name;

        public readonly int MaterialIndex;

        public readonly VertexPositionNormalTexture[] Vertices;

        public readonly ModelFace[] Faces;
        public readonly ModelBone[] Bones;
        
        public ModelMesh(
            PlatformBuffer platformBuffer,
            string name,
            int materialIndex,
            VertexPositionNormalTexture[] vertices,
            ModelFace[] faces,
            ModelBone[] bones = null)
        {
            this.PlatformBuffer = platformBuffer;
            this.Name = name;
            this.MaterialIndex = materialIndex;
            this.Vertices = vertices;
            this.Faces = faces;
            this.Bones = bones;
        }

        public override string ToString()
        {
            return $"Name: { Name }, Material Index: { MaterialIndex }, Vertices Count: { Vertices.Length }, Faces Count: { Faces.Length }, " +
                   $"Bones Count: { Bones.Length }";
        }
    }

    public partial class Model
    {
        public readonly ModelMesh[] Meshes;

        public Model(ModelMesh[] meshes)
        {
            this.Meshes = meshes;
        }

        public override string ToString()
        {
            return $"Meshes: { Meshes.Length }";
        }
    }
}
