using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery.Objects.Actors
{
	public class PartManager
	{
		static readonly Func<Type, IPartList> createPartList = t =>
			(IPartList)typeof(PartList<>).MakeGenericType(t).GetConstructor(Type.EmptyTypes).Invoke(null);

		readonly List<ActorPart> parts = new List<ActorPart>();
		readonly Dictionary<Type, IPartList> partCache = new Dictionary<Type, IPartList>();

		public PartManager() { }

		public void Add(ActorPart part)
		{
			parts.Add(part);

			foreach (var type in part.GetType().GetInterfaces())
				innerAdd(type, part);
		}

		void innerAdd(Type type, ActorPart part)
		{
			if (!partCache.ContainsKey(type))
				partCache.Add(type, createPartList(type));

			partCache[type].Add(part);
		}

		public T GetPartOrDefault<T>()
		{
			object firstOrDefault<P>()
			{
				var type = typeof(P);

				return parts.FirstOrDefault(p => p.GetType() == type);
			}

			return (T)firstOrDefault<T>();
		}

		public T GetPart<T>()
		{
			object first<P>()
			{
				var type = typeof(P);

				return parts.First(p => p.GetType() == type);
			}

			var obj = first<T>();
			return obj == null ? default : (T)obj;
		}

		public List<T> GetParts<T>()
		{
			var type = typeof(T);

			if (!partCache.ContainsKey(type))
				throw new Exception($"Tried to get invalid type '{type}' from a PartManager.");

			return ((PartList<T>)partCache[type]).Get();
		}

		public List<T> GetPartsOrDefault<T>()
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
