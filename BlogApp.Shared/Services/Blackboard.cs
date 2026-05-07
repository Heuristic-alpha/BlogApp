namespace BlogApp.Services
{
    /// <summary>
    /// Blackboard is in memory KVP[string, object] storage
    /// </summary>
    public class Blackboard
    {
        private readonly Dictionary<string, object> _context;

        public Blackboard()
        {
            _context = new Dictionary<string, object>();
        }

        public bool TryGetValue<T>(string key, out T value)
        {
#pragma warning disable CS8600
            if (_context.TryGetValue(key, out object o) && o is T cast)
            {
                value = cast;
                return true;
            }
            value = default(T)!;
            return false;
#pragma warning restore CS8600
        }

        public T GetValue<T>(string key)
        {
            T ret;
            if (TryGetValue<T>(key, out ret))
            {
                return ret;
            }
            else
            {
                throw new System.Exception($"[Blackboard]: Cant find value with key( {key} )");
            }
        }

        /// <summary>
        /// Sets the value for the specified key in the current context. If the key already exists, its value is updated.
        /// </summary>
        /// <param name="key">The key to set or update. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="value">The value to associate with the specified key.</param>
        public void SetValue(string key, object value)
        {
            if (!_context.TryAdd(key, value))
            {
                _context[key] = value;
            }
        }

        /// <summary>
        /// Determines whether the specified key exists in the current context.
        /// </summary>
        /// <param name="key">The key to search for. Cannot be <see langword="null"/> or empty.</param>
        /// <returns><see langword="true"/> if the key exists in the current context; otherwise, <see langword="false"/>.</returns>
        public bool ContainsKey(string key)
        {
            return _context.ContainsKey(key);
        }

        /// <summary>
        /// Removes the value associated with the specified key from the current context.
        /// </summary>
        /// <param name="key">The key to remove. Cannot be <see langword="null"/> or empty.</param>
        public void RemoveKey(string key)
        {
            _context.Remove(key);
        }
    }
}
