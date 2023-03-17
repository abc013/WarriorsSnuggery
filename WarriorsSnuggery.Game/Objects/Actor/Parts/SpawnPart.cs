﻿using System.Collections.Generic;
using System;
using WarriorsSnuggery.Audio.Sound;
using WarriorsSnuggery.Conditions;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects.Weapons;
using System.Linq;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	public enum Occasion
	{
		DAMAGE,
		DEATH,
		TICK
	}

	public enum SpawnPartTypes
	{
		ACTOR,
		WEAPON,
		NONE
	}

	[Desc("Spawns objects when the object takes damage.", "Without the health rule, this rule is useless.")]
	public class SpawnPartInfo : PartInfo
	{
		[Desc("Probability that the object will be spawned.")]
		public readonly float Probability = 1f;
		[Desc("Count of spawned objects.")]
		public readonly int Count;
		[Desc("Time distance between spawn of the objects in ticks.", "Used for the TICK occasion.")]
		public readonly int Tick;
		[Desc("Name of the object.")]
		public readonly string Name;

		[Desc("Object will inherit Team from the dead object.")]
		public readonly bool InheritsTeam;
		[Desc("Object will inherit Bot from the dead object.")]
		public readonly bool InheritsBot;

		[Desc("Type of the object.")]
		public readonly SpawnPartTypes Type = SpawnPartTypes.NONE;
		[Desc("Condition to spawn.")]
		public readonly Condition Condition;
		[Desc("Defines when the objects should be spawned.")]
		public readonly Occasion Occasion = Occasion.TICK;

		[Desc("Offset from the center of idling object where the objects spawn.", "Z-coordinate will be used for height.")]
		public readonly CPos Offset;
		[Desc("Radius in which the objects get spawned randomly.", "If set to 0, physics radius will be used when possible.")]
		public readonly int Radius;
		[Desc("Threshold for damage concerning the DAMAGE occasion.")]
		public readonly int DamageThreshold = 2;

		[Desc("Sound to play when spawning.")]
		public readonly SoundType Sound;

		[Desc("Spawn object at center of actor, not random.")]
		public readonly bool AtCenter;

		public SpawnPartInfo(PartInitSet set) : base(set) { }

		public override ActorPart Create(Actor self)
		{
			return new SpawnPart(self, this);
		}
	}

	public class SpawnPart : ActorPart, ITick, INoticeDamage, INoticeKilled, ISaveLoadable
	{
		readonly SpawnPartInfo info;
		int curTick;

		public SpawnPart(Actor self, SpawnPartInfo info) : base(self)
		{
			this.info = info;
		}

		public void OnLoad(PartLoader loader)
		{
			foreach (var node in loader.GetNodes(typeof(SpawnPart), info.InternalName))
			{
				if (node.Key == "Tick")
					curTick = node.Convert<int>();
			}
		}

		public PartSaver OnSave()
		{
			var saver = new PartSaver(this, info.InternalName);

			saver.Add("Tick", curTick, 0);

			return saver;
		}

		public void OnDamage(Actor damager, int damage)
		{
			if (info.Occasion == Occasion.DAMAGE && damage > info.DamageThreshold)
				create();
		}

		public void OnKilled(Actor killer)
		{
			if (info.Occasion == Occasion.DEATH)
				create();
		}

		public void Tick()
		{
			if (info.Occasion == Occasion.TICK && curTick-- < 0)
				create();
		}

		void create()
		{
			if (info.Condition == null || info.Condition.True(self))
			{
				curTick = info.Tick;
				if (info.Sound != null)
				{
					var sound = new Sound(info.Sound);
					sound.Play(self.Position, false);
				}

				for (int i = 0; i < info.Count; i++)
					createObject();

				return;
			}

			curTick = 0;
		}

		void createObject()
		{
			if (self.World.Game.SharedRandom.NextDouble() > info.Probability)
				return;

			switch (info.Type)
			{
				case SpawnPartTypes.ACTOR:
					Actor actor = null;

					if (!info.AtCenter)
					{
						var radius = info.Radius == 0 ? Math.Max(self.Physics.Boundaries.X, self.Physics.Boundaries.Y) : info.Radius;
						var types = new List<ActorType>() { ActorCache.Types[info.Name] };
						var actors = ActorDistribution.DistributeAround(self.World, self.Position + info.Offset, radius, types, info.InheritsTeam ? self.Team : Actor.NeutralTeam, info.InheritsBot && self.IsBot);

						actor = actors.FirstOrDefault();
					}

					if (actor == null)
					{
						actor = ActorCache.Create(self.World, info.Name, randomPosition(), info.InheritsTeam ? self.Team : Actor.NeutralTeam, info.InheritsBot && self.IsBot);
						self.World.Add(actor);
					}

					if (info.InheritsBot && self.IsBot)
						actor.Bot.Target = self.Bot.Target;
					break;
				case SpawnPartTypes.WEAPON:
					var weapon = WeaponCache.Create(self.World, info.Name, new Target(randomPosition()), self);

					self.World.Add(weapon);
					break;
				default:
					return;
			}
		}

		CPos randomPosition()
		{
			if (info.AtCenter)
				return self.Position + info.Offset;

			var sizeX = info.Radius;
			var sizeY = info.Radius;

			if (info.Radius == 0 && !self.Physics.IsEmpty)
			{
				sizeX = self.Physics.Boundaries.X;
				sizeY = self.Physics.Boundaries.Y;
			}

			var x = self.World.Game.SharedRandom.Next(-sizeX, sizeX);
			var y = self.World.Game.SharedRandom.Next(-sizeY, sizeY);
			return self.Position + new CPos(x, y, 0) + info.Offset;
		}
	}
}
