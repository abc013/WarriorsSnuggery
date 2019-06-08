using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	class BeamWeapon : Weapon
	{
		readonly ColoredCircleRenderable point;
		CPos originPos;
		readonly Color color;
		readonly Color color2;

		public BeamWeapon(World world, WeaponType type, CPos origin, CPos target) : base(world, type, origin, target)
		{
			Target += getInaccuracy();
			originPos = origin;
			color = Color.Red;
			color2 = Color.White;
			point = new ColoredCircleRenderable(color, 1f, 8, DrawMethod.TRIANGLEFAN);
			point.SetScale(0.5f);
		}

		public BeamWeapon(World world, WeaponType type, Actor origin, CPos target) : base(world, type, origin, target)
		{
			Target += getInaccuracy();
			originPos = origin.ActiveWeapon.WeaponOffsetPosition;
			color = Color.Red;
			color2 = Color.White;
			point = new ColoredCircleRenderable(color, 1f, 8, DrawMethod.TRIANGLEFAN);
			point.SetScale(0.5f);
		}

		Color decideColor(int add = 187)
		{
			var ran = Program.SharedRandom;
			return new Color(ran.Next(64) + add, ran.Next(64) + add, ran.Next(64) + add);
		}

		public override void Render()
		{
			ColorManager.LineWidth = 8f + (float)Math.Sin(World.Game.LocalTick / 4f) * 2f;
			ColorManager.DrawLine(originPos, Target, color);
			ColorManager.LineWidth = 4f + (float)Math.Sin(World.Game.LocalTick / 4f) * 2f;
			ColorManager.DrawLine(originPos, Target, color2); //TODO: maximal line size is 10.
			ColorManager.ResetLineWidth();

			point.SetPosition(Target);
			point.Render();

			point.SetPosition(originPos);
			point.Render();

			base.RenderPhysics();
		}

		public override void Tick()
		{
			if (Origin != null)
				originPos = Origin.ActiveWeapon.WeaponOffsetPosition;

			point.SetScale(0.5f + (float)Math.Sin(World.Game.LocalTick / 4f) / 16f);
			base.Tick();
		}

		public override void Dispose()
		{
			point.Dispose();
			base.Dispose();
		}
	}
}
