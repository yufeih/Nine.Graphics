namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class ObjectPool
    {
        private readonly int defaultCapacity;

        // Pre-defined pools for faster access
        private readonly ObjectPool<Sprite> spritePool;

        private readonly IReadOnlyDictionary<Type, ObjectPoolConfig> configs;
        private readonly Dictionary<Type, object> poolsByType = new Dictionary<Type, object>();

        public ObjectPool(int defaultCapacity = 128, IReadOnlyDictionary<Type, ObjectPoolConfig> configs = null)
        {
            this.configs = configs;
            this.defaultCapacity = defaultCapacity;
            this.spritePool = Create(2048, Sprite.Default);
        }

        /// <summary>
        /// Gets the object pool for a given type with an initial capacity ensured.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to be pooled
        /// </typeparam>
        /// <param name="capacity">
        /// The minimum capacity that the result pool should have.
        /// </param>
        /// <returns>
        /// An object pool to put objects in.
        /// </returns>
        public ObjectPool<T> For<T>(int capacity) where T : struct
        {
            object result;

            var type = typeof(T);
            if (type == typeof(Sprite))
                result = spritePool;
            else if (!poolsByType.TryGetValue(type, out result))
                result = poolsByType[type] = Create(Math.Max(capacity, defaultCapacity), default(T));

            var pool = (ObjectPool<T>)result;
            pool.EnsureCapacity(capacity);
            return pool;
        }

        private ObjectPool<T> Create<T>(int capacity, T defaultValue) where T : struct
        {
            ObjectPoolConfig config;
            configs.TryGetValue(typeof(T), out config);
            return new ObjectPool<T>(
                config != null ? config.Capacity : capacity,
                config != null ? (T)config.Default : defaultValue);
        }
    }

    public class ObjectPool<T> where T : struct
    {
        private readonly T defaultValue;
        private T[] items;

        public T this[int index]
        {
            set { items[index] = value; }
        }

        internal ObjectPool(int capacity, T defaultValue)
        {
            this.items = new T[capacity];
            this.defaultValue = defaultValue;
        }

        internal void EnsureCapacity(int capacity)
        {
            if (items.Length < capacity)
            {
                Array.Resize(ref items, capacity);
            }

            for (var i = 0; i < capacity; i++)
            {
                items[capacity] = defaultValue;
            }
        }
    }

    public class ObjectPoolConfig
    {
        public readonly int Capacity;
        public readonly object Default;

        public ObjectPoolConfig(int capacity, object defaultValue)
        {
            Debug.Assert(defaultValue != null);
            Debug.Assert(capacity > 0);

            this.Capacity = capacity;
            this.Default = defaultValue;
        }
    }
}
