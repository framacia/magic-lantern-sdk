using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS.Utilities
{
    public static class CollectionUtility
    {
        public static void AddItem<K,V>(this SerializableDictionary<K, List<V>> serializableDictionary, K key, V value)
        {
            //If key already exists, add the value to it
            if (serializableDictionary.ContainsKey(key))
            {
                serializableDictionary[key].Add(value);

                return;
            }

            //If key does not exist, create along with list of value including the value
            serializableDictionary.Add(key, new List<V>() { value });
        }
    }
}