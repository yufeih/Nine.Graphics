namespace Nine.Graphics
{
    using System;

    public struct TextureId : IEquatable<TextureId>
    {
        public readonly string Name;
        public readonly int Id;

        public TextureId(string name)
        {
            this.Name = name;
            this.Id = 0;
        }

        public static implicit operator TextureId(string name) => new TextureId(name);
        public static implicit operator string(TextureId textureId) => textureId.Name;

        public bool Equals(TextureId other) => other.Id == Id;

        public override bool Equals(object obj) => obj is TextureId && Id == ((TextureId)obj).Id;
        public override int GetHashCode() => Id;

        public override string ToString() => $"[{ Id }] { Name }";
    }
}
