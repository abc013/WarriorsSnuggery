﻿using WarriorsSnuggery.Conditions;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Spawns objects when the object takes damage.", "Without the health rule, this rule is useless.")]
	public class SpawnParticlesPartInfo : PartInfo
	{
		[Desc("Particles to emit when spawning.")]
		public readonly ParticleSpawner Particles;

		[Desc("Condition to spawn.")]
		public readonly Condition Condition;

		[Desc("Defines the occasion when the particles should be spawned.")]
		public readonly Occasion Occasion = Occasion.TICK;
		[Desc("Threshold for damage concerning the DAMAGE occasion.")]
		public readonly int DamageThreshold;
		[Desc("Time distance between spawn of the particles in ticks.", "Used for the TICK occasion.")]
		public readonly int Tick;

		public SpawnParticlesPartInfo(PartInitSet set) : base(set) { }
	}

	public class SpawnParticlesPart : ActorPart, ITick, INoticeDamage, INoticeKilled, ISaveLoadable
	{
		readonly SpawnParticlesPartInfo info;
		[Save("Tick"), DefaultValue(0)]
		int curTick;

		public SpawnParticlesPart(Actor self, SpawnParticlesPartInfo info) : base(self, info)
		{
			this.info = info;
		}

		public void OnLoad(PartLoader loader)
		{
			loader.SetSaveFields(this);
		}

		public PartSaver OnSave()
		{
			var saver = new PartSaver(this);
			saver.AddSaveFields(this);

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
			if (info.Condition == null || info.Condition.True(Self))
				Self.World.Add(info.Particles.Create(Self.World, Self.Position));

			curTick = info.Tick;
		}
	}
}
