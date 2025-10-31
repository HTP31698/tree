using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ProbingStrategy
{
    Linear,

    Quadracic,
    //Double 은 직접 구현하기
    DoubleHash,
}
public class OpenAddressingHashTable<Tkey, TValue> : IDictionary<Tkey, TValue>
{
    private const int DefaultCapacity = 16;
    private const double LoadFactor = 0.6;
    //Linear
    private KeyValuePair<Tkey, TValue>[] table;
    private bool[] occupied;
    private bool[] deleted;
    private int size;
    private int count;
    private ProbingStrategy probingStrategy;

    public OpenAddressingHashTable(ProbingStrategy startegy = ProbingStrategy.Linear)
    {
        table = new KeyValuePair<Tkey, TValue>[DefaultCapacity];
        occupied = new bool[DefaultCapacity];
        deleted = new bool[DefaultCapacity];
        size = DefaultCapacity;
        count = 0;

        probingStrategy = startegy;
    }

    public void Resize()
    { 
        var oldTable = table;
        var oldOccupied = occupied;
        var oldDeleted = deleted;
        var oldSize = size;

        size = size * 2;

        table = new KeyValuePair<Tkey, TValue>[size];
        occupied = new bool[size];
        deleted = new bool[size];
        count = 0;

        for (int i = 0; i < oldSize; ++i)
        {
            if (oldOccupied[i] && !oldDeleted[i])
            {
                Add(oldTable[i].Key, oldTable[i].Value);    
            }
        }
    }
    // 이중해싱 구조라 두개 만든다.
    public int GetPrimaryHash(Tkey key)
    {
        if (key == null)
            throw new ArgumentNullException();

        int hash = key.GetHashCode();
        return Mathf.Abs(hash) % size;
    }
    public int GetSecondaryHash(Tkey key)
    {
        int hash = key.GetHashCode();
        // 0이 반환되지 않도록 1을 더함
        return 1 + (Math.Abs(hash) % (size - 1));
    }

    public int GetProbeIndex(Tkey key, int attempt)
    {
        int primaryHash = GetPrimaryHash(key);
        int primarySecondHash = GetSecondaryHash(key);

        switch (probingStrategy)
        { 
            case ProbingStrategy.Linear:
                return(primaryHash + attempt) % size;
            case ProbingStrategy.Quadracic:
                return (primaryHash + attempt * attempt) % size;
                //이 아래는 알아서 구현
            case ProbingStrategy.DoubleHash:
                return (primaryHash + attempt * primarySecondHash) % size;
        }

        throw new ArgumentException(nameof(key));
    }


    public TValue this[Tkey key] 
    {
        get
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }
            throw new KeyNotFoundException();
        }
        set
        {
            if (key == null)
                throw new ArgumentNullException();

            if ((double)count / size >= LoadFactor)
            {
                Resize();
            }

            int attempt = 0;

            do
            {
                int index = GetProbeIndex(key, attempt);
                if (!occupied[index] || deleted[index])
                {
                    table[index] = new KeyValuePair<Tkey, TValue>(key, value);
                    occupied[index] = true;
                    deleted[index] = false;
                    ++count;
                    return;
                }
                if (table[index].Key.Equals(key))
                {
                    table[index] = new KeyValuePair<Tkey, TValue>(key, value);
                    return;
                }
                ++attempt;

                if (attempt > size)
                {
                    Resize();
                    attempt = 0;
                }
            }
            while (true);
        }
    }

    public ICollection<Tkey> Keys => Enumerable.Range(0, size)
        .Where(i => occupied[i] && !deleted[i])
        .Select(i => table[i].Key)
        .ToList();
    public ICollection<TValue> Values => Enumerable.Range(0, size)
        .Where(i => occupied[i] && !deleted[i])
        .Select(i => table[i].Value)
        .ToList();

    public int Count => count;

    public bool IsReadOnly => false;

    private int FindIndex(Tkey key)
    { 
        if(key == null) 
            throw new ArgumentNullException();

        int attempt = 0;
        do
        {
            int index = GetProbeIndex(key, attempt);
            if (!occupied[index] && !deleted[index])
            {
                return -1;
            }

            if (occupied[index] && !deleted[index] && table[index].Key.Equals(key))
            {
                return index;
            }
            ++attempt;
        }while (attempt < size);
        return -1;
    }

    public void Add(Tkey key, TValue value)
    {
        if (key == null)
            throw new ArgumentNullException();

        if ((double)count / size >= LoadFactor)
        {
            Resize();
        }

        int attempt = 0;

        do
        {
            int index = GetProbeIndex(key, attempt);
            if (!occupied[index] || deleted[index])
            {
                table[index] = new KeyValuePair<Tkey, TValue>(key, value);
                occupied[index] = true;
                deleted[index] = false;
                ++count;
                return;
            }
            if (table[index].Key.Equals(key))
            {
                throw new ArgumentException("키 중복");
            
            }
            ++attempt;

            if (attempt > size)
            {
                Resize();
                attempt = 0;
            }
        }
        while (true);
    }

    public void Add(KeyValuePair<Tkey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        Array.Clear(table, 0, size);
        Array.Clear(occupied, 0, size);
        Array.Clear(deleted, 0, size);
        count = 0;
    }

    public bool Contains(KeyValuePair<Tkey, TValue> item)
    {
        int index = FindIndex(item.Key);
        if (index != -1)
        { 
            return table[index].Value.Equals(item.Value);
        }


        return false;
    }

    public bool ContainsKey(Tkey key)
    {
        return FindIndex(key) != -1;
    }

    public void CopyTo(KeyValuePair<Tkey, TValue>[] array, int arrayIndex)
    {
        int index = arrayIndex;
        foreach (var kvp in this)
        {
            array[index++] = kvp;
        }
    }

    public IEnumerator<KeyValuePair<Tkey, TValue>> GetEnumerator()
    {
        for(int i = 0; i <size; ++i)
        {
            if (occupied[i] && !deleted[i])
            {
                yield return table[i];
            }
        }
    }

    public bool Remove(Tkey key)
    {
        int index = FindIndex(key);
        if (index != -1)
        {
            //실제로 삭제하지 않아야 깨지지 않는다.
            deleted[index] = true;
            --count;
            return true;
        }
        return false;
    }

    public bool Remove(KeyValuePair<Tkey, TValue> item)
    {
        int index = FindIndex(item.Key);
        if (index != -1 && table[index].Value.Equals(item.Value))
        {
            //실제로 삭제하지 않아야 깨지지 않는다.
            deleted[index] = true;
            --count;
            return true;
        }
        return false;
    }

    public bool TryGetValue(Tkey key, out TValue value)
    {
        int index = FindIndex(key);

        if (index != -1)
        {
            value = table[index].Value;
            return true;
        }
        value = default;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void SetProbingStrategy(ProbingStrategy strategy)
    {
        probingStrategy = strategy;
    }
}
