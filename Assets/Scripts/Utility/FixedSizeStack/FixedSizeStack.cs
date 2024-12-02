using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class FixedSizeStack<T> where T: class 
    {
        private T[] items;
        private int idx;
        private int _capacity;
        private int _count;
        public int Capacity => _capacity;
        public int Count => _count;

        public FixedSizeStack(int size)
        {
            _capacity = size;
            Clear();
        }
        
        public void Push(T item)
        {
            idx = (idx + 1) % Capacity;
            _count = Math.Min(Count + 1, Capacity);
            items[idx] = item;
        }

        public T Pop()
        {
            if (Count == 0)
            {
                return null;
            }

            T retItem = items[idx];
            idx = (idx - 1) == -1 ? Capacity - 1 : idx - 1;
            _count = Math.Max(Count - 1, 0);

            return retItem;
        }

        public void Clear()
        {
            items = new T[Capacity];
            idx = 0;
            _count = 0;
        }
    }
}
