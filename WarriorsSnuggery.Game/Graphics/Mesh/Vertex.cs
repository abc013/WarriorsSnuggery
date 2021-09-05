using OpenTK.Mathematics;

namespace WarriorsSnuggery.Graphics
{
	public struct Vertex
	{
		public const int PositionAttributeLocation = 0;
		public const int TextureCoordinateAttributeLocation = 1;
		public const int TextureAttributeLocation = 2;
		public const int ColorAttributeLocation = 3;

		// strange behaviour in shader answered: https://stackoverflow.com/questions/4635913/explicit-vs-automatic-attribute-location-binding-for-opengl-shaders#4638906
		public const int Size = (4 + 2 + 1 + 4) * 4;

		readonly Vector4 position;
		readonly Vector2 textureCoordinate;
		readonly int texture;
		readonly Color color;

		public Vertex(Vector4 position, Vector2 textureCoordinate, int texture, Color color)
		{
			this.position = position;
			this.textureCoordinate = textureCoordinate;
			this.texture = texture;
			this.color = color;
		}

		public Vertex Apply(Matrix4 matrix, Color color)
		{
			return new Vertex(position * matrix, textureCoordinate, texture, color);
		}

		public override int GetHashCode()
		{
			return position.GetHashCode() ^ textureCoordinate.GetHashCode() ^ color.GetHashCode();
		}
	}
}
