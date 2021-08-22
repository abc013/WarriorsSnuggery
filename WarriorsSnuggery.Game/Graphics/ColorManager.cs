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

		public static void ResetLineWidth()
		{
			LineWidth = DefaultLineWidth;
		}

		public static void Initialize()
		{
			line = new BatchObject(Mesh.Line(1f));
			circle = new BatchObject(Mesh.Circle(1f, 32));
			fullscreen_rect = new BatchObject(Mesh.Plane(WindowInfo.UnitHeight));

			filled_rect = new BatchObject(Mesh.Plane(1f));
		}

		public static void DrawLineQuad(CPos pos, CPos radius, Color color)
		{
			DrawLine(pos - radius, pos + new CPos(-radius.X, radius.Y, 0), color);
			DrawLine(pos - radius, pos + new CPos(radius.X, -radius.Y, 0), color);
			DrawLine(pos - new CPos(-radius.X, radius.Y, 0), pos + radius, color);
			DrawLine(pos - new CPos(radius.X, -radius.Y, 0), pos + radius, color);
		}

		public static void DrawLine(CPos start, CPos end, Color color)
		{
			var s = (start - end).FlatDist / 1024f;
			line.SetScale(new Vector(s, s, s));
			line.SetRotation(new VAngle(0, 0, -(start - end).FlatAngle) - new VAngle(0, 0, 90));
			line.SetPosition(start);
			line.SetColor(color);

			line.Render();
		}

		public static void DrawCircle(CPos center, float radius, Color color)
		{
			circle.SetScale(new Vector(radius * 2, radius * 2, radius * 2));
			circle.SetPosition(center);
			circle.SetColor(color);

			circle.Render();
		}

		public static void DrawFullscreenRect(Color color)
		{
			fullscreen_rect.SetScale(WindowInfo.Ratio);
			fullscreen_rect.SetColor(color);
			fullscreen_rect.Render();
		}

		public static void DrawFilledLine(CPos start, CPos end, int width, Color color)
		{
			var diff = start - end;
			filled_rect.SetScale(new CPos(width, (int)diff.FlatDist, 0).ToVector());
			filled_rect.SetPosition((start + end) / new CPos(2, 2, 2));
			filled_rect.SetRotation(new VAngle(0, 0, -diff.FlatAngle) - new VAngle(0, 0, 90));
			filled_rect.SetColor(color);
			filled_rect.Render();
		}

		public static void DrawGlowingFilledLineRect(CPos pointA, CPos pointB, int width, Color color, int radius, int count)
		{
			var alpha = color.A / count;

			for (int i = 0; i < count; i++)
			{
				var currentRadius = radius / (i * i + 1);
				DrawFilledLineRect(pointA, pointB, width + currentRadius, new Color(color.R, color.G, color.B, alpha));
			}
		}

		public static void DrawFilledLineRect(CPos pointA, CPos pointB, int width, Color color)
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

		public static void DrawFilledLineQuad(CPos center, int radius, int width, Color color)
		{
			var topLeft = center - new CPos(radius, radius, 0);
			var bottomRight = center + new CPos(radius, radius, 0);

			DrawFilledLineRect(topLeft, bottomRight, width, color);
		}

		public static void DrawRect(CPos pointA, CPos pointB, Color color)
		{
			filled_rect.SetScale(new CPos(Math.Abs(pointA.X - pointB.X), Math.Abs(pointA.Y - pointB.Y), 0).ToVector());
			filled_rect.SetPosition(new CPos((pointA.X + pointB.X) / 2, (pointA.Y + pointB.Y) / 2, 0));
			filled_rect.SetRotation(VAngle.Zero);
			filled_rect.SetColor(color);
			filled_rect.Render();
		}

		public static void DrawDot(CPos position, Color color)
		{
			DrawQuad(position, 128, color);
		}

		public static void DrawQuad(CPos center, int radius, Color color)
		{
			filled_rect.SetScale(radius / 1024f);
			filled_rect.SetPosition(center);
			filled_rect.SetRotation(VAngle.Zero);
			filled_rect.SetColor(color);
			filled_rect.Render();
		}
	}
}
