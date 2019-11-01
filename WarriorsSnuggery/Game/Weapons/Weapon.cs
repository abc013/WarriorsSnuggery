using System;
using System.Linq;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class Weapon : PhysicsObject
	{
		protected readonly World World;
		protected readonly Actor Origin;
		public CPos Target;
		protected Actor TargetActor;
		protected readonly WeaponType Type;
		protected float DistanceMoved;
		protected int Speed;

		readonly float inaccuracyModifier = 1f;
		readonly float damageModifier = 1f;
		readonly float rangeModifier = 1f;

		public Weapon(World world, WeaponType type, Actor origin, CPos target) : this(world, type, origin.ActiveWeapon.WeaponOffsetPosition, target, origin)
		{
		}

		public Weapon(World world, WeaponType type, CPos origin, CPos target, Actor originActor = null) : base(origin, new IImageSequenceRenderable(type.Textures.GetTextures(), type.Textures.Tick), new Physics.SimplePhysics(origin, 0, type.PhysicalShape, type.PhysicalSize, type.PhysicalSize, type.PhysicalSize))
		{
			World = world;
			Type = type;
			Origin = originActor;
			if (type.Acceleration == 0)
				Speed = type.Speed;

			Target = target;

			if (Type.OrientateToTarget)
				Rotation = new VAngle(0, 0, -Position.AngleToXY(Target));

			if (originActor != null)
			{
				foreach (var effect in originActor.Effects.Where(e => e.Active && e.Spell.Type == Spells.EffectType.INACCURACY))
				{
					inaccuracyModifier *= effect.Spell.Value;
				}

				foreach (var effect in originActor.Effects.Where(e => e.Active && e.Spell.Type == Spells.EffectType.DAMAGE))
				{
					damageModifier *= effect.Spell.Value;
				}

				foreach (var effect in originActor.Effects.Where(e => e.Active && e.Spell.Type == Spells.EffectType.RANGE))
				{
					rangeModifier *= effect.Spell.Value;
				}
			}
		}

		public override void Tick()
		{
			base.Tick();

			if (Type.Acceleration != 0 && Speed != Type.Speed)
			{
				Speed += Type.Acceleration;
				if (Speed > Type.Speed)
					Speed = Type.Speed;
			}

			Move(Target);

			if (Type.OrientateToTarget)
				Rotation = new VAngle(0, 0, -Position.AngleToXY(Target));

			if (InRange(Target))
				Detonate();
		}

		public override void Render()
		{
			RenderShadow();
			base.Render();
		}

		public virtual void Move(CPos target)
		{
			var angle = target.AngleToXY(Position);

			var x = Math.Cos(angle) * Speed;
			var y = Math.Sin(angle) * Speed;
			double z;
			if (Type.WeaponFireType == WeaponFireType.ROCKET)
			{
				int zDiff;
				int dDiff;
				float angle2;
				if (TargetActor != null)
				{
					zDiff = Height - TargetActor.Height;
					dDiff = (int)Position.DistToXY(TargetActor.Position);
				}
				else
				{
					zDiff = Height;
					dDiff = (int)Position.DistToXY(Target);
				}
				angle2 = new CPos(dDiff, zDiff, 0).AngleToXY(CPos.Zero);
				z = Math.Sin(angle2) * Speed;
			}
			else
			{
				z = Type.Gravity;
			}

			var old = Position;
			// Note: we made sure that a weapon's target can't be out of world. (Actor.cs#87(Attack))
			Position = new CPos(Position.X + (int)x, Position.Y + (int)y, Position.Z);
			Physics.Position = Position;

			Height -= (int)z;
			if (Height < 0)
				Detonate();

			World.PhysicsLayer.UpdateSectors(this);

			if (World.CheckCollision(this, true, new[] { typeof(Weapon), typeof(BeamWeapon), typeof(BulletWeapon), typeof(RocketWeapon) }, new[] { Origin }))
				Detonate();

			DistanceMoved += Position.DistToXY(old);
			if (DistanceMoved > Type.MaxRange * rangeModifier || !World.IsInWorld(Position))
				Detonate();
		}

		public virtual bool InRange(CPos position, int range = 256)
		{
			return Position.DistToXY(position) <= range;
		}

		public virtual void Detonate(bool dispose = true)
		{
			foreach (var actor in World.Actors)
			{
				if (Origin != null && Origin.Team == actor.Team)
					continue;

				var dist = Position.DistToXY(actor.Position) / 512;
				if (dist > 128f) continue;
				if (dist < 1f) dist = 1;

				float damagemultiplier = 0f;

				switch (Type.DamageFalloff)
				{
					case FalloffType.LINEAR:
						damagemultiplier = 1 / (float)Math.Pow(1, dist);
						break;
					case FalloffType.QUADRATIC:
						damagemultiplier = 1 / (float)Math.Pow(2, dist);
						break;
					case FalloffType.CUBIC:
						damagemultiplier = 1 / (float)Math.Pow(3, dist);
						break;
					case FalloffType.EXPONENTIAL:
						damagemultiplier = 1 / (float)Math.Pow(5, dist);
						break;
					case FalloffType.ROOT:
						damagemultiplier = 1 / (float)Math.Sqrt(dist);
						break;
				}
				var damage = (int)Math.Floor(damagemultiplier * Type.Damage * damageModifier);

				if (damage < 2 || !actor.IsAlive)
					continue;

				if (actor.WorldPart != null && actor.WorldPart.ShowDamage)
					World.Add(new ActionText(actor.Position + new CPos(0, 0, 1024), new CPos(0, -15, 30), 50, ActionText.ActionTextType.SCALE, new Color(1f, 1 - (damage / (Type.Damage * 1.5f)), 0).ToString() + damage.ToString()));

				if (Origin != null)
					actor.Damage(Origin, damage);
				else
					actor.Damage(damage);
			}

			if (Type.Smudge != null && World.TerrainAt(Position) != null && World.TerrainAt(Position).Type.SpawnSmudge)
				World.Add(new Smudge(new CPos(Position.X, Position.Y, -512), new IImageSequenceRenderable(Type.Smudge.GetTextures(), Type.Smudge.Tick)));

			if (Type.ParticlesOnImpact != null)
			{
				foreach (var particle in Type.ParticlesOnImpact.Create(World, Position, Height))
				{
					World.Add(particle);
				}
			}

			if (dispose)
				Dispose();
		}

		protected CPos getInaccuracy()
		{
			if (Type.Inaccuracy > 0)
			{
				var ranX = (Program.SharedRandom.Next(Type.Inaccuracy) - Type.Inaccuracy / 2) * inaccuracyModifier;
				var ranY = (Program.SharedRandom.Next(Type.Inaccuracy) - Type.Inaccuracy / 2) * inaccuracyModifier;

				return new CPos((int)ranX, (int)ranY, 0);
			}

			return CPos.Zero;
		}
	}
}
