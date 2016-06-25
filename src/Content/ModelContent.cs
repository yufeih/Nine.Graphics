namespace Nine.Graphics.Content
{
    using System;
    using System.Diagnostics;
    using System.Numerics;

    public enum PrimitiveType
    {
        Point = 1,
        Line = 2,
        Triangle = 4,
    }

    public struct VertexPositionNormalTexture
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Vector2 TextureCoordinate;

        public VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 textureCoords)
        {
            this.Position = position;
            this.Normal = normal;
            this.TextureCoordinate = textureCoords;
        }

        public const int SizeInBytes = 12 + 12 + 8;
    }

    public struct VertexWeight
    {
        public readonly int VertexId;
        public readonly float Weight;

        public VertexWeight(int vertexId, float weight)
        {
            this.VertexId = vertexId;
            this.Weight = weight;
        }
    }

    public class ModelAnimationContent
    {

    }

    public class ModelBoneContent
    {
        public readonly string Name;
        public readonly Matrix4x4 Offset;
        public readonly VertexWeight[] VertexWeights;
        
        public ModelBoneContent(
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

    public class ModelFaceContent
    {
        public readonly int[] Indices;

        public ModelFaceContent(int[] indices)
        {
            this.Indices = indices;
        }

        public override string ToString()
        {
            return $"Indices Count: { Indices.Length }";
        }
    }

    public class ModelMeshContent
    {
        public readonly string Name;

        public readonly int MaterialIndex;

        public readonly VertexPositionNormalTexture[] Vertices;

        public readonly ModelFaceContent[] Faces;
        public readonly ModelBoneContent[] Bones;

        public readonly ModelAnimationContent[] Animations;

        public ModelMeshContent(
            string name, 
            int materialIndex,
            VertexPositionNormalTexture[] vertices,
            ModelFaceContent[] faces,
            ModelBoneContent[] bones = null,
            ModelAnimationContent[] animations = null)
        {
            this.Name = name;
            this.MaterialIndex = materialIndex;
            this.Vertices = vertices;
            this.Faces = faces;
            this.Bones = bones;
            this.Animations = animations;
        }

        public override string ToString()
        {
            return $"Name: { Name }, Material Index: { MaterialIndex }, Vertices Count: { Vertices.Length }, Faces Count: { Faces.Length }, " +
                   $"Bones Count: { Bones.Length }, Animations Count: { Animations.Length }";
        }
    }
    
    public class ModelMaterialContent
    {

    }

    public class ModelContent
    {
        public readonly ModelMeshContent[] Meshes;

        public ModelContent(
            ModelMeshContent[] meshes)
        {
            if (meshes == null) throw new ArgumentNullException(nameof(meshes));

            Debug.Assert(meshes.Length > 0);

            this.Meshes = meshes;
        }

        public override string ToString()
        {
            return $"Meshes: { Meshes.Length }";
        }
    }
}
