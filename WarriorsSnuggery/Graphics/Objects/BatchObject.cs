using OpenTK.Graphics.ES30;

namespace WarriorsSnuggery.Graphics
{
	public class BatchObject : BatchRenderable
	{
		readonly ITexture texture;

		public BatchObject(ITexture texture, Color color) : base(Mesh.Image(texture, color), color)
		{
			this.texture = texture;
		}

		public BatchObject(Color color) : base(Mesh.Plane(1f, color), color) { }

		public override void PushToBatchRenderer()
		{
			if (texture != null)
			{
				lock (MasterRenderer.GLLock)
				{
					GL.BindTexture(TextureTarget.Texture2D, texture.SheetID);
				}
			}
			base.PushToBatchRenderer();
		}
	}
}
