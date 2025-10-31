using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChainingHashTable<TKey, TValue> : IDictionary<TKey, TValue>
{
    private const int DefaultCapacity = 16;
    private const double LoadFactor = 0.6;

    //ChainingHash
    private LinkedList<KeyValuePair<TKey, TValue>>[] buckets;
    private int size;
    private int count;
    public ChainingHashTable()
    {
        buckets = new LinkedList<KeyValuePair<TKey, TValue>>[DefaultCapacity];
        size = DefaultCapacity;
        count = 0;
    }
    public TValue this[TKey key] 
    {
        get
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }
            throw new KeyNotFoundException("키 없음!");
        }
        set
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if ((double)count / size >= LoadFactor)
                Resize();

            int index = GetChaningHash(key);
            if (buckets[index] == null)
                buckets[index] = new LinkedList<KeyValuePair<TKey, TValue>>();

            for (var node = buckets[index].First; node != null; node = node.Next)
            {
                if (EqualityComparer<TKey>.Default.Equals(node.Value.Key, key))
                {
                    node.Value = new KeyValuePair<TKey, TValue>(key, value);
                    return;
                }
            }

            buckets[index].AddLast(new KeyValuePair<TKey, TValue>(key, value));
            count++;
        }
    }

    public ICollection<TKey> Keys => Enumerable.Range(0, size)
        .Where(i => buckets[i] != null)
        .Select(i => buckets[i].First.Value.Key)
        .ToList();

    public ICollection<TValue> Values => Enumerable.Range(0, size)
        .Where(i => buckets[i] != null)
        .Select(i => buckets[i].First.Value.Value)
        .ToList();

    public int Count => count;

    public bool IsReadOnly => false;


    public int GetChaningHash(TKey key)
    {
        if (key == null)
            throw new ArgumentNullException();

        int hash = key.GetHashCode();
        return Mathf.Abs(hash) % size;
    }
    private void Resize()
    {
        int newSize = size * 2;
        var newBuckets = new LinkedList<KeyValuePair<TKey, TValue>>[newSize];
        var oldBuckets = buckets;

        size = newSize;
        buckets = newBuckets;
        count = 0;

        for (int i = 0; i < oldBuckets.Length; ++i)
        {
            if (oldBuckets[i] != null)
            {
                foreach (var bucket in oldBuckets[i])
                {
                    Add(bucket.Key, bucket.Value);
                }
            }
        }
    }

    public void Add(TKey key, TValue value)
    {
        if (key == null) 
            throw new ArgumentNullException(nameof(key));

        if ((double)count / size >= LoadFactor)
            Resize();

        int index = GetChaningHash(key);
        if (buckets[index] == null)
            buckets[index] = new LinkedList<KeyValuePair<TKey, TValue>>();

        for (var node = buckets[index].First; node != null; node = node.Next)
        {
            if (EqualityComparer<TKey>.Default.Equals(node.Value.Key, key))
                throw new ArgumentException("키가 이미 존재합니다.", nameof(key));
        }

        buckets[index].AddLast(new KeyValuePair<TKey, TValue>(key, value));
        count++;
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        buckets = new LinkedList<KeyValuePair<TKey, TValue>>[DefaultCapacity];
        size = DefaultCapacity;
        count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        int index = GetChaningHash(item.Key);

        for (var node = buckets[index].First; node != null; node = node.Next)
        {
            if (EqualityComparer<TKey>.Default.Equals(node.Value.Key, item.Key)
                && EqualityComparer<TValue>.Default.Equals(node.Value.Value, item.Value))
                return true;

        }
        return false;
    }

    public bool ContainsKey(TKey key)
    {
        int index = GetChaningHash(key);

        for (var node = buckets[index].First; node != null; node = node.Next)
        {
            if (EqualityComparer<TKey>.Default.Equals(node.Value.Key, key))
                return true;

        }
        return false;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) 
            throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < count) 
            throw new ArgumentException("대상 배열이 작습니다.");

        int index = arrayIndex;
        for (int i = 0; i < size; i++)
        {
            var bucket = buckets[i];
            if (bucket == null) 
                continue;
            for (var n = bucket.First; n != null; n = n.Next)
                array[i++] = n.Value;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var bucket in buckets)
        {
            if (bucket != null)
            {
                foreach (var kvp in bucket)
                {
                    yield return kvp;
                }
            }
        }
    }

    public bool Remove(TKey key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        int index = GetChaningHash(key);

        if (buckets[index] == null)
            return false;

        for (var node = buckets[index].First; node != null; node = node.Next)
        {
            if (EqualityComparer<TKey>.Default.Equals(node.Value.Key, key))
            {
                buckets[index].Remove(node);
                --count;
                return true;
            }
        }

        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (item.Key == null)
            throw new ArgumentNullException(nameof(item.Key));

        int index = GetChaningHash(item.Key);

        if (buckets[index] == null)
            return false;

        for (var node = buckets[index].First; node != null; node = node.Next)
        {
            if (EqualityComparer<TKey>.Default.Equals(node.Value.Key, item.Key)
                &&EqualityComparer<TValue>.Default.Equals(node.Value.Value, item.Value))
            {
                buckets[index].Remove(node);
                --count;
                return true;
            }
        }

        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        int index = GetChaningHash(key);
        for (var node = buckets[index].First; node != null; node = node.Next)
        {
            if (EqualityComparer<TKey>.Default.Equals(node.Value.Key, key))
            {
                value = node.Value.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
