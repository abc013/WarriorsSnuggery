using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class InfoText : ITickRenderable
	{
		readonly UITextLine infoText;

		int infoTextDuration;

		public InfoText()
		{
			infoText = new UITextLine(FontManager.Pixel16);
		}

		public void Tick()
		{
			if (infoTextDuration-- < 100)
				infoText.Position -= new CPos(64, 0, 0);

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
			var corner = (int)(WindowInfo.UnitWidth * 512);
			infoText.Position = new CPos(-corner + 512, -6144 - 256, 0);
			infoText.WriteText(text);
			infoTextDuration = duration;
		}
	}
}
