using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public class LoadingScreen : IRenderable
	{
		readonly ColoredCircle loading, loading2, loading3;
		int rot;

		readonly GameObject icon;

		public bool Visible { get; private set;}

		public LoadingScreen()
		{
			loading = new ColoredCircle(new CPos(0, 2048,0), new Color(128,128,190), 1.5f, 8, isFilled: false);
			loading2 = new ColoredCircle(new CPos(0, 2048,0), new Color(0,0,128), 1.5f, 8, true);
			loading3 = new ColoredCircle(new CPos(0, 2048,0), new Color(64,64,190), 0.75f, 8, true);
			icon = new GameObject(new CPos(0, -2048, 0), new ImageRenderable(TextureManager.Texture("loading")))
			{
				Scale = 5f
			};
		}

		public void SetVisible(bool visible)
		{
			Visible = visible;
		}

		public void Render()
		{
			icon.Render();
			loading.Rotation = new CPos(0,0, rot++);
			loading2.Tick();
			loading3.Tick();
			loading2.Render();
			loading.Render();
			loading3.Render();
		}

		public void Dispose()
		{
			loading.Dispose();
			loading2.Dispose();
			loading3.Dispose();
			icon.Dispose();
		}
	}
}
