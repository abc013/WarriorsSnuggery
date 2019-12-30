using OpenTK;
using OpenTK.Graphics;

namespace WarriorsSnuggery.Graphics
{
	public struct Vertex
	{
		public const int Size = (4 + 4 + 4) * 4;

		readonly Vector4 position;
		readonly Vector4 textureCoordinate;
		readonly Color4 color;

		public Vertex(Vector4 position, Vector4 textureCoordinate, Color4 color)
		{
			this.position = position;
			this.textureCoordinate = textureCoordinate;
			this.color = color;
		}

		public Vertex UseMatrix(Matrix4 matrix)
		{
			return new Vertex(position * matrix, textureCoordinate, color);
		}

		public override int GetHashCode()
		{
			return position.GetHashCode() ^ textureCoordinate.GetHashCode();
		}
	}

	public struct ColoredVertex
	{
		public const int Size = (4 + 4) * 4;

		readonly Vector4 position;
		readonly Color4 color;

		public ColoredVertex(Vector4 position, Color4 color)
		{
			this.position = position;
			this.color = color;
		}

		public override int GetHashCode()
		{
			return position.GetHashCode() ^ color.GetHashCode();
		}
	}

	public struct TexturedVertex
	{
		// strange behaviour in shader answered: https://stackoverflow.com/questions/4635913/explicit-vs-automatic-attribute-location-binding-for-opengl-shaders#4638906
		public const int Size = (4 + 4 + 4) * 4;

		readonly Vector4 position;
		readonly Vector4 textureCoordinate;
		readonly Color4 color;

		public TexturedVertex(Vector4 position, Vector2 textureCoordinate)
		{
			this.position = position;
			this.textureCoordinate = new Vector4(textureCoordinate);
			color = new Color4(1f, 1, 1, 1);
		}

		public override int GetHashCode()
		{
			return position.GetHashCode() ^ textureCoordinate.GetHashCode() ^ color.GetHashCode();
		}
	}

	public struct CharVertex
	{
		// strange behaviour in shader answered: https://stackoverflow.com/questions/4635913/explicit-vs-automatic-attribute-location-binding-for-opengl-shaders#4638906
		public const int Size = (4 + 4) * 4;

		readonly Vector4 position;
		readonly Vector4 textureCoordinate;

		public CharVertex(Vector4 position, Vector2 textureCoordinate)
		{
			this.position = position;
			this.textureCoordinate = new Vector4(textureCoordinate);
		}

		public override int GetHashCode()
		{
			return position.GetHashCode() ^ textureCoordinate.GetHashCode();
		}
	}
}
