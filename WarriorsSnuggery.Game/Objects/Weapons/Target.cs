using System;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Objects.Weapons
{
	public enum TargetType : byte
	{
		ACTOR,
		POSITION
	}

	public class Target : ISaveable
	{
		public readonly TargetType Type;

		public CPos Position
		{
			get
			{
				if (Type == TargetType.POSITION)
					return position;

				return Actor.Position;
			}
		}
		readonly CPos position;

		public readonly Actor Actor;
		public Target(CPos target)
		{
			position = target;
			Type = TargetType.POSITION;
		}

		public Target(Actor target)
		{
			if (target == null)
				throw new NullReferenceException($"Tried to create actor target, but targeted actor is null.");

			Actor = target;
			Type = TargetType.ACTOR;
		}

		public Target(TextNodeInitializer initializer, World world)
		{
			var actorID = initializer.Convert(nameof(Actor), uint.MaxValue);
			Actor = world.ActorLayer.ToAdd().FirstOrDefault(a => a.ID == actorID);
			Type = TargetType.ACTOR;
			if (Actor == null)
			{
				position = initializer.Convert(nameof(Position), CPos.Zero);
				Type = TargetType.POSITION;
			}
		}

		public TextNodeSaver Save()
		{
			var saver = new TextNodeSaver();

			if (Actor != null)
				saver.Add(nameof(Actor), Actor.ID);
			else
				saver.Add(nameof(Position), Position, CPos.Zero);

			return saver;
		}
	}
}
