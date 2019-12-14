using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects
{
	class BeamWeapon : Weapon
	{
		ImageRenderable[] renderables;
		int renderabledistance;
		int tick;

		CPos originPos;
		int curTick;
		int frame;

		int impactInterval;
		int duration;
		int buildupduration;
		int endduration;
		readonly RayPhysics rayPhysics;

		public BeamWeapon(World world, WeaponType type, Actor origin, CPos target) : this(world, type, origin.ActiveWeapon.WeaponOffsetPosition, target, origin) { }

		public BeamWeapon(World world, WeaponType type, CPos origin, CPos target, Actor originActor = null) : base(world, type, origin, target, originActor)
		{
			originPos = origin;
			impactInterval = type.BeamImpactInterval;
			Target = target;
			rayPhysics = new RayPhysics(world)
			{
				Start = originPos,
				Target = target
			};

			duration = Type.BeamDuration;
			buildupduration = Type.BeamStartDuration;
			endduration = Type.BeamEndDuration;

			if (buildupduration > 0 && Type.TextureStart != null)
				useTexture(Type.TextureStart);
			else
				useTexture(Type.Texture);
		}

		void useTexture(TextureInfo texture)
		{
			frame = 0;
			tick = texture.Tick;
			renderabledistance = 1024 * texture.Height / MasterRenderer.PixelSize;
			curTick = 0;

			var sprite = texture.GetTextures();
			renderables = new ImageRenderable[sprite.Length];
			for (int i = 0; i < sprite.Length; i++)
			{
				renderables[i] = new ImageRenderable(sprite[i]);
			}
		}

		public override void Render()
		{
			var distance = (originPos - Position).FlatDist;
			var angle = (originPos - Position).FlatAngle;
			var fit = distance / renderabledistance;

			var curFrame = frame;
			for (int i = 0; i < fit; i++)
			{
				var renderable = renderables[curFrame];

				var posX = (int)(Math.Cos(angle) * i * renderabledistance);
				var posY = (int)(Math.Sin(angle) * i * renderabledistance);

				renderable.SetRotation(new VAngle(0, 0, -angle) + new VAngle(0, 0, 270));
				renderable.SetPosition(originPos + new CPos(posX, posY, 0));
				renderable.Render();

				curFrame--;
				if (curFrame < 0)
					curFrame = renderables.Length - 1;
			}
		}

		public override void Tick()
		{
			if (buildupduration-- == 0)
				useTexture(Type.Texture);

			if (duration == 0)
			{
				if (Type.TextureEnd != null)
					useTexture(Type.TextureEnd);
				else
					useTexture(Type.Texture);
			}

			curTick--;
			if (curTick < 0)
			{
				frame++;
				if (frame >= renderables.Length)
					frame = 0;

				curTick = tick;
			}

			if (Origin != null)
			{
				originPos = Origin.ActiveWeapon.WeaponOffsetPosition;
				rayPhysics.Start = originPos;
			}

			rayPhysics.Target = Target;
			rayPhysics.CalculateEnd(Origin);
			Position = rayPhysics.End;

			var dist = (originPos - Position).FlatDist;

			if (Type.WeaponFireType == WeaponFireType.DIRECTEDBEAM && dist > (originPos - Target).FlatDist)
				Position = Target;

			if (dist > Type.MaxRange)
			{
				var angle = (originPos - Target).FlatAngle;
				Position = originPos + new CPos((int)(Math.Cos(angle) * Type.MaxRange), (int)(Math.Sin(angle) * Type.MaxRange), 0);
			}

			if (duration > 0 && buildupduration <= 0 && impactInterval-- <= 0)
			{
				Detonate(false);
				impactInterval = Type.BeamImpactInterval;
			}

			if (Type.OrientateToTarget)
				Rotation = new VAngle(0, 0,(Position - Target).FlatAngle);

			if (buildupduration < 0 && duration-- < 0 && endduration-- < 0)
					Dispose();
		}

		public override void Dispose()
		{
			base.Dispose();
		}
	}
}
