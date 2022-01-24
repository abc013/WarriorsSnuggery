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

		public Vertex(Vector3 position, Vector2 textureCoordinate, int texture, Color color)
		{
			this.position = new Vector4(position, 1.0f);
			this.textureCoordinate = textureCoordinate;
			this.texture = texture;
			this.color = color;
		}

		public Vertex Modify(Vector3 offset, Vector3 scale, Quaternion rotation, Color color)
		{
			return new Vertex((Vector3.Transform(position.Xyz, rotation) * scale) + offset, textureCoordinate, texture, color);
		}

		public override int GetHashCode()
		{
			return position.GetHashCode() ^ textureCoordinate.GetHashCode() ^ color.GetHashCode();
		}
	}
}
