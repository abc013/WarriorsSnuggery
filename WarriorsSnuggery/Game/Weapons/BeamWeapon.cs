/*
 * User: Andreas
 * Date: 25.11.2017
 * 
 */
using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects
{
	class BeamWeapon : Weapon
	{
		ImageRenderable[] renderables;
		readonly int renderabledistance;
		readonly int tick;
		CPos originPos;
		int curTick;
		int frame;

		int impactInterval;
		int duration;
		readonly RayPhysics rayPhysics;

		public BeamWeapon(World world, WeaponType type, CPos origin, CPos target, Action targetSetter = null) : base(world, type, origin, target)
		{
			Target += getInaccuracy();
			originPos = origin;
			tick = type.Textures.Tick;
			duration = type.Reload;
			impactInterval = type.BeamImpactInterval;
			rayPhysics = new RayPhysics(world)
			{
				Start = originPos,
				Target = target
			};

			renderabledistance = (1024 * type.Textures.Height) / 24;
			init();
		}

		public BeamWeapon(World world, WeaponType type, Actor origin, CPos target, Action targetSetter = null) : base(world, type, origin, target)
		{
			Target += getInaccuracy();
			originPos = origin.ActiveWeapon.WeaponOffsetPosition;
			tick = type.Textures.Tick;
			duration = 100;
			rayPhysics = new RayPhysics(world)
			{
				Start = originPos,
				Target = target
			};

			init();
			renderabledistance = (1024 * Type.Textures.Height) / 24;
		}

		void init()
		{
			var sprite = Type.Textures.GetTextures();
			renderables = new ImageRenderable[sprite.Length];
			for (int i = 0; i < sprite.Length; i++)
			{
				renderables[i] = new ImageRenderable(sprite[i]);
			}
			curTick = tick;
		}

		public override void Render()
		{
			var distance = originPos.Dist(Position);
			var angle = Position.Angle(originPos);
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

			if (Type.WeaponFireType == WeaponFireType.DIRECTEDBEAM && originPos.Dist(Position) > originPos.Dist(Target))
				Position = Target;

			if (impactInterval-- <= 0)
			{
				Detonate(false);
				impactInterval = Type.BeamImpactInterval;
			}

			if (Type.OrientateToTarget)
				Rotation = new VAngle(0, 0, -Position.Angle(Target));

			if (duration-- < 0)
			{
				Dispose();
			}
		}

		public override void Dispose()
		{
			//foreach (var renderable in renderables)
			//renderable.Dispose();

			base.Dispose();
		}
	}
}
