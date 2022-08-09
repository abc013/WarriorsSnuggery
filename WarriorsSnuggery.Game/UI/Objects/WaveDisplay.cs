namespace WarriorsSnuggery.UI.Objects
{
	class WaveDisplay : DisplayBar
	{
		readonly Game game;
		int currentWave = -1;

		public WaveDisplay(Game game) : base(new UIPos(1280, 512), PanelCache.Types["wooden"], new Color(0, 255, 0, 64))
		{
			this.game = game;
		}

		public override void Tick()
		{
			base.Tick();

			if (game.CurrentWave != currentWave)
			{
				currentWave = game.CurrentWave;
				SetText($"Wave {game.CurrentWave}/{game.Waves}");
				DisplayPercentage = game.CurrentWave / (float)game.Waves;
			}
		}
	}
}
