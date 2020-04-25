using OpenToolkit.Mathematics;

namespace WarriorsSnuggery.Graphics
{
	public struct Vertex
	{
		// strange behaviour in shader answered: https://stackoverflow.com/questions/4635913/explicit-vs-automatic-attribute-location-binding-for-opengl-shaders#4638906
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

		public Vertex Apply(Matrix4 matrix, Color color)
		{
			return new Vertex(position * matrix, textureCoordinate, color);
		}

		public override int GetHashCode()
		{
			return position.GetHashCode() ^ textureCoordinate.GetHashCode() ^ color.GetHashCode();
		}
	}
}
