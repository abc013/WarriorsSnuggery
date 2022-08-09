namespace WarriorsSnuggery.UI.Objects
{
	class WaveDisplay : DisplayBar, ITick
	{
		readonly Game game;
		int currentWave = -1;

		public WaveDisplay(Game game) : base(new UIPos(1280, 512), PanelCache.Types["wooden"], new Color(0, 255, 0, 64))
		{
			this.game = game;
		}

		public virtual void Tick()
		{
			if (game.CurrentWave != currentWave)
			{
				currentWave = game.CurrentWave;
				SetText($"Wave {game.CurrentWave}/{game.Waves}");
				DisplayPercentage = game.CurrentWave / (float)game.Waves;
			}
		}
	}
}
