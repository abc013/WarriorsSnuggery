using WarriorsSnuggery.Objectives;

namespace WarriorsSnuggery.UI.Objects
{
	class WaveDisplay : DisplayBar, ITick
	{
		readonly WaveObjectiveController controller;
		int currentWave = -1;

		public WaveDisplay(WaveObjectiveController controller) : base(new UIPos(1280, 512), PanelCache.Types["wooden"], new Color(0, 255, 0, 64))
		{
			this.controller = controller;
		}

		public override void Tick()
		{
			base.Tick();

			if (controller.CurrentWave != currentWave)
			{
				currentWave = controller.CurrentWave;
				SetText($"Wave {controller.CurrentWave}/{controller.Waves}");
				DisplayPercentage = controller.CurrentWave / (float)controller.Waves;
			}
		}
	}
}
