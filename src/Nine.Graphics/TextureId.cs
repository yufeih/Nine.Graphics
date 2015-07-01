namespace Nine.Graphics
{
    using System;

    public struct TextureId : IEquatable<TextureId>
    {
        public readonly int Id;

        public string Name => map[Id];

        public static int Count => map.Count;

        private static readonly IdMap map = new IdMap();

        public static readonly TextureId None = new TextureId();
        public static readonly TextureId White = new TextureId("n:white");
        public static readonly TextureId Black = new TextureId("n:black");
        public static readonly TextureId Error = new TextureId("n:error");
        public static readonly TextureId Missing = new TextureId("n:missing");
        public static readonly TextureId Transparent = new TextureId("n:transparent");

        public TextureId(string name) { Id = map[name]; }

        public static implicit operator TextureId(string name) => new TextureId(name);
        public static implicit operator string(TextureId textureId) => textureId.Name;

        public bool Equals(TextureId other) => other.Id == Id;

        public override bool Equals(object obj) => obj is TextureId && Id == ((TextureId)obj).Id;
        public override int GetHashCode() => Id;

        public override string ToString() => $"[{ Id }] { Name }";
    }
}
