using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public static class Mesh
	{
		static readonly Dictionary<Texture, Vertex[]> meshCache = new Dictionary<Texture, Vertex[]>();

		public static Vertex[] Image(Texture texture)
		{
			if (meshCache.ContainsKey(texture))
				return meshCache[texture];

			var x = texture.X / (float)Settings.SheetSize + Settings.SheetHalfPixel;
			var y = texture.Y / (float)Settings.SheetSize + Settings.SheetHalfPixel;
			var w = (texture.X + texture.Width) / (float)Settings.SheetSize - Settings.SheetHalfPixel;
			var h = (texture.Y + texture.Height) / (float)Settings.SheetSize - Settings.SheetHalfPixel;
			var scale = texture.Height * MasterRenderer.PixelMultiplier / 2;
			var correction = texture.Width / (float)texture.Height;
			var color = Color.White;
			var id = SpriteManager.SheetIndex(texture.SheetID);

			Vertex[] vertices =
			{
				new Vertex(new Vector(scale * correction,  scale,  0), new Vector4(w, y, id, 0), color),
				new Vertex(new Vector(-scale * correction, -scale, 0), new Vector4(x, h, id, 0), color),
				new Vertex(new Vector(scale * correction,  -scale, 0), new Vector4(w, h, id, 0), color),
				new Vertex(new Vector(-scale * correction, scale,  0), new Vector4(x, y, id, 0), color),
				new Vertex(new Vector(-scale * correction, -scale, 0), new Vector4(x, h, id, 0), color),
				new Vertex(new Vector(scale * correction,  scale,  0), new Vector4(w, y, id, 0), color),
			};

			meshCache[texture] = vertices;

			return vertices;
		}

		public static Vertex[] Frame()
		{
			var color = Color.White;
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

		public static Vertex[] UIPanel(Texture texture, MPos bounds)
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
			var id = SpriteManager.SheetIndex(texture.SheetID);

			Vertex[] vertices =
			{
				new Vertex(new Vector(offset.X + size.X * 2, offset.Y + size.Y * 2,  0), new Vector4(w, y, id, 0), color),
				new Vertex(new Vector(offset.X, offset.Y, 0), new Vector4(x, h, id, 0), color),
				new Vertex(new Vector(offset.X + size.X * 2, offset.Y, 0), new Vector4(w, h, id, 0), color),
				new Vertex(new Vector(offset.X, offset.Y + size.Y * 2,  0), new Vector4(x, y, id, 0), color),
				new Vertex(new Vector(offset.X, offset.Y, 0), new Vector4(x, h, id, 0), color),
				new Vertex(new Vector(offset.X + size.X * 2, offset.Y + size.Y * 2, 0), new Vector4(w, y, id, 0), color),
			};

			return vertices;
		}

		public static Vertex[] Line(float size)
		{
			var color = Color.White;

			Vertex[] vertices =
			{
				new Vertex(new Vector(0, 0, 0), new Vector4(-1), color),
				new Vertex(new Vector(0, size, 0), new Vector4(-1), color),
			};

			return vertices;
		}

		public static Vertex[] Plane(float scale)
		{
			var color = Color.White;
			scale /= 2;

			Vertex[] vertices =
			{
				new Vertex(new Vector(scale,  scale,  0), new Vector4(-1), color),
				new Vertex(new Vector(-scale, -scale, 0), new Vector4(-1), color),
				new Vertex(new Vector(scale,  -scale, 0), new Vector4(-1), color),
				new Vertex(new Vector(-scale, scale,  0), new Vector4(-1), color),
				new Vertex(new Vector(-scale, -scale, 0), new Vector4(-1), color),
				new Vertex(new Vector(scale,  scale,  0), new Vector4(-1), color),
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

				vertices[i] = new Vertex(vector, new Vector4(-1), color);
				if (i != 0 && i != resolution * 2 - 1)
					vertices[++i] = new Vertex(vector, new Vector4(-1), color);
			}
			return vertices;
		}

		public static Vertex[] Character(Font font, char c)
		{
			var texture = font.GetTexture(c);

			if (meshCache.ContainsKey(texture))
				return meshCache[texture];

			var x = texture.X / (float)Settings.SheetSize + Settings.SheetHalfPixel;
			var y = texture.Y / (float)Settings.SheetSize + Settings.SheetHalfPixel;
			var w = (texture.X + texture.Width) / (float)Settings.SheetSize - Settings.SheetHalfPixel;
			var h = (texture.Y + texture.Height) / (float)Settings.SheetSize - Settings.SheetHalfPixel;
			var correction = texture.Width / (float)texture.Height;
			var color = Color.White;
			var id = SpriteManager.SheetIndex(texture.SheetID);
			var scale = MasterRenderer.PixelMultiplier * font.Info.Size / 1.5f;

			Vertex[] vertices =
			{
				new Vertex(new Vector(scale * correction,  scale,  0), new Vector4(w, y, id, 0), color),
				new Vertex(new Vector(-scale * correction, -scale, 0), new Vector4(x, h, id, 0), color),
				new Vertex(new Vector(scale * correction,  -scale, 0), new Vector4(w, h, id, 0), color),
				new Vertex(new Vector(-scale * correction, scale,  0), new Vector4(x, y, id, 0), color),
				new Vertex(new Vector(-scale * correction, -scale, 0), new Vector4(x, h, id, 0), color),
				new Vertex(new Vector(scale * correction,  scale,  0), new Vector4(w, y, id, 0), color),
			};

			meshCache[texture] = vertices;

			return vertices;
		}
	}
}
