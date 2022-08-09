using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public static class Mesh
	{
		static readonly Dictionary<(Texture, Color), Vertex[]> meshCache = new Dictionary<(Texture, Color), Vertex[]>();

		public static Vertex[] Character(Font font, char c)
		{
			return Image(font.GetTexture(c));
		}

		public static Vertex[] Image(Texture texture)
		{
			return Image(texture, Color.White);
		}

		public static Vertex[] Image(Texture texture, Color color)
		{
			if (meshCache.ContainsKey((texture, color)))
				return meshCache[(texture, color)];

			var x = texture.X / (float)Settings.SheetSize + Settings.SheetHalfPixel;
			var y = texture.Y / (float)Settings.SheetSize + Settings.SheetHalfPixel;
			var w = (texture.X + texture.Width) / (float)Settings.SheetSize - Settings.SheetHalfPixel;
			var h = (texture.Y + texture.Height) / (float)Settings.SheetSize - Settings.SheetHalfPixel;
			var scale = texture.Height * Constants.PixelMultiplier / 2;
			var correction = texture.Width / (float)texture.Height;
			var id = SheetManager.SheetIndex(texture.SheetID);

			Vertex[] vertices =
			{
				new Vertex(new Vector(scale * correction,  scale,  0), new Vector2(w, y), id, color),
				new Vertex(new Vector(-scale * correction, -scale, 0), new Vector2(x, h), id, color),
				new Vertex(new Vector(scale * correction,  -scale, 0), new Vector2(w, h), id, color),
				new Vertex(new Vector(-scale * correction, scale,  0), new Vector2(x, y), id, color),
				new Vertex(new Vector(-scale * correction, -scale, 0), new Vector2(x, h), id, color),
				new Vertex(new Vector(scale * correction,  scale,  0), new Vector2(w, y), id, color),
			};

			meshCache[(texture, color)] = vertices;

			return vertices;
		}

		public static Vertex[] Frame()
		{
			var color = Color.White;
			var scale = 1;

			Vertex[] vertices =
			{
				new Vertex(new Vector(scale,  -scale,  0), new Vector2(1, 0), 0, color),
				new Vertex(new Vector(-scale, scale, 0), new Vector2(0, 1), 0, color),
				new Vertex(new Vector(scale,  scale, 0), new Vector2(1, 1), 0, color),
				new Vertex(new Vector(-scale, -scale,  0), new Vector2(0, 0), 0, color),
				new Vertex(new Vector(-scale, scale, 0), new Vector2(0, 1), 0, color),
				new Vertex(new Vector(scale,  -scale,  0), new Vector2(1, 0), 0, color),
			};

			return vertices;
		}

		public static Vertex[] UIPanel(Texture texture, UIPos bounds)
		{
			var color = Color.White;
			var size = new Vector(bounds.X / 1024f, bounds.Y / 1024f, 0);

			return UIPanel(texture, color, size);
		}

		public static Vertex[] UIPanel(Texture texture, Color color, Vector size)
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
					Array.Copy(uiPanelPiece(texture, color, lOffset, lSize), 0, vertices, index, 6);
					index += 6;
				}
			}

			return vertices;
		}

		static Vertex[] uiPanelPiece(Texture texture, Color color, Vector offset, Vector size)
		{
			var x = texture.X / (float)Settings.SheetSize + Settings.SheetHalfPixel;
			var y = texture.Y / (float)Settings.SheetSize + Settings.SheetHalfPixel;
			var w = (texture.X + size.X * texture.Width) / Settings.SheetSize - Settings.SheetHalfPixel;
			var h = (texture.Y + size.Y * texture.Height) / Settings.SheetSize - Settings.SheetHalfPixel;
			var id = SheetManager.SheetIndex(texture.SheetID);

			Vertex[] vertices =
			{
				new Vertex(new Vector(offset.X + size.X * 2, offset.Y + size.Y * 2,  0), new Vector2(w, y), id, color),
				new Vertex(new Vector(offset.X, offset.Y, 0), new Vector2(x, h), id, color),
				new Vertex(new Vector(offset.X + size.X * 2, offset.Y, 0), new Vector2(w, h), id, color),
				new Vertex(new Vector(offset.X, offset.Y + size.Y * 2,  0), new Vector2(x, y), id, color),
				new Vertex(new Vector(offset.X, offset.Y, 0), new Vector2(x, h), id, color),
				new Vertex(new Vector(offset.X + size.X * 2, offset.Y + size.Y * 2, 0), new Vector2(w, y), id, color),
			};

			return vertices;
		}

		public static Vertex[] Line(float size)
		{
			var color = Color.White;

			Vertex[] vertices =
			{
				new Vertex(new Vector(0, 0, 0), Vector2.Zero, -1, color),
				new Vertex(new Vector(0, size, 0), Vector2.Zero, -1, color),
			};

			return vertices;
		}

		public static Vertex[] Plane(float scale)
		{
			var color = Color.White;
			scale /= 2;

			Vertex[] vertices =
			{
				new Vertex(new Vector(scale,  scale,  0), Vector2.Zero, -1, color),
				new Vertex(new Vector(-scale, -scale, 0), Vector2.Zero, -1, color),
				new Vertex(new Vector(scale,  -scale, 0), Vector2.Zero, -1, color),
				new Vertex(new Vector(-scale, scale,  0), Vector2.Zero, -1, color),
				new Vertex(new Vector(-scale, -scale, 0), Vector2.Zero, -1, color),
				new Vertex(new Vector(scale,  scale,  0), Vector2.Zero, -1, color),
			};

			return vertices;
		}

		public static Vertex[] Circle(float size, int resolution = 360)
		{
			var color = Color.White;
			size /= 2;

			var vertices = new Vertex[resolution * 2];
			for (int i = 0; i < resolution * 2; i++)
			{
				var vector = Vector.FromFlatAngle(i * MathF.PI / resolution * 2, size);

				vertices[i] = new Vertex(vector, Vector2.Zero, -1, color);
				if (i != 0 && i != resolution * 2 - 1)
					vertices[++i] = new Vertex(vector, Vector2.Zero, -1, color);
			}
			return vertices;
		}
	}
}
