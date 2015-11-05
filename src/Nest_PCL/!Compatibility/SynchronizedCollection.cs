using System;
using System.Collections.Generic;
using System.Threading;

namespace System.Collections.Generic
{
    internal class SynchronizedCollection<T> : IList<T>
    {
        List<T> inner = new List<T>();
        public readonly object SyncRoot;

        public int Count
        {
            get
            {
                lock (SyncRoot)
                    return inner.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public T this[int index]
        {
            get
            {
                lock(SyncRoot)
                    return inner[index];
            }

            set
            {
                lock (SyncRoot)
                    inner[index] = value;
            }
        }

        public SynchronizedCollection(object syncRoot)
        {
            this.SyncRoot = syncRoot;
        }

        public SynchronizedCollection() : this(new object())
        {

        }

        public int IndexOf(T item)
        {
            lock (SyncRoot)
                return inner.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            lock (SyncRoot)
                inner.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            lock (SyncRoot)
                inner.RemoveAt(index);
        }

        public void Add(T item)
        {
            lock (SyncRoot)
                inner.Add(item);
        }

        public void Clear()
        {
            lock (SyncRoot)
                inner.Clear();
        }

        public bool Contains(T item)
        {
            lock (SyncRoot)
                return inner.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot)
                inner.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            lock (SyncRoot)
                return inner.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            Monitor.Enter(SyncRoot);
            try
            {
                foreach (var item in inner)
                {
                    yield return item;
                }
            }
            finally
            {
                Monitor.Exit(SyncRoot);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}