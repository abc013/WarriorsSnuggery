using System;

namespace WarriorsSnuggery.Graphics
{
	public static class ColorManager
	{
		public const float DefaultLineWidth = 2.0f;

		public static float LineWidth
		{
			get => lineWidth;
			set
			{
				lineWidth = value;
				MasterRenderer.SetLineWidth(lineWidth);
			}
		}
		static float lineWidth;

		static BatchObject line;
		static BatchObject circle;

		static BatchObject fullscreen_rect;
		static BatchObject filled_rect;
		static BatchObject gradient_rect;

		public static void ResetLineWidth()
		{
			LineWidth = DefaultLineWidth;
		}

		public static void Initialize()
		{
			line = new BatchObject(Mesh.Line(1f));
			circle = new BatchObject(Mesh.Circle(1f, 32));
			fullscreen_rect = new BatchObject(WindowInfo.UnitHeight);

			filled_rect = new BatchObject(1f);
			gradient_rect = new BatchObject(Mesh.GradientPlane(1f, Color.White, Color.White.WithAlpha(0f)));
		}

		public static void DrawLineQuad(in CPos pos, in CPos radius, in Color color)
		{
			DrawLine(pos - radius, pos + new CPos(-radius.X, radius.Y, 0), color);
			DrawLine(pos - radius, pos + new CPos(radius.X, -radius.Y, 0), color);
			DrawLine(pos - new CPos(-radius.X, radius.Y, 0), pos + radius, color);
			DrawLine(pos - new CPos(radius.X, -radius.Y, 0), pos + radius, color);
		}

		public static void DrawLine(in CPos start, in CPos end, in Color color)
		{
			var s = (start - end).FlatDist / 1024f;
			line.SetScale(new Vector(s, s, s));
			line.SetRotation(new VAngle(0, 0, -(start - end).FlatAngle) - new VAngle(0, 0, 90));
			line.SetPosition(start);
			line.SetColor(color);

			line.Render();
		}

		public static void DrawCircle(in CPos center, float radius, in Color color)
		{
			circle.SetScale(new Vector(radius * 2, radius * 2, radius * 2));
			circle.SetPosition(center);
			circle.SetColor(color);

			circle.Render();
		}

		public static void DrawFullscreenRect(in Color color)
		{
			fullscreen_rect.SetScale(WindowInfo.Ratio);
			fullscreen_rect.SetColor(color);
			fullscreen_rect.Render();
		}

		public static void DrawFilledLine(in CPos start, in CPos end, int width, in Color color)
		{
			var diff = start - end;
			filled_rect.SetScale(new CPos(width, (int)diff.FlatDist, 0).ToVector());
			filled_rect.SetPosition((start + end) / 2);
			filled_rect.SetRotation(new VAngle(0, 0, -diff.FlatAngle) - new VAngle(0, 0, 90));
			filled_rect.SetColor(color);
			filled_rect.Render();
		}

		public static void DrawGlowingFilledLineRect(in CPos pointA, in CPos pointB, int width, in Color color, int radius, int count)
		{
			var alpha = color.A / count;

			for (int i = 0; i < count; i++)
			{
				var currentRadius = radius / (i * i + 1);
				DrawFilledLineRect(pointA, pointB, width + currentRadius, color.WithAlpha(alpha));
			}
		}

		public static void DrawFilledLineRect(in CPos pointA, in CPos pointB, int width, in Color color)
		{
			var bottomLeft = new CPos(Math.Min(pointA.X, pointB.X), Math.Min(pointA.Y, pointB.Y), 0);
			var topRight = new CPos(Math.Max(pointA.X, pointB.X), Math.Max(pointA.Y, pointB.Y), 0);
			var bottomRight = new CPos(topRight.X, bottomLeft.Y, 0);
			var topLeft = new CPos(bottomLeft.X, topRight.Y, 0);

			DrawRect(topLeft + new CPos(width, -width, 0), topRight + new CPos(-width, width, 0), color);
			DrawRect(topLeft + new CPos(-width, width, 0), bottomLeft + new CPos(width, -width, 0), color);
			DrawRect(bottomLeft + new CPos(width, -width, 0), bottomRight + new CPos(-width, width, 0), color);
			DrawRect(bottomRight + new CPos(width, -width, 0), topRight + new CPos(-width, width, 0), color);
		}

		public static void DrawFilledLineQuad(in CPos center, int radius, int width, in Color color)
		{
			var topLeft = center - new CPos(radius, radius, 0);
			var bottomRight = center + new CPos(radius, radius, 0);

			DrawFilledLineRect(topLeft, bottomRight, width, color);
		}

		public static void DrawRect(in CPos pointA, in CPos pointB, in Color color)
		{
			filled_rect.SetScale(new CPos(Math.Abs(pointA.X - pointB.X), Math.Abs(pointA.Y - pointB.Y), 0).ToVector());
			filled_rect.SetPosition(new CPos((pointA.X + pointB.X) / 2, (pointA.Y + pointB.Y) / 2, 0));
			filled_rect.SetRotation(VAngle.Zero);
			filled_rect.SetColor(color);
			filled_rect.Render();
		}

		public static void DrawGradientRect(in CPos pointA, in CPos pointB, in Color color, int gradientDirection = 0)
		{
			gradient_rect.SetScale(new CPos(Math.Abs(pointA.X - pointB.X), Math.Abs(pointA.Y - pointB.Y), 0).ToVector());
			gradient_rect.SetPosition(new CPos((pointA.X + pointB.X) / 2, (pointA.Y + pointB.Y) / 2, 0));
			gradient_rect.SetRotation(new VAngle(0, 0, gradientDirection * Angle.MaxRange / 4));
			gradient_rect.SetColor(color);
			gradient_rect.Render();
		}

		public static void DrawDot(in CPos position, in Color color)
		{
			DrawQuad(position, 128, color);
		}

		public static void DrawQuad(in CPos center, int radius, in Color color)
		{
			filled_rect.SetScale(radius / 1024f);
			filled_rect.SetPosition(center);
			filled_rect.SetRotation(VAngle.Zero);
			filled_rect.SetColor(color);
			filled_rect.Render();
		}
	}
}
