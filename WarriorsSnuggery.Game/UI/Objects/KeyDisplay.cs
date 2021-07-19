using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class KeyDisplay : Panel
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				key.SetPosition(value);
			}
		}

		public override VAngle Rotation
		{
			get => base.Rotation;
			set
			{
				base.Rotation = value;

				key.SetRotation(value);
			}
		}

		public override float Scale
		{
			get => base.Scale;
			set
			{
				base.Scale = value;

				key.SetScale(value);
			}
		}

		readonly Game game;
		readonly BatchObject key;
		bool keyFound;
		int scaleCooldown;

		public KeyDisplay(Game game) : base(new MPos(712, 512), PanelCache.Types["wooden"])
		{
			this.game = game;

			key = new BatchObject(UISpriteManager.Get("UI_key")[0]);
			key.SetColor(Color.Black);
		}

		public override void Tick()
		{
			base.Tick();

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
