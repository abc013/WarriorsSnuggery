using OpenTK;
using System;

namespace WarriorsSnuggery.Graphics
{
	public static class Mesh
	{
		public static Vertex[] Terrain(ITexture texture, Color color)
		{
			var x = texture.Offset.X / (float)Settings.SheetSize;
			var y = texture.Offset.Y / (float)Settings.SheetSize;
			var w = (texture.Offset.X + texture.Width) / (float)Settings.SheetSize;
			var h = (texture.Offset.Y + texture.Height) / (float)Settings.SheetSize;
			var scale = 1 / 2f + 0.001f;
			var color4 = color.toColor4();
			var id = SpriteManager.SheetIndex(texture.SheetID);

			Vertex[] vertices =
			{
				new Vertex(new Vector(scale,  scale,  0, 1.0f), new Vector4(w, y, id, 0), color4),
				new Vertex(new Vector(-scale, -scale, 0, 1.0f), new Vector4(x, h, id, 0), color4),
				new Vertex(new Vector(scale,  -scale, 0, 1.0f), new Vector4(w, h, id, 0), color4),
				new Vertex(new Vector(-scale, scale,  0, 1.0f), new Vector4(x, y, id, 0), color4),
				new Vertex(new Vector(-scale, -scale, 0, 1.0f), new Vector4(x, h, id, 0), color4),
				new Vertex(new Vector(scale,  scale,  0, 1.0f), new Vector4(w, y, id, 0), color4),
			};

			return vertices;
		}

		public static Vertex[] Image(ITexture texture, Color color)
		{
			var x = texture.Offset.X / (float)Settings.SheetSize;
			var y = texture.Offset.Y / (float)Settings.SheetSize;
			var w = (texture.Offset.X + texture.Width) / (float)Settings.SheetSize;
			var h = (texture.Offset.Y + texture.Height) / (float)Settings.SheetSize;
			var scale = texture.Height / 48f;
			var correction = texture.Width / (float)texture.Height;
			var color4 = color.toColor4();
			var id = SpriteManager.SheetIndex(texture.SheetID);

			Vertex[] vertices =
			{
				new Vertex(new Vector(scale * correction,  scale,  0, 1.0f), new Vector4(w, y, id, 0), color4),
				new Vertex(new Vector(-scale * correction, -scale, 0, 1.0f), new Vector4(x, h, id, 0), color4),
				new Vertex(new Vector(scale * correction,  -scale, 0, 1.0f), new Vector4(w, h, id, 0), color4),
				new Vertex(new Vector(-scale * correction, scale,  0, 1.0f), new Vector4(x, y, id, 0), color4),
				new Vertex(new Vector(-scale * correction, -scale, 0, 1.0f), new Vector4(x, h, id, 0), color4),
				new Vertex(new Vector(scale * correction,  scale,  0, 1.0f), new Vector4(w, y, id, 0), color4),
			};

			return vertices;
		}

		// Frame, used for pixelated rendering
		public static Vertex[] Frame()
		{
			var color = Color.White.toColor4();
			var scale = 1;

			Vertex[] vertices =
			{
				new Vertex(new Vector(scale,  scale,  0, 1.0f), new Vector4(1, 0, 0, 0), color),
				new Vertex(new Vector(-scale, -scale, 0, 1.0f), new Vector4(0, 1, 0, 0), color),
				new Vertex(new Vector(scale,  -scale, 0, 1.0f), new Vector4(1, 1, 0, 0), color),
				new Vertex(new Vector(-scale, scale,  0, 1.0f), new Vector4(0, 0, 0, 0), color),
				new Vertex(new Vector(-scale, -scale, 0, 1.0f), new Vector4(0, 1, 0, 0), color),
				new Vertex(new Vector(scale,  scale,  0, 1.0f), new Vector4(1, 0, 0, 0), color),
			};

			return vertices;
		}

		public static Vertex[] Plane(float scale, float x, float y, float w, float h, Color color)
		{
			scale /= 2;
			var correction = w < h ? w / h : h / w;
			var color4 = color.toColor4();

			Vertex[] vertices;
			if (w < h)
			{
				Vertex[] vertices1 =
				{
					new Vertex(new Vector(scale * correction,  scale,  0, 1.0f), new Vector4(w, y, 0, 0), color4),
					new Vertex(new Vector(-scale * correction, -scale, 0, 1.0f), new Vector4(x, h, 0, 0), color4),
					new Vertex(new Vector(scale * correction,  -scale, 0, 1.0f), new Vector4(w, h, 0, 0), color4),
					new Vertex(new Vector(-scale * correction, scale,  0, 1.0f), new Vector4(x, y, 0, 0), color4),
					new Vertex(new Vector(-scale * correction, -scale, 0, 1.0f), new Vector4(x, h, 0, 0), color4),
					new Vertex(new Vector(scale * correction,  scale,  0, 1.0f), new Vector4(w, y, 0, 0), color4),
				};
				vertices = vertices1;
			}
			else
			{
				Vertex[] vertices1 =
				{
					new Vertex(new Vector(scale,  scale * correction,  0, 1.0f), new Vector4(w, y, 0, 0), color4),
					new Vertex(new Vector(-scale, -scale * correction, 0, 1.0f), new Vector4(x, h, 0, 0), color4),
					new Vertex(new Vector(scale,  -scale * correction, 0, 1.0f), new Vector4(w, h, 0, 0), color4),
					new Vertex(new Vector(-scale, scale * correction,  0, 1.0f), new Vector4(x, y, 0, 0), color4),
					new Vertex(new Vector(-scale, -scale * correction, 0, 1.0f), new Vector4(x, h, 0, 0), color4),
					new Vertex(new Vector(scale,  scale * correction,  0, 1.0f), new Vector4(w, y, 0, 0), color4),
				};
				vertices = vertices1;
			}

			return vertices;
		}

		public static Vertex[] Plane(float scale, float w, float h, Color color)
		{
			scale /= 2;
			var correction = w < h ? w / h : h / w;
			var color4 = color.toColor4();

			Vertex[] vertices;
			if (w < h)
			{
				Vertex[] vertices1 =
				{
					new Vertex(new Vector(scale * correction,  scale,  0, 1.0f), new Vector4(1, 0, 0, 0), color4),
					new Vertex(new Vector(-scale * correction, -scale, 0, 1.0f), new Vector4(0, 1, 0, 0), color4),
					new Vertex(new Vector(scale * correction,  -scale, 0, 1.0f), new Vector4(1, 1, 0, 0), color4),
					new Vertex(new Vector(-scale * correction, scale,  0, 1.0f), new Vector4(0, 0, 0, 0), color4),
					new Vertex(new Vector(-scale * correction, -scale, 0, 1.0f), new Vector4(0, 1, 0, 0), color4),
					new Vertex(new Vector(scale * correction,  scale,  0, 1.0f), new Vector4(1, 0, 0, 0), color4),
				};
				vertices = vertices1;
			}
			else
			{
				Vertex[] vertices1 =
				{
					new Vertex(new Vector(scale,  scale * correction,  0, 1.0f), new Vector4(1, 0, 0, 0), color4),
					new Vertex(new Vector(-scale, -scale * correction, 0, 1.0f), new Vector4(0, 1, 0, 0), color4),
					new Vertex(new Vector(scale,  -scale * correction, 0, 1.0f), new Vector4(1, 1, 0, 0), color4),
					new Vertex(new Vector(-scale, scale * correction,  0, 1.0f), new Vector4(0, 0, 0, 0), color4),
					new Vertex(new Vector(-scale, -scale * correction, 0, 1.0f), new Vector4(0, 1, 0, 0), color4),
					new Vertex(new Vector(scale,  scale * correction,  0, 1.0f), new Vector4(1, 0, 0, 0), color4),
				};
				vertices = vertices1;
			}

			return vertices;
		}

		public static Vertex[] PixelOrientedPlane(float scale, int width, int height, Color color)
		{
			scale /= 2;
			var sX = width * MasterRenderer.PixelMultiplier;
			var sY = height * MasterRenderer.PixelMultiplier;
			var color4 = color.toColor4();

			Vertex[] vertices =
			{
				new Vertex(new Vector(sX * scale,  sY * scale,  0, 1.0f), new Vector4(sX,  0, 0, 0), color4),
				new Vertex(new Vector(-sX * scale, -sY * scale, 0, 1.0f), new Vector4( 0, sY, 0, 0), color4),
				new Vertex(new Vector(sX * scale,  -sY * scale, 0, 1.0f), new Vector4(sX, sY, 0, 0), color4),
				new Vertex(new Vector(-sX * scale, sY * scale,  0, 1.0f), new Vector4( 0,  0, 0, 0), color4),
				new Vertex(new Vector(-sX * scale, -sY * scale, 0, 1.0f), new Vector4( 0, sY, 0, 0), color4),
				new Vertex(new Vector(sX * scale,  sY * scale,  0, 1.0f), new Vector4(sX,  0, 0, 0), color4),
			};

			return vertices;
		}
		public static Vertex[] Line(float size, Color color)
		{
			var color4 = color.toColor4();

			Vertex[] vertices =
			{
				new Vertex(new Vector(0, 0, 0, 1.0f), new Vector4(-1), color4),
				new Vertex(new Vector(0, size, 0, 1.0f), new Vector4(-1), color4),
			};

			return vertices;
		}

		public static Vertex[] PlaneEdges(float size, Color color)
		{
			return PlaneEdges(size, size, color);
		}

		public static Vertex[] PlaneEdges(float x, float y, Color color)
		{
			x /= 2;
			y /= 2;
			var color4 = color.toColor4();

			Vertex[] vertices =
			{
				new Vertex(new Vector(x,  y,  0, 1.0f), new Vector4(-1), color4),
				new Vertex(new Vector(-x, y,  0, 1.0f), new Vector4(-1), color4),
				new Vertex(new Vector(-x,  y, 0, 1.0f), new Vector4(-1), color4),
				new Vertex(new Vector(-x, -y, 0, 1.0f), new Vector4(-1), color4),
				new Vertex(new Vector(-x, -y, 0, 1.0f), new Vector4(-1), color4),
				new Vertex(new Vector(x,  -y, 0, 1.0f), new Vector4(-1), color4),
				new Vertex(new Vector(x,  -y, 0, 1.0f), new Vector4(-1), color4),
				new Vertex(new Vector(x,  y,  0, 1.0f), new Vector4(-1), color4),
			};

			return vertices;
		}

		public static Vertex[] Plane(float size, Color color)
		{
			return Plane(size, size, color);
		}

		public static Vertex[] Plane(float x, float y, Color color)
		{
			x /= 2;
			y /= 2;
			var color4 = color.toColor4();

			Vertex[] vertices =
			{
				new Vertex(new Vector(x,  y,  0, 1.0f), new Vector4(-1), color4),
				new Vertex(new Vector(-x, -y, 0, 1.0f), new Vector4(-1), color4),
				new Vertex(new Vector(x,  -y, 0, 1.0f), new Vector4(-1), color4),
				new Vertex(new Vector(-x, y,  0, 1.0f), new Vector4(-1), color4),
				new Vertex(new Vector(-x, -y, 0, 1.0f), new Vector4(-1), color4),
				new Vertex(new Vector(x,  y,  0, 1.0f), new Vector4(-1), color4),
			};

			return vertices;
		}

		public static Vertex[] Circle(float size, Color color, int resolution = 360)
		{
			size /= 2;
			var color4 = color.toColor4();

			var vertices = new Vertex[resolution];
			for (int i = 0; i < resolution; i++)
			{
				var x = ((float)Math.Cos(i * Math.PI / resolution * 2)) * size;
				var y = ((float)Math.Sin(i * Math.PI / resolution * 2)) * size;
				vertices[i] = new Vertex(new Vector(x, y, 0f, 1f), new Vector4(-1), color4);
			}
			return vertices;
		}
	}

	public static class CharMesh
	{
		public static CharVertex[] Character(IFont font)
		{
			var w = font.MaxSize.X;
			var h = font.MaxSize.Y;
			var size = w * IFont.FontSizeMultiplier;

			CharVertex[] vertices =
			{
				new CharVertex(new Vector(-size, -size, 0, 1.0f), new Vector2(0, h)),
				new CharVertex(new Vector(size,  -size, 0, 1.0f), new Vector2(w, h)),
				new CharVertex(new Vector(-size, size,  0, 1.0f), new Vector2(0, 0)),
				new CharVertex(new Vector(-size, size,  0, 1.0f), new Vector2(0, 0)),
				new CharVertex(new Vector(size,  -size, 0, 1.0f), new Vector2(w, h)),
				new CharVertex(new Vector(size,  size,  0, 1.0f), new Vector2(w, 0)),
			};
			return vertices;
		}
	}
}
