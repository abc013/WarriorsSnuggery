﻿using System;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Objects.Weapons
{
	public enum TargetType : byte
	{
		ACTOR,
		POSITION
	}

	public class Target
	{
		public readonly TargetType Type;

		public CPos Position
		{
			get
			{
				if (Type == TargetType.POSITION)
					return position;
				else
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
	}
}
