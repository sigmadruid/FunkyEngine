using System.Collections.Generic;

namespace Funky.Utility
{
    public class ObjectPool<T> where T : class, new()
    {
        private readonly Stack<T> _pool = new();

        public T Allocate()
        {
            return _pool.Count > 0 ? _pool.Pop() : new T();
        }

        public void Recycle(T o)
        {
            if (o != null)
                _pool.Push(o);
        }

        public void Clear()
        {
            _pool.Clear();
        }

        public int Size => _pool.Count;
    }
}