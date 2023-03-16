using WarriorsSnuggery.Conditions;
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

		public override ActorPart Create(Actor self)
		{
			return new SpawnParticlesPart(self, this);
		}
	}

	public class SpawnParticlesPart : ActorPart, ITick, INoticeDamage, INoticeKilled, ISaveLoadable
	{
		readonly SpawnParticlesPartInfo info;
		int curTick;

		public SpawnParticlesPart(Actor self, SpawnParticlesPartInfo info) : base(self)
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
				self.World.Add(info.Particles.Create(self.World, self.Position));

			curTick = info.Tick;
		}
	}
}
