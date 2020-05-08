using OpenToolkit.Graphics.ES20;
using System;
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

		public CPos OriginPos;
		public int OriginHeight;
		int curTick;
		int frame;

		int impactInterval;
		int duration;
		int buildupduration;
		int endduration;

		public BeamWeapon(World world, WeaponType type, Target target, Actor origin) : base(world, type, target, origin)
		{
			projectileType = (BeamProjectileType)type.Projectile;
			impactInterval = projectileType.ImpactInterval;
			rayPhysics = new RayPhysics(world)
			{
				Start = OriginPos,
				Target = TargetPosition,
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
				sound.Play(OriginPos, true);
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
			var distance = (OriginPos - GraphicPosition - new CPos(0, OriginHeight, -OriginHeight)).FlatDist;
			var angle = (OriginPos - GraphicPosition - new CPos(0, OriginHeight, -OriginHeight)).FlatAngle;
			var fit = distance / renderabledistance;

			var curFrame = frame;
			for (int i = 0; i < fit; i++)
			{
				var renderable = renderables[curFrame];

				var posX = (int)(Math.Cos(angle) * i * renderabledistance);
				var posY = (int)(Math.Sin(angle) * i * renderabledistance);

				renderable.SetRotation(new VAngle(0, 0, -angle) + new VAngle(0, 0, 270));
				renderable.SetPosition(OriginPos + new CPos(posX, posY, 0) - new CPos(0, OriginHeight, -OriginHeight));
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
			if (Disposed)
				return;

			if (buildupduration-- == 0)
				useTexture(projectileType.Beam);

			if (duration == 0 && projectileType.BeamCooldown != null)
				useTexture(projectileType.BeamCooldown);

			curTick--;
			if (curTick < 0)
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

			var dist = (OriginPos - Position).FlatDist;

			sound?.SetPosition(OriginPos + (Position - OriginPos) / new CPos(2, 2, 1));

			if (dist > Type.MaxRange * RangeModifier)
			{
				var angle = (OriginPos - TargetPosition).FlatAngle;
				Position = OriginPos + new CPos((int)(Math.Cos(angle) * Type.MaxRange * RangeModifier), (int)(Math.Sin(angle) * Type.MaxRange * RangeModifier), 0);
				Height = 0;
			}

			if (projectileType.Directed && dist > (OriginPos - TargetPosition).FlatDist)
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
			OriginHeight = Origin.ActiveWeapon.WeaponHeightPosition;
			OriginPos = Origin.ActiveWeapon.WeaponOffsetPosition;
			rayPhysics.Start = OriginPos;
			rayPhysics.StartHeight = OriginHeight;
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
			if (dTarget.Dist <= projectileType.MovementSpeed)
			{
				TargetPosition = target;
				return;
			}

			var angle = dTarget.FlatAngle;
			var dx = (int)-(Math.Cos(angle) * projectileType.MovementSpeed);
			var dy = (int)-(Math.Sin(angle) * projectileType.MovementSpeed);
			TargetPosition += new CPos(dx, dy, 0);

		}

		public override void Dispose()
		{
			base.Dispose();
			sound?.Stop();
		}
	}
}
