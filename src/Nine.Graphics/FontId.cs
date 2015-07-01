namespace Nine.Graphics
{
    using System;

    public struct FontId : IEquatable<FontId>
    {
        public readonly short Id;

        public string Name => map[Id];

        public static int Count => map.Count;

        private static readonly IdMap map = new IdMap();

        public FontId(string name) { Id = (short)map[name]; }

        public static implicit operator FontId(string name) => new FontId(name);
        public static implicit operator string(FontId textureId) => textureId.Name;

        public bool Equals(FontId other) => other.Id == Id;

        public override bool Equals(object obj) => obj is FontId && Id == ((FontId)obj).Id;
        public override int GetHashCode() => Id;

        public override string ToString() => $"[{ Id }] { Name }";
    }
}
