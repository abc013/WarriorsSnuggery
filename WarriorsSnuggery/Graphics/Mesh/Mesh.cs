using OpenToolkit.Mathematics;
using System;

namespace WarriorsSnuggery.Graphics
{
	public static class Mesh
	{
		public static Vertex[] Image(Texture texture, Color color)
		{
			var x = texture.Offset.X / (float)Settings.SheetSize + Settings.SheetHalfPixel;
			var y = texture.Offset.Y / (float)Settings.SheetSize + Settings.SheetHalfPixel;
			var w = (texture.Offset.X + texture.Width) / (float)Settings.SheetSize - Settings.SheetHalfPixel;
			var h = (texture.Offset.Y + texture.Height) / (float)Settings.SheetSize - Settings.SheetHalfPixel;
			var scale = texture.Height / 48f;
			var correction = texture.Width / (float)texture.Height;
			var color4 = color.toColor4();
			var id = SpriteManager.SheetIndex(texture.SheetID);

			Vertex[] vertices =
			{
				new Vertex(new Vector(scale * correction,  scale,  0), new Vector4(w, y, id, 0), color4),
				new Vertex(new Vector(-scale * correction, -scale, 0), new Vector4(x, h, id, 0), color4),
				new Vertex(new Vector(scale * correction,  -scale, 0), new Vector4(w, h, id, 0), color4),
				new Vertex(new Vector(-scale * correction, scale,  0), new Vector4(x, y, id, 0), color4),
				new Vertex(new Vector(-scale * correction, -scale, 0), new Vector4(x, h, id, 0), color4),
				new Vertex(new Vector(scale * correction,  scale,  0), new Vector4(w, y, id, 0), color4),
			};

			return vertices;
		}

		public static Vertex[] Frame()
		{
			var color = Color.White.toColor4();
			var scale = 1;

			Vertex[] vertices =
			{
				new Vertex(new Vector(scale,  -scale,  0), new Vector4(1, 0, 0, 0), color),
				new Vertex(new Vector(-scale, scale, 0), new Vector4(0, 1, 0, 0), color),
				new Vertex(new Vector(scale,  scale, 0), new Vector4(1, 1, 0, 0), color),
				new Vertex(new Vector(-scale, -scale,  0), new Vector4(0, 0, 0, 0), color),
				new Vertex(new Vector(-scale, scale, 0), new Vector4(0, 1, 0, 0), color),
				new Vertex(new Vector(scale,  -scale,  0), new Vector4(1, 0, 0, 0), color),
			};

			return vertices;
		}

		public static Vertex[] UIPlane(Texture texture, Color color, Vector size)
		{
			var countX = (int)Math.Ceiling(size.X);
			var countY = (int)Math.Ceiling(size.Y);
			var vertices = new Vertex[countX * countY * 6];

			var index = 0;
			var sOffset = new Vector(-size.X, -size.Y, 0);
			for (int x = 0; x < countX; x++)
			{
				for (int y = 0; y < countY; y++)
				{
					var scaleX = Math.Min(1f, size.X - x);
					var scaleY = Math.Min(1f, size.Y - y);
					var lSize = new Vector(scaleX, scaleY, 0);
					var lOffset = sOffset + new Vector(x * 2, y * 2, 0);
					Array.Copy(uiPlanePart(texture, color, lOffset, lSize), 0, vertices, index, 6);
					index += 6;
				}
			}

			return vertices;
		}

		static Vertex[] uiPlanePart(Texture texture, Color color, Vector offset, Vector size)
		{
			var x = texture.Offset.X / (float)Settings.SheetSize + Settings.SheetHalfPixel;
			var y = texture.Offset.Y / (float)Settings.SheetSize + Settings.SheetHalfPixel;
			var w = (texture.Offset.X + size.X * texture.Width) / Settings.SheetSize - Settings.SheetHalfPixel;
			var h = (texture.Offset.Y + size.Y * texture.Height) / Settings.SheetSize - Settings.SheetHalfPixel;
			var color4 = color.toColor4();
			var id = SpriteManager.SheetIndex(texture.SheetID);

			Vertex[] vertices =
			{
				new Vertex(new Vector(offset.X + size.X * 2, offset.Y + size.Y * 2,  0), new Vector4(w, y, id, 0), color4),
				new Vertex(new Vector(offset.X, offset.Y, 0), new Vector4(x, h, id, 0), color4),
				new Vertex(new Vector(offset.X + size.X * 2, offset.Y, 0), new Vector4(w, h, id, 0), color4),
				new Vertex(new Vector(offset.X, offset.Y + size.Y * 2,  0), new Vector4(x, y, id, 0), color4),
				new Vertex(new Vector(offset.X, offset.Y, 0), new Vector4(x, h, id, 0), color4),
				new Vertex(new Vector(offset.X + size.X * 2, offset.Y + size.Y * 2, 0), new Vector4(w, y, id, 0), color4),
			};

			return vertices;
		}

		public static Vertex[] Line(float size, Color color)
		{
			var color4 = color.toColor4();

			Vertex[] vertices =
			{
				new Vertex(new Vector(0, 0, 0), new Vector4(-1), color4),
				new Vertex(new Vector(0, size, 0), new Vector4(-1), color4),
			};

			return vertices;
		}

		public static Vertex[] Plane(float scale, Color color)
		{
			scale /= 2;
			var color4 = color.toColor4();

			Vertex[] vertices =
			{
				new Vertex(new Vector(scale,  scale,  0), new Vector4(-1), color4),
				new Vertex(new Vector(-scale, -scale, 0), new Vector4(-1), color4),
				new Vertex(new Vector(scale,  -scale, 0), new Vector4(-1), color4),
				new Vertex(new Vector(-scale, scale,  0), new Vector4(-1), color4),
				new Vertex(new Vector(-scale, -scale, 0), new Vector4(-1), color4),
				new Vertex(new Vector(scale,  scale,  0), new Vector4(-1), color4),
			};

			return vertices;
		}

		public static Vertex[] Circle(float size, Color color, int resolution = 360)
		{
			size /= 2;
			var color4 = color.toColor4();

			var vertices = new Vertex[resolution * 2];
			for (int i = 0; i < resolution * 2; i++)
			{
				var x = ((float)Math.Cos(i * Math.PI / resolution * 2)) * size;
				var y = ((float)Math.Sin(i * Math.PI / resolution * 2)) * size;
				vertices[i] = new Vertex(new Vector(x, y, 0), new Vector4(-1), color4);
				if (i != 0 && i != resolution * 2 - 1)
					vertices[++i] = new Vertex(new Vector(x, y, 0), new Vector4(-1), color4);
			}
			return vertices;
		}

		public static Vertex[] Character(Font font, char c)
		{
			var texture = font.GetTexture(c);
			var x = texture.Offset.X / (float)Settings.SheetSize + Settings.SheetHalfPixel;
			var y = texture.Offset.Y / (float)Settings.SheetSize + Settings.SheetHalfPixel;
			var w = (texture.Offset.X + texture.Width) / (float)Settings.SheetSize - Settings.SheetHalfPixel;
			var h = (texture.Offset.Y + texture.Height) / (float)Settings.SheetSize - Settings.SheetHalfPixel;
			var correction = texture.Width / (float)texture.Height;
			var id = SpriteManager.SheetIndex(texture.SheetID);
			var scale = MasterRenderer.PixelMultiplier * font.Info.Size / 1.5f;

			Vertex[] vertices =
			{
				new Vertex(new Vector(scale * correction,  scale,  0), new Vector4(w, y, id, 0), Color.White),
				new Vertex(new Vector(-scale * correction, -scale, 0), new Vector4(x, h, id, 0), Color.White),
				new Vertex(new Vector(scale * correction,  -scale, 0), new Vector4(w, h, id, 0), Color.White),
				new Vertex(new Vector(-scale * correction, scale,  0), new Vector4(x, y, id, 0), Color.White),
				new Vertex(new Vector(-scale * correction, -scale, 0), new Vector4(x, h, id, 0), Color.White),
				new Vertex(new Vector(scale * correction,  scale,  0), new Vector4(w, y, id, 0), Color.White),
			};
			return vertices;
		}
	}
}
