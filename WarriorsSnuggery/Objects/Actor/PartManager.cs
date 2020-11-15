using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery.Objects
{
	public class PartManager
	{
		static readonly Func<Type, IPartList> createPartList = t =>
			(IPartList)typeof(PartList<>).MakeGenericType(t).GetConstructor(Type.EmptyTypes).Invoke(null);

		public readonly List<ActorPart> Parts = new List<ActorPart>();

		readonly Dictionary<Type, IPartList> partCache = new Dictionary<Type, IPartList>();

		public PartManager() { }

		public void Add(ActorPart part)
		{
			Parts.Add(part);

			foreach (var type in part.GetType().GetInterfaces())
				innerAdd(type, part);
		}

		void innerAdd(Type type, ActorPart part)
		{
			if (!partCache.ContainsKey(type))
				partCache.Add(type, createPartList(type));

			partCache[type].Add(part);
		}

		public List<T> Get<T>()
		{
			var type = typeof(T);

			if (!partCache.ContainsKey(type))
				throw new Exception("Tried to get invalid type '{type}' from a PartManager.");

			return ((PartList<T>)partCache[type]).Get();
		}

		public List<T> GetOrDefault<T>()
		{
			var type = typeof(T);

			if (!partCache.ContainsKey(type))
				return new List<T>();

			return ((PartList<T>)partCache[type]).Get();
		}

		class PartList<T> : IPartList
		{
			readonly List<T> parts = new List<T>();

			public void Add(object part)
			{
				parts.Add((T)part);
			}

			public List<T> Get()
			{
				return parts;
			}
		}

		interface IPartList
		{
			void Add(object obj);
		}
	}
}
