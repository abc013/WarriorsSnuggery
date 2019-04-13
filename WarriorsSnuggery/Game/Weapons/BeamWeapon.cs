/*
 * User: Andreas
 * Date: 25.11.2017
 * 
 */
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
			if (Type.Inaccuracy > 0)
			{
				var ranX = Program.SharedRandom.Next(Type.Inaccuracy) - Type.Inaccuracy / 2;
				var ranY = Program.SharedRandom.Next(Type.Inaccuracy) - Type.Inaccuracy / 2;
				Target += new CPos(ranX, ranY, 0);
			}
			originPos = origin;
			color = Color.Red;
			color2 = Color.White;
			point = new ColoredCircleRenderable(color, 1f, 8, Graphics.DrawMethod.TRIANGLEFAN);
		}

		public BeamWeapon(World world, WeaponType type, Actor origin, CPos target) : base(world, type, origin, target)
		{
			if (Type.Inaccuracy > 0)
			{
				var ranX = Program.SharedRandom.Next(Type.Inaccuracy) - Type.Inaccuracy / 2;
				var ranY = Program.SharedRandom.Next(Type.Inaccuracy) - Type.Inaccuracy / 2;

				Target += new CPos(ranX, ranY, 0);
			}
			originPos = origin.ActiveWeapon.WeaponOffsetPosition;
			color = Color.Red;
			color2 = Color.White;
			point = new ColoredCircleRenderable(color, 1f, 8, Graphics.DrawMethod.TRIANGLEFAN);
		}

		Color decideColor(int add = 187)
		{
			var ran = Program.SharedRandom;
			return new Color(ran.Next(64) + add, ran.Next(64) + add, ran.Next(64) + add);
		}

		public override void Render()
		{
			ColorManager.LineWidth = 10f;
			ColorManager.DrawLine(originPos, Target, color);
			ColorManager.LineWidth = 5f;
			ColorManager.DrawLine(originPos, Target, color2);
			ColorManager.ResetLineWidth();

			point.SetPosition(Target);
			point.Render();

			point.SetPosition(originPos);
			point.Render();

			Program.CheckGraphicsError("Code bug", true);
			base.RenderPhysics();
		}

		public override void Tick()
		{
			if (Origin != null)
				originPos = Origin.ActiveWeapon.WeaponOffsetPosition;

			point.SetScale(0.5f + (float)Math.Sin(World.Game.LocalTick / 4f) / 16f);

			if (Program.CheckGraphicsError("Code bug", false))
			{
				Log.WriteDebug("GL Bug has been detected. This Method is for counting only."); // TODO: error has something to do with the beam weapon. maybe its not disposed properly.
			}
			base.Tick();
		}

		public override void Dispose()
		{
			point.Dispose();
			base.Dispose();
		}
	}
}
