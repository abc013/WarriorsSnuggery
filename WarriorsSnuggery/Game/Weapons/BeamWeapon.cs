using System;

namespace WarriorsSnuggery.Objects
{
	class BeamWeapon : Weapon
	{
		readonly ColoredLineRenderable beam;
		readonly ColoredLineRenderable beam2;
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
			color = decideColor();
			color2 = decideColor(64);
			var length = (float)originPos.GetDistToXY(Target) / 1024f;
			beam = new ColoredLineRenderable(color, 1f);
			beam.setScale(length);
			beam2 = new ColoredLineRenderable(color2, 1f);
			beam2.setScale(length);
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
			color = decideColor();
			color2 = decideColor(127);
			var length = (float)originPos.GetDistToXY(Target) / 1024f;
			beam = new ColoredLineRenderable(color, 1f);
			beam.setScale(length);
			beam2 = new ColoredLineRenderable(color2, 1f);
			beam2.setScale(length);
			point = new ColoredCircleRenderable(color, 1f, 8, Graphics.DrawMethod.TRIANGLEFAN);
		}

		Color decideColor(int add = 187)
		{
			var ran = Program.SharedRandom;
			return new Color(ran.Next(64) + add, ran.Next(64) + add, ran.Next(64) + add);
		}

		public override void Render()
		{
			var length = (float) originPos.GetDistToXY(Target) / 1024f;
			var rotation = new OpenTK.Vector4(0, 0, (-originPos.GetAngleToXY(Target) - 90) / 180f * (float)Math.PI, 0);

			point.setScale(0.5f + (float) Math.Sin(World.Game.LocalTick/4f) / 16f);
			point.setPosition(Target);
			point.Render();
			point.setPosition(originPos);
			point.Render();
			beam.setScale(length);
			beam.setPosition(Target);
			beam.setRotation(rotation);
			beam2.setScale(length);
			beam2.setPosition(Target);
			beam2.setRotation(rotation);
			OpenTK.Graphics.ES30.GL.LineWidth(15f);
			beam2.Render();
			OpenTK.Graphics.ES30.GL.LineWidth(5f);
			beam.Render();
			OpenTK.Graphics.ES30.GL.LineWidth(2.5f);
			Program.CheckGraphicsError("Code bug", true);
			base.RenderPhysics();
		}

		public override void Tick()
		{
			if (Origin != null)
				originPos = Origin.ActiveWeapon.WeaponOffsetPosition;

			if (Program.CheckGraphicsError("Code bug", false))
			{
				Log.WriteDebug("GL Bug has been detected. This Method is for counting only."); // TODO: error has something to do with the beam weapon. maybe its not disposed properly.
			}
			base.Tick();
		}

		public override void Dispose()
		{
			beam.Dispose();
			beam2.Dispose();
			point.Dispose();
			base.Dispose();
		}
	}
}
