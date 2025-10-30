using System;
using System.Collections.Generic;
using UnityEngine;

public class TileQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
{
    private List<(TElement Element, TPriority Priority)> heap;

    public TileQueue()
    {
        heap = new List<(TElement, TPriority)>();
    }

    public int Count => heap.Count;

    public void Enqueue(TElement element, TPriority priority)
    {
        heap.Add((element, priority));
        HeapifyUp(heap.Count - 1);
    }

    public TElement Dequeue()
    {
        if (Count == 0)
            throw new InvalidOperationException();

        var temp = heap[0].Element;
        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);

        if (heap.Count > 0)
        {
            HeapifyDown(0);
        }

        return temp;
    }

    public TElement Peek()
    {
        if (Count == 0)
            throw new InvalidOperationException();

        return heap[0].Element;
    }

    public void Clear()
    {
        heap.Clear();
    }

    private void HeapifyUp(int index)
    {
        int parentIndex;
        while (index > 0)
        {
            parentIndex = (index - 1) / 2;
            if (heap[index].Priority.CompareTo(heap[parentIndex].Priority) < 0)
            {
                var temp = heap[index];
                heap[index] = heap[parentIndex];
                heap[parentIndex] = temp;

                index = parentIndex;
            }
            else
            {
                break;
            }
        }
    }

    private void HeapifyDown(int index)
    {
        int leftChildIndex;
        int rightChildIndex;
        int smallerChildIndex;

        while (index < heap.Count)
        {
            leftChildIndex = index * 2 + 1;
            rightChildIndex = index * 2 + 2;

            if (leftChildIndex >= heap.Count)
                break;

            if (rightChildIndex >= heap.Count)
            {
                smallerChildIndex = leftChildIndex;
            }
            else
            {
                smallerChildIndex = heap[leftChildIndex].Priority
                   .CompareTo(heap[rightChildIndex].Priority) < 0 ? leftChildIndex : rightChildIndex;
            }

            if (heap[index].Priority.CompareTo(heap[smallerChildIndex].Priority) > 0)
            {
                var temp = heap[index];
                heap[index] = heap[smallerChildIndex];
                heap[smallerChildIndex] = temp;

                index = smallerChildIndex;
            }
            else
            {
                break;
            }
        }
    }
}