using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimpleHashTable<TKey, TValue> : IDictionary<TKey, TValue>
{
    private const int DefaultCapacity = 16;
    //적재율
    private const double LoadFactor = 0.75f;

    //배열 < 기본
    private KeyValuePair<TKey, TValue>[] table;

    //해당 배열에 들어가있냐? 안들어가있냐?
    private bool[] occuiped;

    //배열의 사이즈
    private int size;

    //배열의 카운트
    private int count;

    public SimpleHashTable()
    {
        table = new KeyValuePair<TKey, TValue>[DefaultCapacity];
        occuiped = new bool[DefaultCapacity];
        size = DefaultCapacity;
        count = 0;
    }

    //혼자 만든거 겟사이즈
    public int GetSize()
    { 
        return size;
    }
    public int GetCount()
    {
        return count;
    }

    //해시함수
    private int GetIndex(TKey key, int size)
    {
        if (key == null)
        {
            throw new System.ArgumentNullException();
        }

        int hash = key.GetHashCode();
        return Mathf.Abs(hash) % size;
    }

    //해시함수
    private int GetIndex(TKey key)
    {
        return GetIndex(key, this.size);
    }


    //인덱서 구현
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
            {
                throw new ArgumentNullException(nameof(key));
            }

            int index = GetIndex(key);
            //있는지 없는지 검사 index만 하는게 아니라 테이블 키와 인섹서로 넘어오는 키를 검사
            if (occuiped[index] && table[index].Key.Equals(key))
            {
                //value는 index에서 넘어오는 value이다.
                table[index] = new KeyValuePair<TKey, TValue>(key, value);
            }
            //슬롯이 비어 있을때 add해주는 경우
            else if (!occuiped[index])
            {
                table[index] = new KeyValuePair<TKey, TValue>(key, value);
                occuiped[index] = true;
                ++count;
            }
            //key는 같지가 않은데 occuiped가 true인 상황 해시충돌
            else
            {
                throw new InvalidOperationException("해시 충돌!");
            }

        }
    }

    //링큐로 구현했었음
    public ICollection<TKey> Keys => Enumerable.Range(0, size)
        .Where(i => occuiped[i])
        .Select(i => table[i].Key)
        .ToList();
    public ICollection<TValue> Values => Enumerable.Range(0, size)
        .Where(i => occuiped[i])
        .Select(i => table[i].Value)
        .ToList();

    public int Count => count;

    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        //null 체크는 알아서 하도록 / 해쉬충돌도
        //적재율 확인
        if ((double)count / size >= LoadFactor)
        {
            Resize();
        }
        int index = GetIndex(key);
        //사용 안되는 상황
        if (!occuiped[index])
        {
            table[index] = new KeyValuePair<TKey, TValue>(key, value);
            occuiped[index] = true;
            ++count;
        }
        else if (table[index].Key.Equals(key))
        {
            throw new ArgumentException("키 중복");
        }
        else
        {
            throw new InvalidOperationException("해시 충돌");
        }
    }

    public void Resize()
    {
        //resize는 일반적으로 x2
        int newSize = size * 2;
        var newTable = new KeyValuePair<TKey, TValue>[newSize];
        var newOccuiped = new bool[newSize];

        //순회
        for (int i = 0; i < size; ++i)
        {
            if (!occuiped[i])
                continue;

            int newIndex = GetIndex(table[i].Key, newSize);

            if (newOccuiped[newIndex])
            { 
               throw new InvalidOperationException("해시 충돌");
            }

            newTable[newIndex] = table[i];
            newOccuiped[newIndex] = true;
        }

        size = newSize;
        table = newTable;
        occuiped = newOccuiped;
    }


    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        Array.Clear(table, 0, size);
        Array.Clear(occuiped, 0, size);
        count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return ContainsKey(item.Key);
    }

    public bool ContainsKey(TKey key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        int index = GetIndex(key);
        //테이블에 진짜 키가 들어 있는지까지 검사할 것
        return occuiped[index] && table[index].Key.Equals(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (arrayIndex <= size)
        {
            array[arrayIndex] = table[arrayIndex];
        }
        throw new ArgumentOutOfRangeException("사이즈 초과");
    }

    //IDictionary를 상속받아서
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < size; i++)
        {
            if (occuiped[i])
            {
                yield return table[i];
            }
        }
    }

    //index 검사해서 있으면 삭제
    public bool Remove(TKey key)
    {
        if (key == null)
        { 
            throw new ArgumentNullException(nameof(key));
        }

        int index = GetIndex(key);
        //테이블의 키가 지우려는 키와 동일한지
        if (occuiped[index] && table[index].Key.Equals(key))
        {
            occuiped[index] = false;
            table[index] = default;
            --count;
            return true;
        }
        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }

    //value가 있으면 true 없으면 false return
    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null)
        { 
            throw new ArgumentNullException(nameof(key));
        }

        int index = GetIndex(key);
        if (occuiped[index] && table[index].Key.Equals(key))
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
}
