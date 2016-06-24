namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// https://github.com/dotnet/roslyn/issues/120
    /// </summary>
    public struct Slice<T> : IEquatable<Slice<T>>, IReadOnlyList<T>
    {
        public readonly T[] Items;
        public readonly int Begin;
        public readonly int End;
        public readonly int Length;

        public T this[int index] => Items[Begin + index];

        public Slice(T[] items)
        {
            Debug.Assert(items != null);

            this.Items = items;
            this.Begin = 0;
            this.End = items.Length;
            this.Length = End - Begin;
        }

        public Slice(T[] items, int begin)
        {
            Debug.Assert(items != null);
            Debug.Assert(begin >= 0 && begin < items.Length);

            this.Items = items;
            this.Begin = begin;
            this.End = items.Length;
            this.Length = End - Begin;
        }

        public Slice(T[] items, int begin, int count)
        {
            Debug.Assert(items != null);
            Debug.Assert(begin >= 0 && begin < items.Length);
            Debug.Assert(count >= 0 && count <= items.Length - begin);

            this.Items = items;
            this.Begin = begin;
            this.End = begin + count;
            this.Length = count;
        }

        public static implicit operator Slice<T>(T[] array) => new Slice<T>(array);

        public bool Equals(Slice<T> other) => Items == other.Items && Begin == other.Begin && End == other.End;
        public override bool Equals(object obj) => obj is Slice<T> && Equals((Slice<T>)obj);
        public override int GetHashCode() => Items.GetHashCode() ^ (Begin.GetHashCode() << 8) ^ (End.GetHashCode() << 16);

        public override string ToString() => $"{ typeof(T[]).Name } [{ Length }]({ Begin } - { End })";

        public Enumerator GetEnumerator() => new Enumerator { index = Begin - 1, items = Items, begin = Begin, end = End };

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        int IReadOnlyCollection<T>.Count => Length;

        public struct Enumerator : IEnumerator<T>
        {
            internal T[] items;
            internal int index;
            internal int begin;
            internal int end;

            public T Current => items[index];

            object IEnumerator.Current => items[index];

            public bool MoveNext() => ++index < end;
            public void Reset() => index = begin;

            public void Dispose() { }
        }
    }
}
