using Lagrange.Milky.Extension;

namespace Lagrange.Milky.Utility.Cache;

public sealed class FifoCache<TKey, TValue>(int capacity) : ICache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity = capacity;
    private readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _cache = [];
    private readonly LinkedList<KeyValuePair<TKey, TValue>> _sorted = [];

    private readonly ReaderWriterLockSlim _lock = new();

    public TValue? Get(TKey key)
    {
        using (_lock.UsingReadLock())
        {
            if (!_cache.TryGetValue(key, out LinkedListNode<KeyValuePair<TKey, TValue>>? node)) return default;

            return node.Value.Value;
        }
    }

    public void Put(TKey key, TValue value)
    {
        using (_lock.UsingWriteLock())
        {
            if (_cache.TryGetValue(key, out LinkedListNode<KeyValuePair<TKey, TValue>>? node))
            {
                node.Value = new KeyValuePair<TKey, TValue>(key, value);
                _sorted.Remove(node);
                _sorted.AddFirst(node);
            }
            else
            {
                if (_cache.Count == _capacity)
                {
                    var oldNode = _sorted.Last;
                    if (oldNode != null)
                    {
                        _sorted.RemoveLast();
                        _cache.Remove(oldNode.Value.Key);
                    }
                }

                KeyValuePair<TKey, TValue> item = new(key, value);
                node = _sorted.AddFirst(item);
                _cache.Add(key, node);
            }
        }
    }
}