namespace Nine.Graphics
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    public struct TextureId : IEquatable<TextureId>
    {
        public readonly string Name;
        public readonly int Id;

        public bool IsMissing => Id == 0;

        public static readonly TextureId Missing = new TextureId();

        public static int Count => count;

        private static int count;
        private static readonly ConcurrentDictionary<string, int> textureIds = new ConcurrentDictionary<string, int>();

        public TextureId(string name)
        {
            this.Name = name;
            this.Id = textureIds.GetOrAdd(name, key => Interlocked.Increment(ref count) + 1);
        }

        public static implicit operator TextureId(string name) => new TextureId(name);
        public static implicit operator string(TextureId textureId) => textureId.Name;

        public bool Equals(TextureId other) => other.Id == Id;

        public override bool Equals(object obj) => obj is TextureId && Id == ((TextureId)obj).Id;
        public override int GetHashCode() => Id;

        public override string ToString() => $"[{ Id }] { Name }";
    }
}
