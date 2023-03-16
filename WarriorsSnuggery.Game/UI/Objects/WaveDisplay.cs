namespace WarriorsSnuggery.UI.Objects
{
	class WaveDisplay : DisplayBar, ITick
	{
		readonly WaveController controller;
		int currentWave = -1;

		public WaveDisplay(Game game) : base(new UIPos(1280, 512), PanelCache.Types["wooden"], new Color(0, 255, 0, 64))
		{
			controller = game.WaveController;
		}

		public virtual void Tick()
		{
			if (controller.CurrentWave != currentWave)
			{
				currentWave = controller.CurrentWave;
				SetText($"Wave {controller.CurrentWave}/{controller.Waves}");
				DisplayPercentage = controller.CurrentWave / (float)controller.Waves;
			}
		}
	}
}
