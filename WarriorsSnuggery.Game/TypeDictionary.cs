using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery
{
	public class TypeDictionary<T>
	{
		readonly Dictionary<string, T> types = new Dictionary<string, T>();

		public void Add(string key, T value)
		{
			if (types.ContainsKey(key))
				throw new ArgumentException($"The key '{key}' has already been added to the dictionary (Type '{typeof(T).Name}').");

			types.Add(key, value);
		}

		public T this[string key]
		{
			get
			{
				if (!ContainsKey(key))
					throw new MissingFieldException($"The key '{key}' does not exist.");

				return types[key];
			}
		}

		public string this[T value]
		{
			get
			{
				if (!ContainsValue(value))
					throw new Exception($"Unable to find value of type '{typeof(T).Name}' in the dictionary.");

				return types.First(t => t.Value.Equals(value)).Key;
			}
		}

		public Dictionary<string, T>.KeyCollection Keys => types.Keys;
		public Dictionary<string, T>.ValueCollection Values => types.Values;

		public bool ContainsKey(string key) => types.ContainsKey(key);
		public bool ContainsValue(T value) => types.ContainsValue(value);
	}
}
