using OpenTK;
using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Graphics
{
	public static class TexturedMesh
	{
		// Text character
		public static TexturedVertex[] Character(IFont font)
		{
			var w = font.MaxSize.X;
			var h = font.MaxSize.Y;
			var size = w * TextRenderable.SizeMultiplier;

			TexturedVertex[] vertices =
			{
				new TexturedVertex(new Vector(-size, -size, 0, 1.0f), new Vector2(0, h)),
				new TexturedVertex(new Vector(size,  -size, 0, 1.0f), new Vector2(w, h)),
				new TexturedVertex(new Vector(-size, size,  0, 1.0f), new Vector2(0, 0)),
				new TexturedVertex(new Vector(-size, size,  0, 1.0f), new Vector2(0, 0)),
				new TexturedVertex(new Vector(size,  -size, 0, 1.0f), new Vector2(w, h)),
				new TexturedVertex(new Vector(size,  size,  0, 1.0f), new Vector2(w, 0)),
			};
			return vertices;
		}

		// Terrain
		const float halfTerrain = MasterRenderer.PixelMultiplier * 24 / 2 + 0.001f;

		static readonly TexturedVertex[] terrain =
		{
				new TexturedVertex(new Vector(halfTerrain,  halfTerrain,  0, 1.0f), new Vector2(1, 0)),
				new TexturedVertex(new Vector(-halfTerrain, -halfTerrain, 0, 1.0f), new Vector2(0, 1)),
				new TexturedVertex(new Vector(halfTerrain,  -halfTerrain, 0, 1.0f), new Vector2(1, 1)),
				new TexturedVertex(new Vector(-halfTerrain, halfTerrain,  0, 1.0f), new Vector2(0, 0)),
				new TexturedVertex(new Vector(-halfTerrain, -halfTerrain, 0, 1.0f), new Vector2(0, 1)),
				new TexturedVertex(new Vector(halfTerrain,  halfTerrain,  0, 1.0f), new Vector2(1, 0)),
		};

		public static TexturedVertex[] Terrain()
		{
			return terrain;
		}

		// Frame, used for pixelated rendering
		public static TexturedVertex[] Terrain(int xp, int yp, int width, int height)
		{
			var x = xp / (float)Settings.SheetSize;
			var y = yp / (float)Settings.SheetSize;
			var w = (xp + width) / (float)Settings.SheetSize;
			var h = (yp + height) / (float)Settings.SheetSize;
			var scale = 1/2f;

			TexturedVertex[] vertices =
			{
				new TexturedVertex(new Vector(scale,  -scale,  0, 1.0f), new Vector2(w, y)),
				new TexturedVertex(new Vector(-scale, scale, 0, 1.0f), new Vector2(x, h)),
				new TexturedVertex(new Vector(scale,  scale, 0, 1.0f), new Vector2(w, h)),
				new TexturedVertex(new Vector(-scale, -scale,  0, 1.0f), new Vector2(x, y)),
				new TexturedVertex(new Vector(-scale, scale, 0, 1.0f), new Vector2(x, h)),
				new TexturedVertex(new Vector(scale,  -scale,  0, 1.0f), new Vector2(w, y)),
			};

			return vertices;
		}

		// Frame, used for pixelated rendering
		public static TexturedVertex[] Frame()
		{
			var scale = 1;

			TexturedVertex[] vertices =
			{
				new TexturedVertex(new Vector(scale,  -scale,  0, 1.0f), new Vector2(1, 0)),
				new TexturedVertex(new Vector(-scale, scale, 0, 1.0f), new Vector2(0, 1)),
				new TexturedVertex(new Vector(scale,  scale, 0, 1.0f), new Vector2(1, 1)),
				new TexturedVertex(new Vector(-scale, -scale,  0, 1.0f), new Vector2(0, 0)),
				new TexturedVertex(new Vector(-scale, scale, 0, 1.0f), new Vector2(0, 1)),
				new TexturedVertex(new Vector(scale,  -scale,  0, 1.0f), new Vector2(1, 0)),
			};

			return vertices;
		}

		// Simple plane
		public static TexturedVertex[] Plane(float scale, float x, float y, float w, float h)
		{
			scale /= 2;
			var correction = w < h ? w / h : h / w;

			TexturedVertex[] vertices;
			if (w < h)
			{
				TexturedVertex[] vertices1 =
				{
					new TexturedVertex(new Vector(scale * correction, scale,   0, 1.0f), new Vector2(w, y)),
					new TexturedVertex(new Vector(-scale * correction, -scale, 0, 1.0f), new Vector2(x, h)),
					new TexturedVertex(new Vector(scale * correction, -scale,  0, 1.0f), new Vector2(w, h)),
					new TexturedVertex(new Vector(-scale * correction, scale,  0, 1.0f), new Vector2(x, y)),
					new TexturedVertex(new Vector(-scale * correction, -scale, 0, 1.0f), new Vector2(x, h)),
					new TexturedVertex(new Vector(scale * correction, scale,   0, 1.0f), new Vector2(w, y)),
				};
				vertices = vertices1;
			}
			else
			{
				TexturedVertex[] vertices1 =
				{
					new TexturedVertex(new Vector(scale, scale * correction,   0, 1.0f), new Vector2(w, y)),
					new TexturedVertex(new Vector(-scale, -scale * correction, 0, 1.0f), new Vector2(x, h)),
					new TexturedVertex(new Vector(scale, -scale * correction,  0, 1.0f), new Vector2(w, h)),
					new TexturedVertex(new Vector(-scale, scale * correction,  0, 1.0f), new Vector2(x, y)),
					new TexturedVertex(new Vector(-scale, -scale * correction, 0, 1.0f), new Vector2(x, h)),
					new TexturedVertex(new Vector(scale, scale * correction,   0, 1.0f), new Vector2(w, y)),
				};
				vertices = vertices1;
			}

			return vertices;
		}

		public static TexturedVertex[] Plane(float scale, float w, float h)
		{
			scale /= 2;
			var correction = w < h ? w / h : h / w;

			TexturedVertex[] vertices;
			if (w < h)
			{
				TexturedVertex[] vertices1 =
				{
					new TexturedVertex(new Vector(scale * correction, scale,   0, 1.0f), new Vector2(1, 0)),
					new TexturedVertex(new Vector(-scale * correction, -scale, 0, 1.0f), new Vector2(0, 1)),
					new TexturedVertex(new Vector(scale * correction, -scale,  0, 1.0f), new Vector2(1, 1)),
					new TexturedVertex(new Vector(-scale * correction, scale,  0, 1.0f), new Vector2(0, 0)),
					new TexturedVertex(new Vector(-scale * correction, -scale, 0, 1.0f), new Vector2(0, 1)),
					new TexturedVertex(new Vector(scale * correction, scale,   0, 1.0f), new Vector2(1, 0)),
				};
				vertices = vertices1;
			}
			else
			{
				TexturedVertex[] vertices1 =
				{
					new TexturedVertex(new Vector(scale, scale * correction,   0, 1.0f), new Vector2(1, 0)),
					new TexturedVertex(new Vector(-scale, -scale * correction, 0, 1.0f), new Vector2(0, 1)),
					new TexturedVertex(new Vector(scale, -scale * correction,  0, 1.0f), new Vector2(1, 1)),
					new TexturedVertex(new Vector(-scale, scale * correction,  0, 1.0f), new Vector2(0, 0)),
					new TexturedVertex(new Vector(-scale, -scale * correction, 0, 1.0f), new Vector2(0, 1)),
					new TexturedVertex(new Vector(scale, scale * correction,   0, 1.0f), new Vector2(1, 0)),
				};
				vertices = vertices1;
			}

			return vertices;
		}

		public static TexturedVertex[] PixelOrientedPlane(float scale, int width, int height)
		{
			scale /= 2;
			var sizeX = width * MasterRenderer.PixelMultiplier;
			var sizeY = height * MasterRenderer.PixelMultiplier;

			TexturedVertex[] vertices =
			{
				new TexturedVertex(new Vector(sizeX * scale,  sizeY * scale,  0, 1.0f), new Vector2(sizeX, 0)),
				new TexturedVertex(new Vector(-sizeX * scale, -sizeY * scale, 0, 1.0f), new Vector2(0, sizeY)),
				new TexturedVertex(new Vector(sizeX * scale,  -sizeY * scale, 0, 1.0f), new Vector2(sizeX, sizeY)),
				new TexturedVertex(new Vector(-sizeX * scale, sizeY * scale,  0, 1.0f), new Vector2(0, 0)),
				new TexturedVertex(new Vector(-sizeX * scale, -sizeY * scale, 0, 1.0f), new Vector2(0, sizeY)),
				new TexturedVertex(new Vector(sizeX * scale,  sizeY * scale,  0, 1.0f), new Vector2(sizeX, 0)),
			};

			return vertices;
		}
	}

	public static class ColoredMesh
	{
		public static ColoredVertex[] Line(float size, Color color)
		{
			var color4 = color.toColor4();

			ColoredVertex[] vertices =
			{
				new ColoredVertex(new Vector(0, 0, 0, 1.0f), color4),
				new ColoredVertex(new Vector(0, size, 0, 1.0f), color4),
			};

			return vertices;
		}

		public static ColoredVertex[] PlaneEdges(float size, Color color)
		{
			return PlaneEdges(size, size, color);
		}

		public static ColoredVertex[] PlaneEdges(float x, float y, Color color)
		{
			x /= 2;
			y /= 2;
			var color4 = color.toColor4();

			ColoredVertex[] vertices =
			{
				new ColoredVertex(new Vector(x,  y,  0, 1.0f), color4),
				new ColoredVertex(new Vector(-x, y,  0, 1.0f), color4),
				new ColoredVertex(new Vector(-x,  y, 0, 1.0f), color4),
				new ColoredVertex(new Vector(-x, -y, 0, 1.0f), color4),
				new ColoredVertex(new Vector(-x, -y, 0, 1.0f), color4),
				new ColoredVertex(new Vector(x,  -y, 0, 1.0f), color4),
				new ColoredVertex(new Vector(x,  -y, 0, 1.0f), color4),
				new ColoredVertex(new Vector(x,  y,  0, 1.0f), color4),
			};

			return vertices;
		}

		public static ColoredVertex[] Plane(float size, Color color)
		{
			return Plane(size, size, color);
		}

		public static ColoredVertex[] Plane(float x, float y, Color color)
		{
			x /= 2;
			y /= 2;
			var color4 = color.toColor4();

			ColoredVertex[] vertices =
			{
				new ColoredVertex(new Vector(x,  y,  0, 1.0f), color4),
				new ColoredVertex(new Vector(-x, -y, 0, 1.0f), color4),
				new ColoredVertex(new Vector(x,  -y, 0, 1.0f), color4),
				new ColoredVertex(new Vector(-x, y,  0, 1.0f), color4),
				new ColoredVertex(new Vector(-x, -y, 0, 1.0f), color4),
				new ColoredVertex(new Vector(x,  y,  0, 1.0f), color4),
			};

			return vertices;
		}

		public static ColoredVertex[] Circle(float size, Color color, int resolution = 360)
		{
			size /= 2;
			var color4 = color.toColor4();

			var vertices = new ColoredVertex[resolution];
			for (int i = 0; i < resolution; i++)
			{
				var x = ((float)Math.Cos(i * Math.PI / resolution * 2)) * size;
				var y = ((float)Math.Sin(i * Math.PI / resolution * 2)) * size;
				vertices[i] = new ColoredVertex(new Vector(x, y, 0f, 1f), color4);
			}
			return vertices;
		}
	}
}
