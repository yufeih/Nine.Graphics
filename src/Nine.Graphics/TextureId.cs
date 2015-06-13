namespace Nine.Graphics
{
    using System;
    using System.Collections.Concurrent;

    public struct TextureId : IEquatable<TextureId>
    {
        public readonly int Id;

        public string Name => names[Id];

        public bool IsMissing => Id == 0;

        public static int Count => count;

        private static int count = 1;
        private static string[] names = new string[128];
        private static readonly ConcurrentDictionary<string, int> textureIds = new ConcurrentDictionary<string, int>();

        public static readonly TextureId None = new TextureId();
        public static readonly TextureId White = new TextureId("n:white");
        public static readonly TextureId Black = new TextureId("n:black");
        public static readonly TextureId Error = new TextureId("n:error");
        public static readonly TextureId Missing = new TextureId("n:missing");
        public static readonly TextureId Transparent = new TextureId("n:transparent");

        public TextureId(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                this.Id = 0;
            }
            else
            {
                this.Id = textureIds.GetOrAdd(name, key =>
                {
                    lock (textureIds)
                    {
                        if (names.Length <= count)
                        {
                            Array.Resize(ref names, count);
                        }
                        names[count] = name;
                        return count++;
                    }
                });
            }
        }

        public static implicit operator TextureId(string name) => new TextureId(name);
        public static implicit operator string(TextureId textureId) => textureId.Name;

        public bool Equals(TextureId other) => other.Id == Id;

        public override bool Equals(object obj) => obj is TextureId && Id == ((TextureId)obj).Id;
        public override int GetHashCode() => Id;

        public override string ToString() => $"[{ Id }] { Name }";
    }
}
