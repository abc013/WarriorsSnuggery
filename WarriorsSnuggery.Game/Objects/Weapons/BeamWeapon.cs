using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Weapons.Projectiles;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons
{
	class BeamWeapon : Weapon
	{
		readonly BeamProjectile projectile;
		readonly RayPhysics rayPhysics;

		readonly Sound sound;
		BatchRenderable[] renderables;
		int renderabledistance;
		int tick;
		int curTick;
		int frame;

		[Save("OriginPosition")]
		CPos originPos;
		[Save("OriginHeight")]
		int originHeight;

		[Save("ImpactInterval")]
		int impactInterval;
		[Save("Duration")]
		int duration;
		[Save("BuildupDuration")]
		int buildupduration;
		[Save("EndDuration")]
		int endduration;

		public BeamWeapon(World world, WeaponType type, Target target, Actor origin, uint id) : base(world, type, target, origin, id)
		{
			projectile = (BeamProjectile)type.Projectile;
			impactInterval = projectile.ImpactInterval;
			rayPhysics = new RayPhysics(world)
			{
				Target = TargetPosition,
				TargetHeight = TargetHeight
			};

			setPosition();

			duration = type.ShootDuration;
			buildupduration = projectile.StartupDuration;
			endduration = projectile.CooldownDuration;

			if (buildupduration > 0 && projectile.BeamStartUp != null)
				useTexture(projectile.BeamStartUp);
			else
				useTexture(projectile.Beam);

			if (projectile.BeamSound != null)
			{
				sound = new Sound(projectile.BeamSound);
				sound.Play(originPos, true);
			}
		}

		public BeamWeapon(World world, WeaponInit init) : base(world, init)
		{
			projectile = (BeamProjectile)Type.Projectile;
			rayPhysics = new RayPhysics(world)
			{
				Target = TargetPosition,
				TargetHeight = TargetHeight
			};

			originPos = init.Convert("OriginPosition", TargetPosition);
			originHeight = init.Convert("OriginHeight", TargetHeight);
			setPosition();

			impactInterval = init.Convert("ImpactInterval", projectile.ImpactInterval);
			duration = init.Convert("Duration", Type.ShootDuration);
			buildupduration = init.Convert("BuildupDuration", projectile.StartupDuration);
			endduration = init.Convert("EndDuration", projectile.CooldownDuration);

			if (buildupduration > 0 && projectile.BeamStartUp != null)
				useTexture(projectile.BeamStartUp);
			else
				useTexture(projectile.Beam);

			if (projectile.BeamSound != null)
			{
				sound = new Sound(projectile.BeamSound);
				sound.Play(originPos, true);
			}
		}

		void useTexture(TextureInfo texture)
		{
			frame = 0;
			tick = texture.Tick;
			renderabledistance = 1024 * texture.Height / Constants.PixelSize;
			curTick = 0;

			var sprite = texture.GetTextures();
			renderables = new BatchRenderable[sprite.Length];
			for (int i = 0; i < sprite.Length; i++)
				renderables[i] = new BatchObject(sprite[i]);
		}

		public override void Render()
		{
			var originGraphicPosition = originPos - new CPos(0, originHeight, -originHeight);
			var distance = originGraphicPosition - GraphicPosition;
			var angle = distance.FlatAngle;
			var fit = distance.FlatDist / renderabledistance;

			var offset = CPos.FromFlatAngle(angle, renderabledistance);

			var curFrame = frame;
			for (int i = 0; i < fit; i++)
			{
				var renderable = renderables[curFrame];

				var pos = new CPos(offset.X * i, offset.Y * i, 0);

				renderable.SetRotation(new VAngle(0, 0, 270 - angle));
				renderable.SetPosition(originGraphicPosition + pos);
				renderable.Render();

				if (curFrame-- <= 0)
					curFrame = renderables.Length - 1;
			}

			if (Settings.DeveloperMode)
				rayPhysics.RenderDebug();
		}

		public override void Tick()
		{
			if (Disposed || World.Game.Editor)
				return;

			if (buildupduration-- == 0)
				useTexture(projectile.Beam);

			if (duration == 0 && projectile.BeamCooldown != null)
				useTexture(projectile.BeamCooldown);

			if (curTick-- < 0)
			{
				if (frame++ >= renderables.Length - 1)
					frame = 0;

				curTick = tick;
			}

			setPosition();

			rayPhysics.Target = TargetPosition;
			rayPhysics.TargetHeight = TargetHeight;
			rayPhysics.CalculateEnd(new[] { Origin.Physics });
			Position = rayPhysics.End;
			Height = rayPhysics.EndHeight;

			var dist = (originPos - Position).SquaredFlatDist;

			if (dist > (Type.MaxRange * RangeModifier) * (Type.MaxRange * RangeModifier))
			{
				Position = clampToMaxRange(originPos, (originPos - TargetPosition).FlatAngle);
				Height = 0;
			}

			if (projectile.Directed && dist > (originPos - TargetPosition).SquaredFlatDist)
			{
				Position = TargetPosition;
				Height = 0;
			}

			var distance = originPos - Position;

			sound?.SetPosition(Position + distance / new CPos(2, 2, 1));

			if (duration > 0 && buildupduration <= 0)
			{
				if (projectile.BeamParticles != null && duration % projectile.BeamParticleTick == 0)
				{
					var angle = distance.FlatAngle;
					var fit = distance.FlatDist / projectile.BeamParticleDistance;
					var heightdiff = (originHeight - Height) / projectile.BeamParticleDistance;

					var offset = CPos.FromFlatAngle(angle, projectile.BeamParticleDistance);

					for (int i = 0; i < fit; i++)
						World.Add(projectile.BeamParticles.Create(World, originPos + new CPos(offset.X * i, offset.Y * i, 0), originHeight + heightdiff * i));
				}

				if (impactInterval-- <= 0)
				{
					Detonate(new Target(Position, Height), false);
					impactInterval = projectile.ImpactInterval;
				}
			}

			if (buildupduration < 0 && duration-- < 0 && endduration-- < 0)
				Dispose();
		}

		public override bool CheckVisibility()
		{
			return CameraVisibility.IsVisible(GraphicPosition) || CameraVisibility.IsVisible(originPos);
		}

		void setPosition()
		{
			if (Origin != null)
			{
				originHeight = Origin.Weapon.WeaponHeightPosition;
				originPos = Origin.Weapon.WeaponOffsetPosition;
			}
			rayPhysics.Start = originPos;
			rayPhysics.StartHeight = originHeight;
		}

		public void Move(CPos target, int height)
		{
			TargetHeight = height;

			if (Position == target)
				return;

			var length = (originPos - target).FlatDist;

			var oldangle = (originPos - TargetPosition).FlatAngle;
			var newangle = (originPos - target).FlatAngle;

			var diff = WarriorsSnuggery.Angle.Diff(oldangle, newangle);

			diff = Math.Clamp(-diff, -projectile.ArcTurnSpeed, projectile.ArcTurnSpeed);

			TargetPosition = originPos + CPos.FromFlatAngle(oldangle + diff, length);
		}

		public override List<string> Save()
		{
			var list = base.Save();
			list.AddRange(SaveAttribute.GetFields(this, false));

			return list;
		}

		public override void Dispose()
		{
			base.Dispose();
			sound?.Stop();
		}
	}
}
