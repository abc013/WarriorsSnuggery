using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class InfoText : ITickRenderable
	{
		readonly UIText infoText;

		int infoTextDuration;

		public InfoText()
		{
			infoText = new UIText(FontManager.Default);
		}

		public void Tick()
		{
			if (infoTextDuration-- < 100)
				infoText.Position -= new UIPos(64, 0);

			infoText.Tick();
		}

		public void Render()
		{
			if (infoTextDuration > 0)
				infoText.Render();
		}

		public void DebugRender()
		{
			infoText.DebugRender();
		}

		public void SetMessage(int duration, string text)
		{
			if (infoTextDuration < 100)
				UIUtils.PlayPingSound();

			var corner = (int)(WindowInfo.UnitWidth * 512);
			infoText.Position = new UIPos(-corner + 512, -6144 - 256);
			infoText.SetText(text);
			infoTextDuration = duration;
		}
	}
}
