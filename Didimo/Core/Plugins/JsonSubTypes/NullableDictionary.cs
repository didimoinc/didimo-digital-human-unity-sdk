using System;
using System.Collections.Generic;

namespace JsonSubTypes
{
    public class NullableDictionary<TKey, TValue>
    {
        private bool hasNullKey;
        private TValue nullKeyValue;
        private readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (Equals(key, default(TKey)))
            {
                if (!hasNullKey)
                {
                    value = default(TValue);
                    return false;
                }

                value = nullKeyValue;
                return true;
            }

            return dictionary.TryGetValue(key, out value);
        }

        public void Add(TKey key, TValue value)
        {
            if (Equals(key, default(TKey)))
            {
                if (hasNullKey)
                {
                    throw new ArgumentException();
                }

                hasNullKey = true;
                nullKeyValue = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        public IEnumerable<TKey> NotNullKeys()
        {
            return dictionary.Keys;
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Entries()
        {
            if (hasNullKey)
            {
                yield return new KeyValuePair<TKey, TValue>(default(TKey), nullKeyValue);
            }

            foreach (var value in dictionary)
            {
                yield return value;
            }
        }
    }
}
