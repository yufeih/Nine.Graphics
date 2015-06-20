namespace Nine.Graphics
{
    using System;
    using System.Collections.Concurrent;

    class IdMap
    {
        private int count = 1;
        private string[] idToName = new string[128];
        private readonly ConcurrentDictionary<string, int> nameToId = new ConcurrentDictionary<string, int>();

        public int Count => count;
        public string this[int id] => idToName[id];

        public int this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name)) return 0;

                return nameToId.GetOrAdd(name, key =>
                {
                    lock (nameToId)
                    {
                        if (idToName.Length <= count)
                        {
                            Array.Resize(ref idToName, count);
                        }
                        idToName[count] = name;
                        return count++;
                    }
                });
            }
        }
    }
}
