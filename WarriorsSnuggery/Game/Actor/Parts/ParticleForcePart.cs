using System;
using System.Linq;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Objects.Parts
{
	public class ParticleForcePartInfo : PartInfo
	{
		[Desc("Only affect particles of the types given here.", "If empty, all will be affected.")]
		public readonly string[] AffectedTypes = new string[0];

		[Desc("Type of the force.")]
		public readonly ParticleForceType ForceType = ParticleForceType.FORCE;
		[Desc("Strength of the force.")]
		public readonly float Strength = 1f;

		[Desc("Maximum range of the force.")]
		public readonly int MaxRange;
		[Desc("Minimum range of the force.")]
		public readonly int MinRange;

		[Desc("Force will also affect rotation.")]
		public readonly bool AffectRotation = false;
		[Desc("Determines whether the force should only applied if the actor is a player.")]
		public readonly bool AffectOnlyWhenPlayer = false;

		public ParticleForcePartInfo(MiniTextNode[] nodes) : base(nodes) { }

		public override ActorPart Create(Actor self)
		{
			return new ParticleForcePart(self, this);
		}
	}

	public class ParticleForcePart : ActorPart
	{
		readonly ParticleForcePartInfo info;
		readonly ParticleForce force;
		readonly float maxRangesquared;

		public ParticleForcePart(Actor self, ParticleForcePartInfo info) : base(self)
		{
			this.info = info;
			maxRangesquared = info.MaxRange; // TODO use falloff
			force = new ParticleForce(info.ForceType, info.Strength);
		}

		public override void Tick()
		{
			if (info.MaxRange <= 0 || (info.AffectOnlyWhenPlayer && !self.IsPlayer))
				return;

			foreach(var obj in self.World.Objects)
			{
				if (!(obj is Particle particle) || !particle.AffectedByObjects)
					continue; // TODO cache affectable particles

				if (info.AffectedTypes.Length != 0 && !info.AffectedTypes.Contains(particle.Name))
					continue;

				var dist = particle.Position.DistToXY(self.GraphicPosition);
				if (dist > info.MaxRange || dist < info.MinRange)
					continue;

				var ratio = 1 - dist / maxRangesquared;

				particle.AffectVelocity(force, ratio, self.GraphicPosition);
				if (info.AffectRotation)
					particle.AffectRotation(force, ratio, self.GraphicPosition);
			}
		}
	}
}
