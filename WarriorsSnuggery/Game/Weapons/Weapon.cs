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

		public Weapon(World world, WeaponType type, CPos origin, CPos target, Actor originActor = null) : base(origin, new IImageSequenceRenderable(type.Texture.GetTextures(), type.Texture.Tick), new Physics.SimplePhysics(origin, 0, type.PhysicalShape, type.PhysicalSize, type.PhysicalSize, type.PhysicalSize))
		{
			World = world;
			Type = type;
			Origin = originActor;
			if (type.Acceleration == 0)
				Speed = type.Speed;

			Target = target;

			if (Type.OrientateToTarget)
				Rotation = new VAngle(0, 0, -(Target - Position).FlatAngle);

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
				Rotation = new VAngle(0, 0, -(Target - Position).FlatAngle);

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
			var angle = (Position - target).FlatAngle;

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
					dDiff = (int)(Position - TargetActor.Position).FlatDist;
				}
				else
				{
					zDiff = Height;
					dDiff = (int)(Position - Target).FlatDist;
				}
				angle2 = new CPos(-dDiff, -zDiff, 0).FlatAngle;
				z = Math.Sin(angle2) * Speed;
			}
			else
			{
				z = Type.Gravity;
			}

			var old = Position;
			Position = new CPos(Position.X + (int)x, Position.Y + (int)y, Position.Z);
			Physics.Position = Position;

			Height -= (int)z;
			if (Height < 0)
				Detonate();

			World.PhysicsLayer.UpdateSectors(this);

			if (World.CheckCollision(this, true, new[] { typeof(Weapon), typeof(BeamWeapon), typeof(BulletWeapon), typeof(RocketWeapon) }, new[] { Origin }))
				Detonate();

			DistanceMoved += (Position - old).FlatDist;
			if (DistanceMoved > Type.MaxRange * rangeModifier || !World.IsInWorld(Position))
				Detonate();
		}

		public virtual bool InRange(CPos position, int range = 256)
		{
			return (Position - position).FlatDist <= range;
		}

		public virtual void Detonate(bool dispose = true)
		{
			if (Type.Damage != 0)
			{
				foreach (var actor in World.Actors)
				{
					if (!actor.IsAlive || actor.Health == null || Origin != null && Origin.Team == actor.Team)
						continue;

					var dist = (Position - actor.Position).FlatDist / 512;
					if (dist > 32f) continue;
					if (dist < 1f) dist = 1;

					float damagemultiplier = getDamageMultiplier(dist);
					var damage = (int)Math.Floor(damagemultiplier * Type.Damage * damageModifier);

					if (damage == 0)
						continue;

					if (actor.WorldPart != null && actor.WorldPart.ShowDamage)
						World.Add(new ActionText(actor.Position + new CPos(0, 0, 1024), new CPos(0, -15, 30), 50, ActionText.ActionTextType.SCALE, new Color(1f, 1 - (damage / (Type.Damage * 1.5f)), 0).ToString() + damage.ToString()));

					if (Origin != null)
						actor.Damage(Origin, damage);
					else
						actor.Damage(damage);
				}
			}

			if (Type.WallDamage != 0)
			{
				foreach (var wall in World.WallLayer.Walls)
				{
					if (wall == null || wall.Type.Invincible)
						continue;

					var dist = (Position - wall.Position).FlatDist / 512;
					if (dist > 32f) continue;
					if (dist < 1f) dist = 1;

					float damagemultiplier = getDamageMultiplier(dist);
					var damage = (int)Math.Floor(damagemultiplier * Type.WallDamage * damageModifier);

					if (damage == 0)
						continue;

					wall.Damage(damage);
				}
			}

			if (Type.Smudge != null && World.TerrainAt(Position) != null && World.TerrainAt(Position).Type.SpawnSmudge)
				World.Add(new Smudge(new CPos(Position.X, Position.Y, -512), new IImageSequenceRenderable(Type.Smudge.GetTextures(), Type.Smudge.Tick)));

			if (Type.ParticlesOnImpact != null)
			{
				foreach (var particle in Type.ParticlesOnImpact.Create(World, Position, Type.WeaponFireType == WeaponFireType.BEAM ? 0 : Height))
				{
					World.Add(particle);
				}
			}

			if (dispose)
				Dispose();
		}

		float getDamageMultiplier(float dist)
		{
			switch (Type.Falloff)
			{
				case FalloffType.LINEAR:
					return 1 / dist;
				case FalloffType.QUADRATIC:
					return 1 / (dist * dist);
				case FalloffType.CUBIC:
					return 1 / (dist * dist * dist);
				case FalloffType.EXPONENTIAL:
					return 1 / (float)Math.Pow(2, dist);
				case FalloffType.ROOT:
					return 1 / (float)Math.Sqrt(dist);
				default:
					return 1;
			}
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
