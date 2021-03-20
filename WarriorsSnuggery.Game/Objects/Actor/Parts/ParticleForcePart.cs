using System.Linq;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Objects.Parts
{
	public class ParticleForcePartInfo : PartInfo
	{
		[Desc("Only affect particles of the types given here.", "If empty, all will be affected.")]
		public readonly ParticleType[] AffectedTypes = new ParticleType[0];

		[Desc("Type of the force.")]
		public readonly ParticleForceType ForceType = ParticleForceType.FORCE;
		[Desc("Strength of the force.")]
		public readonly float Strength = 1f;
		[Desc("Also calculate height differences.")]
		public readonly bool UseHeight = true;

		[Desc("Maximum range of the force.")]
		public readonly int MaxRange;
		[Desc("Minimum range of the force.")]
		public readonly int MinRange;

		public readonly int MaxRangeSquared;
		public readonly int MinRangeSquared;

		[Desc("Force will also affect rotation.")]
		public readonly bool AffectRotation = false;
		[Desc("Determines whether the force should only applied if the actor is a player.")]
		public readonly bool AffectOnlyWhenPlayer = false;

		public ParticleForcePartInfo(PartInitSet set) : base(set)
		{
			MaxRangeSquared = MaxRange * MaxRange;
			MinRangeSquared = MinRange * MinRange;
		}

		public override ActorPart Create(Actor self)
		{
			return new ParticleForcePart(self, this);
		}
	}

	public class ParticleForcePart : ActorPart, ITick, INoticeMove
	{
		readonly ParticleForcePartInfo info;
		readonly ParticleForce force;

		ParticleSector[] sectors;

		public ParticleForcePart(Actor self, ParticleForcePartInfo info) : base(self)
		{
			this.info = info;
			force = new ParticleForce(info.ForceType, info.Strength, info.UseHeight);
			sectors = self.World.ParticleLayer.GetSectors(self.Position, info.MaxRange);
		}

		public void OnMove(CPos old, CPos speed)
		{
			sectors = self.World.ParticleLayer.GetSectors(self.Position, info.MaxRange);
		}

		public void Tick()
		{
			if (info.MaxRange <= 0 || (info.AffectOnlyWhenPlayer && !self.IsPlayer))
				return;

			foreach (var sector in sectors)
			{
				foreach (var particle in sector.Particles)
				{
					if (!particle.Type.AffectedByObjects)
						continue;

					var dist = (particle.Position - self.GraphicPosition).SquaredFlatDist;
					if (dist > info.MaxRangeSquared || dist < info.MinRangeSquared)
						continue;

					if (info.AffectedTypes.Length != 0 && !info.AffectedTypes.Contains(particle.Type))
						continue;

					var ratio = (float)(1 - dist / (double)info.MaxRangeSquared);

					// rather cache affections (as own class) in particle and then apply all at once, saving performance
					particle.AffectVelocity(force, ratio, self.GraphicPosition, self.Height);
					if (info.AffectRotation)
						particle.AffectRotation(force, ratio, self.GraphicPosition);
				}
			}
		}
	}
}
