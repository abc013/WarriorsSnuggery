using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public sealed class KeyDisplay : Panel, ITick
	{
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				key.SetPosition(value);
			}
		}

		readonly Game game;
		readonly BatchObject key;
		bool keyFound;
		int scaleCooldown;

		public KeyDisplay(Game game) : base(new UIPos(712, 512), PanelCache.Types["wooden"])
		{
			this.game = game;

			key = new BatchObject(UISpriteManager.Get("UI_key")[0]);
			key.SetColor(Color.Black);
		}

		public void Tick()
		{
			if (!keyFound && game.Stats.KeyFound)
			{
				keyFound = true;

				key.SetColor(Color.White);
				scaleCooldown = 10;
			}

			if (keyFound && scaleCooldown-- > 0)
				key.SetScale((scaleCooldown / 10f) + 1f);
		}

		public override void Render()
		{
			base.Render();

			key.Render();
		}
	}
}
