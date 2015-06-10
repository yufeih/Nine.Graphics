namespace Nine.Graphics
{
    using System;
    using System.Diagnostics;

    public struct Slice<T> : IEquatable<Slice<T>>
    {
        public readonly T[] Items;
        public readonly int Begin;
        public readonly int End;

        public int Count => End - Begin;

        public T this[int index] => Items[Begin + index];

        public Slice(T[] items)
        {
            Debug.Assert(items != null);

            this.Items = items;
            this.Begin = 0;
            this.End = items.Length;
        }

        public Slice(T[] items, int begin)
        {
            Debug.Assert(items != null);
            Debug.Assert(begin >= 0 && begin < items.Length);

            this.Items = items;
            this.Begin = begin;
            this.End = items.Length;
        }

        public Slice(T[] items, int begin, int end)
        {
            Debug.Assert(items != null);
            Debug.Assert(begin >= 0 && begin < items.Length);
            Debug.Assert(end >= 0 && end < items.Length);
            Debug.Assert(end >= begin);

            this.Items = items;
            this.Begin = begin;
            this.End = end;
        }

        public static implicit operator Slice<T>(T[] array) => new Slice<T>(array);

        public bool Equals(Slice<T> other) => Items == other.Items && Begin == other.Begin && End == other.End;
        public override bool Equals(object obj) => obj is Slice<T> && Equals((Slice<T>)obj);
        public override int GetHashCode() => Items.GetHashCode() ^ (Begin.GetHashCode() << 8) ^ (End.GetHashCode() << 16);

        public override string ToString() => $"{ typeof(T[]).Name } [{ Count }]({ Begin } - { End })";
    }
}
