/*
 * User: Andreas
 * Date: 11.08.2017
 */
using System.Collections.Generic;
using OpenTK.Graphics.ES30;

namespace WarriorsSnuggery.Graphics
{
	public class ISprite : Renderable
	{
		static readonly Dictionary<int, ISprite> Sprites = new Dictionary<int, ISprite>();

		public static ISprite Create(TexturedVertex[] vertices, ITexture[] textures, int tick)
		{
			ISprite sprite;
			
			// textures always come from the same filename, so generate the hash just once
			// and then xor with the amount of textures (avoids same ID for animations with different lengths)
			var key = textures[0].GetHashCode() ^ textures.Length;
			
			// Leftover code:
			//TODO: when textures begin to bug after adding new textures, please look here
			/*foreach (var texture in textures)
			{
				var hash = texture.GetHashCode();
				if (key != hash)
					key ^= hash;
			}*/
			//foreach (var texture in textures)
			//	key += texture.ID;
			//foreach(var vertex in vertices)
			//	key ^= vertex.GetHashCode();

			if (Sprites.ContainsKey(key))
			{
				sprite = Sprites[key];
			}
			else
			{
				sprite = new ISprite(vertices, textures);
				Sprites.Add(key, sprite);
			}

			return sprite;
		}

		public static void DisposeSprites()
		{
			foreach (var sprite in Sprites.Values)
				sprite.Dispose();

			Sprites.Clear();
		}

		public readonly ITexture[] textures;
		
		public int CurTexture;

		ISprite(TexturedVertex[] vertices, ITexture[] textures) : base(MasterRenderer.TextureShader, vertices.Length)
		{
			this.textures = textures;
			IImage.CreateTextureBuffer(vertices, BufferID, VertexArrayID);
		}

		public override void Bind()
		{

			lock(MasterRenderer.GLLock)
			{
				UseProgram();
				GL.BindVertexArray(VertexArrayID);
				Program.CheckGraphicsError("ImageBind_Array");
				GL.BindTexture(TextureTarget.Texture2D, textures[CurTexture].ID);
				Program.CheckGraphicsError("ImageBind_Texture");
			}
		}
	}
}
