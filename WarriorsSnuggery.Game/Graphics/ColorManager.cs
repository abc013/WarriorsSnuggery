﻿using System;

namespace WarriorsSnuggery.Graphics
{
	public static class ColorManager
	{
		public static float LineWidth
		{
			get
			{
				return lineWidth;
			}
			set
			{
				lineWidth = value;
				MasterRenderer.SetLineWidth(lineWidth);
			}
		}
		static float lineWidth;
		public const float DefaultLineWidth = 2.0f;

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
			line = new BatchObject(Mesh.Line(1f, Color.White), Color.White);
			circle = new BatchObject(Mesh.Circle(1f, Color.White, 32), Color.White);
			fullscreen_rect = new BatchObject(Mesh.Plane(WindowInfo.UnitHeight, Color.White), Color.White);
			WindowRescaled();

			filled_rect = new BatchObject(Mesh.Plane(1f, Color.White), Color.White);
		}

		public static void WindowRescaled()
		{
			fullscreen_rect?.SetScale(WindowInfo.Ratio);
		}

		public static void DrawLineRect(CPos pos, CPos radius, Color color)
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

			line.PushToBatchRenderer();
		}

		public static void DrawCircle(CPos center, float radius, Color color)
		{
			circle.SetScale(new Vector(radius * 2, radius * 2, radius * 2));
			circle.SetPosition(center);
			circle.SetColor(color);

			circle.PushToBatchRenderer();
		}

		public static void DrawFullscreenRect(Color color)
		{
			fullscreen_rect.SetColor(color);
			fullscreen_rect.PushToBatchRenderer();
		}

		public static void DrawRect(CPos bottomleft, CPos topright, Color color)
		{
			filled_rect.SetScale(new CPos(Math.Abs(bottomleft.X - topright.X), Math.Abs(bottomleft.Y - topright.Y), 0).ToVector());
			filled_rect.SetPosition(new CPos((bottomleft.X + topright.X) / 2, (bottomleft.Y + topright.Y) / 2, 0));
			filled_rect.SetColor(color);
			filled_rect.PushToBatchRenderer();
		}

		public static void DrawQuad(CPos position, int radius, Color color)
		{
			filled_rect.SetScale(radius / 1024f);
			filled_rect.SetPosition(position);
			filled_rect.SetColor(color);
			filled_rect.PushToBatchRenderer();
		}

		public static void DrawDot(CPos position, Color color)
		{
			DrawQuad(position, 128, color);
		}
	}
}
