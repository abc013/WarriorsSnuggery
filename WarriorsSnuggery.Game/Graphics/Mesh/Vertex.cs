using OpenTK.Mathematics;

namespace WarriorsSnuggery.Graphics
{
	public struct Vertex
	{
		public const int PositionAttributeLocation = 0;
		public const int TextureCoordinateAttributeLocation = 1;
		public const int TextureAttributeLocation = 2;
		public const int TextureFlagsAttributeLocation = 3;
		public const int ColorAttributeLocation = 4;

		// strange behaviour in shader answered: https://stackoverflow.com/questions/4635913/explicit-vs-automatic-attribute-location-binding-for-opengl-shaders#4638906
		public const int Size = (4 + 2 + 1 + 1 + 4) * 4;

		readonly Vector4 position;
		readonly Vector2 textureCoordinate;
		readonly int texture;
		readonly TextureFlags textureFlags;
		readonly Color color;

		public Vertex(in Vector3 position, in Vector2 textureCoordinate, int texture, in Color color, TextureFlags textureFlags = TextureFlags.None)
		{
			this.position = new Vector4(position, 1.0f);
			this.textureCoordinate = textureCoordinate;
			this.texture = texture;
			this.textureFlags = textureFlags;
			this.color = color;
		}

		public Vertex Modify(in Vector3 offset, in Vector3 scale, in Quaternion rotation, in Color color, TextureFlags textureFlags)
		{
			// Prevent calculations if no rotation is required
			var pos = position.Xyz;
			if (rotation != Quaternion.Identity)
				pos = Vector3.Transform(pos, rotation);

			// Prevent calculations if one or both colors are white
			var col = color;
			if (col == Color.White)
				col = this.color;
			else if (this.color != Color.White)
				col *= this.color;

			return new Vertex((pos * scale) + offset, textureCoordinate, texture, col, textureFlags);
		}

		public override int GetHashCode()
		{
			return position.GetHashCode() ^ textureCoordinate.GetHashCode() ^ color.GetHashCode() ^ texture.GetHashCode() ^ textureFlags.GetHashCode();
		}
	}
}
