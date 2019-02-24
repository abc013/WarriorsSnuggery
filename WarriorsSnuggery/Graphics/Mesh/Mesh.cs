/*
 * User: Andreas
 * Date: 10.08.2017
 */
using System;
using OpenTK;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public static class TexturedMesh
	{
		public static TexturedVertex[] Character(IFont font)
        {
            var w = font.MaxSize.X;
            var h = font.MaxSize.Y;
            var size = w * TextRenderable.SizeMultiplier;

            TexturedVertex[] vertices =
            {
                new TexturedVertex(new Vector4(-size, -size, 0, 1.0f), new Vector2(0, h)),
                new TexturedVertex(new Vector4(size,  -size, 0, 1.0f), new Vector2(w, h)),
                new TexturedVertex(new Vector4(-size, size,  0, 1.0f), new Vector2(0, 0)),
                new TexturedVertex(new Vector4(-size, size,  0, 1.0f), new Vector2(0, 0)),
                new TexturedVertex(new Vector4(size,  -size, 0, 1.0f), new Vector2(w, h)),
                new TexturedVertex(new Vector4(size,  size,  0, 1.0f), new Vector2(w, 0)),
            };
            return vertices;
		}

		const float halfTerrain = GLPos.PixelMultiplier * 24 / 2 + 0.001f;

		static readonly TexturedVertex[] terrain =
		{
				new TexturedVertex(new Vector4(halfTerrain,  halfTerrain,  0, 1.0f), new Vector2(1, 0)),
				new TexturedVertex(new Vector4(-halfTerrain, -halfTerrain, 0, 1.0f), new Vector2(0, 1)),
				new TexturedVertex(new Vector4(halfTerrain,  -halfTerrain, 0, 1.0f), new Vector2(1, 1)),
				new TexturedVertex(new Vector4(-halfTerrain, halfTerrain,  0, 1.0f), new Vector2(0, 0)),
				new TexturedVertex(new Vector4(-halfTerrain, -halfTerrain, 0, 1.0f), new Vector2(0, 1)),
				new TexturedVertex(new Vector4(halfTerrain,  halfTerrain,  0, 1.0f), new Vector2(1, 0)),
		};

		public static TexturedVertex[] Terrain()
		{
			return terrain;
		}

		public static TexturedVertex[] Frame()
		{
			var scale = 1;

			TexturedVertex[] vertices =
			{
				new TexturedVertex(new Vector4(scale,  -scale,  0, 1.0f), new Vector2(1, 0)),
				new TexturedVertex(new Vector4(-scale, scale, 0, 1.0f), new Vector2(0, 1)),
				new TexturedVertex(new Vector4(scale,  scale, 0, 1.0f), new Vector2(1, 1)),
				new TexturedVertex(new Vector4(-scale, -scale,  0, 1.0f), new Vector2(0, 0)),
				new TexturedVertex(new Vector4(-scale, scale, 0, 1.0f), new Vector2(0, 1)),
				new TexturedVertex(new Vector4(scale,  -scale,  0, 1.0f), new Vector2(1, 0)),
			};

			return vertices;
		}

		public static TexturedVertex[] Plane(float scale, float w, float h)
		{
			scale /= 2;
			var correction = w/h;

			TexturedVertex[] vertices =
			{
				new TexturedVertex(new Vector4(scale * correction,  scale,  0, 1.0f), new Vector2(1, 0)),
				new TexturedVertex(new Vector4(-scale * correction, -scale, 0, 1.0f), new Vector2(0, 1)),
				new TexturedVertex(new Vector4(scale * correction,  -scale, 0, 1.0f), new Vector2(1, 1)),
				new TexturedVertex(new Vector4(-scale * correction, scale,  0, 1.0f), new Vector2(0, 0)),
				new TexturedVertex(new Vector4(-scale * correction, -scale, 0, 1.0f), new Vector2(0, 1)),
				new TexturedVertex(new Vector4(scale * correction,  scale,  0, 1.0f), new Vector2(1, 0)),
			};
			
			return vertices;
		}

		public static TexturedVertex[] PixelOrientedPlane(float scale, int width, int height)
		{
			scale /= 2;
			var sizeX = width * GLPos.PixelMultiplier;
			var sizeY = height * GLPos.PixelMultiplier;

			TexturedVertex[] vertices =
			{
				new TexturedVertex(new Vector4(sizeX * scale,  sizeY * scale,  0, 1.0f), new Vector2(sizeX, 0)),
				new TexturedVertex(new Vector4(-sizeX * scale, -sizeY * scale, 0, 1.0f), new Vector2(0, sizeY)),
				new TexturedVertex(new Vector4(sizeX * scale,  -sizeY * scale, 0, 1.0f), new Vector2(sizeX, sizeY)),
				new TexturedVertex(new Vector4(-sizeX * scale, sizeY * scale,  0, 1.0f), new Vector2(0, 0)),
				new TexturedVertex(new Vector4(-sizeX * scale, -sizeY * scale, 0, 1.0f), new Vector2(0, sizeY)),
				new TexturedVertex(new Vector4(sizeX * scale,  sizeY * scale,  0, 1.0f), new Vector2(sizeX, 0)),
			};

			return vertices;
		}

		public static TexturedVertex[] Circle(float size, float w, float h, int resolution = 180)
		{
			size /= 2;

			var vertices = new TexturedVertex[resolution];
			for (int i = 0; i < resolution; i++)
			{
				var x = ((float) Math.Cos(i * Math.PI / resolution * 2)) * size;
				var y = ((float) Math.Sin(i * Math.PI / resolution * 2)) * size;
				vertices[i] = new TexturedVertex(new Vector4(x, y, 0f, 1f), new Vector2((int) x,(int) y));
			}
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
				new ColoredVertex(new Vector4(0,0,0,1.0f), color4),
				new ColoredVertex(new Vector4(0,size,0,1.0f), color4),
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
				new ColoredVertex(new Vector4(x,  y,  0, 1.0f), color4),
				new ColoredVertex(new Vector4(-x, y, 0, 1.0f), color4),
				new ColoredVertex(new Vector4(-x,  y, 0, 1.0f), color4),
				new ColoredVertex(new Vector4(-x, -y,  0, 1.0f), color4),
				new ColoredVertex(new Vector4(-x, -y, 0, 1.0f), color4),
				new ColoredVertex(new Vector4(x,  -y,  0, 1.0f), color4),
				new ColoredVertex(new Vector4(x,  -y,  0, 1.0f), color4),
				new ColoredVertex(new Vector4(x,  y,  0, 1.0f), color4),
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
				new ColoredVertex(new Vector4(x,  y,  0, 1.0f), color4),
				new ColoredVertex(new Vector4(-x, -y, 0, 1.0f), color4),
				new ColoredVertex(new Vector4(x,  -y, 0, 1.0f), color4),
				new ColoredVertex(new Vector4(-x, y,  0, 1.0f), color4),
				new ColoredVertex(new Vector4(-x, -y, 0, 1.0f), color4),
				new ColoredVertex(new Vector4(x,  y,  0, 1.0f), color4),
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
				var x = ((float) Math.Cos(i * Math.PI / resolution * 2)) * size;
				var y = ((float) Math.Sin(i * Math.PI / resolution * 2)) * size;
				vertices[i] = new ColoredVertex(new Vector4(x, y, 0f, 1f), color4);
			}
			return vertices;
		}

		public static ColoredVertex[] Cube(float size, Color color)
		{
			size /= 2;
			var color4 = color.toColor4();

			ColoredVertex[] vertices =
			{
				new ColoredVertex(new Vector4(-size, -size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(-size, -size, size, 1.0f), color4),
				new ColoredVertex(new Vector4(-size, size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(-size, size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(-size, -size, size, 1.0f), color4),
				new ColoredVertex(new Vector4(-size, size, size, 1.0f), color4),

				new ColoredVertex(new Vector4(size, -size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, -size, size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, -size, size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, size, size, 1.0f), color4),

				new ColoredVertex(new Vector4(-size, -size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, -size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(-size, -size, size, 1.0f), color4),
				new ColoredVertex(new Vector4(-size, -size, size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, -size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, -size, size, 1.0f), color4),

				new ColoredVertex(new Vector4(-size, size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(-size, size, size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(-size, size, size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, size, size, 1.0f), color4),

				new ColoredVertex(new Vector4(-size, -size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(-size, size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, -size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, -size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(-size, size, -size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, size, -size, 1.0f), color4),

				new ColoredVertex(new Vector4(-size, -size, size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, -size, size, 1.0f), color4),
				new ColoredVertex(new Vector4(-size, size, size, 1.0f), color4),
				new ColoredVertex(new Vector4(-size, size, size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, -size, size, 1.0f), color4),
				new ColoredVertex(new Vector4(size, size, size, 1.0f), color4),
			};

			return vertices;
		}
	}
}
