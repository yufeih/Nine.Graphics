namespace Nine.Graphics
{
    using System;

    public struct ModelId : IEquatable<ModelId>
    {
        public readonly int Id;

        public string Name => map[Id];

        public static int Count => map.Count;

        private static readonly IdMap map = new IdMap();

        public static readonly ModelId None = new ModelId();
        public static readonly ModelId Error = new ModelId("n:error");
        public static readonly ModelId Missing = new ModelId("n:missing");

        public ModelId(string name) { Id = map[name]; }

        public static implicit operator ModelId(string name) => new ModelId(name);
        public static implicit operator string (ModelId textureId) => textureId.Name;

        public bool Equals(ModelId other) => other.Id == Id;

        public override bool Equals(object obj) => obj is ModelId && Id == ((ModelId)obj).Id;
        public override int GetHashCode() => Id;

        public override string ToString() => $"[{ Id }] { Name }";
    }
}
