using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons
{
	class BeamWeapon : Weapon
	{
		readonly BeamProjectileType projectileType;
		readonly RayPhysics rayPhysics;

		readonly Sound sound;
		BatchRenderable[] renderables;
		int renderabledistance;
		int tick;
		int curTick;
		int frame;

		CPos originPos;
		int originHeight;

		int impactInterval;
		int duration;
		int buildupduration;
		int endduration;

		public BeamWeapon(World world, WeaponType type, Target target, Actor origin, uint id) : base(world, type, target, origin, id)
		{
			projectileType = (BeamProjectileType)type.Projectile;
			impactInterval = projectileType.ImpactInterval;
			rayPhysics = new RayPhysics(world)
			{
				Target = TargetPosition,
				TargetHeight = TargetHeight
			};

			setPosition();

			duration = projectileType.BeamDuration;
			buildupduration = projectileType.StartupDuration;
			endduration = projectileType.CooldownDuration;

			if (buildupduration > 0 && projectileType.BeamStartUp != null)
				useTexture(projectileType.BeamStartUp);
			else
				useTexture(projectileType.Beam);

			if (projectileType.BeamSound != null)
			{
				sound = new Sound(projectileType.BeamSound);
				sound.Play(originPos, true);
			}
		}

		public BeamWeapon(World world, WeaponInit init) : base(world, init)
		{
			projectileType = (BeamProjectileType)Type.Projectile;
			rayPhysics = new RayPhysics(world)
			{
				Target = TargetPosition,
				TargetHeight = TargetHeight
			};

			originPos = init.Convert("OriginPosition", TargetPosition);
			originHeight = init.Convert("OriginHeight", TargetHeight);
			setPosition();

			impactInterval = init.Convert("ImpactInterval", projectileType.ImpactInterval);
			duration = init.Convert("Duration", projectileType.BeamDuration);
			buildupduration = init.Convert("BuildupDuration", projectileType.StartupDuration);
			endduration = init.Convert("EndDuration", projectileType.CooldownDuration);

			if (buildupduration > 0 && projectileType.BeamStartUp != null)
				useTexture(projectileType.BeamStartUp);
			else
				useTexture(projectileType.Beam);

			if (projectileType.BeamSound != null)
			{
				sound = new Sound(projectileType.BeamSound);
				sound.Play(originPos, true);
			}
		}

		void useTexture(TextureInfo texture)
		{
			frame = 0;
			tick = texture.Tick;
			renderabledistance = 1024 * texture.Height / MasterRenderer.PixelSize;
			curTick = 0;

			var sprite = texture.GetTextures();
			renderables = new BatchRenderable[sprite.Length];
			for (int i = 0; i < sprite.Length; i++)
				renderables[i] = new BatchObject(sprite[i], Color.White);
		}

		public override void Render()
		{
			var distance = (originPos - GraphicPosition - new CPos(0, originHeight, -originHeight)).FlatDist;
			var angle = (originPos - GraphicPosition - new CPos(0, originHeight, -originHeight)).FlatAngle;
			var fit = distance / renderabledistance;

			var curFrame = frame;
			for (int i = 0; i < fit; i++)
			{
				var renderable = renderables[curFrame];

				var posX = (int)(Math.Cos(angle) * i * renderabledistance);
				var posY = (int)(Math.Sin(angle) * i * renderabledistance);

				renderable.SetRotation(new VAngle(0, 0, -angle) + new VAngle(0, 0, 270));
				renderable.SetPosition(originPos + new CPos(posX, posY, 0) - new CPos(0, originHeight, -originHeight));
				renderable.PushToBatchRenderer();

				curFrame--;
				if (curFrame < 0)
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
				useTexture(projectileType.Beam);

			if (duration == 0 && projectileType.BeamCooldown != null)
				useTexture(projectileType.BeamCooldown);

			if (curTick-- < 0)
			{
				frame++;
				if (frame >= renderables.Length)
					frame = 0;

				curTick = tick;
			}

			setPosition();

			rayPhysics.Target = TargetPosition;
			rayPhysics.TargetHeight = TargetHeight;
			rayPhysics.CalculateEnd(new[] { Origin });
			Position = rayPhysics.End;
			Height = rayPhysics.EndHeight;

			var dist = (originPos - Position).SquaredFlatDist;

			sound?.SetPosition(originPos + (Position - originPos) / new CPos(2, 2, 1));

			if (dist > (Type.MaxRange * RangeModifier) * (Type.MaxRange * RangeModifier))
			{
				var angle = (originPos - TargetPosition).FlatAngle;
				Position = originPos + new CPos((int)(Math.Cos(angle) * Type.MaxRange * RangeModifier), (int)(Math.Sin(angle) * Type.MaxRange * RangeModifier), 0);
				Height = 0;
			}

			if (projectileType.Directed && dist > (originPos - TargetPosition).SquaredFlatDist)
			{
				Position = TargetPosition;
				Height = 0;
			}

			if (duration > 0 && buildupduration <= 0 && impactInterval-- <= 0)
			{
				Detonate(new Target(Position, Height), false);
				impactInterval = projectileType.ImpactInterval;
			}

			if (buildupduration < 0 && duration-- < 0 && endduration-- < 0)
				Dispose();
		}

		void setPosition()
		{
			if (Origin != null)
			{
				originHeight = Origin.ActiveWeapon.WeaponHeightPosition;
				originPos = Origin.ActiveWeapon.WeaponOffsetPosition;
			}
			rayPhysics.Start = originPos;
			rayPhysics.StartHeight = originHeight;
		}

		public void Move(CPos target, int height)
		{
			var dHeight = height - TargetHeight;
			if (dHeight > projectileType.MovementSpeed)
				TargetHeight += projectileType.MovementSpeed;
			else if (dHeight < -projectileType.MovementSpeed)
				TargetHeight -= projectileType.MovementSpeed;
			else
				TargetHeight += dHeight;

			var dTarget = target - TargetPosition;
			if (dTarget.SquaredFlatDist <= projectileType.MovementSpeed * projectileType.MovementSpeed)
			{
				TargetPosition = target;
				return;
			}

			var angle = dTarget.FlatAngle;
			var dx = (int)-(Math.Cos(angle) * projectileType.MovementSpeed);
			var dy = (int)-(Math.Sin(angle) * projectileType.MovementSpeed);
			TargetPosition += new CPos(dx, dy, 0);

		}

		public override List<string> Save()
		{
			var list = base.Save();

			list.Add("OriginPosition=" + originPos);
			list.Add("OriginHeight=" + originHeight);
			list.Add("ImpactInterval=" + impactInterval);
			list.Add("Duration=" + duration);
			list.Add("BuildupDuration=" + buildupduration);
			list.Add("EndDuration=" + endduration);

			return list;
		}

		public override void Dispose()
		{
			base.Dispose();
			sound?.Stop();
		}
	}
}
