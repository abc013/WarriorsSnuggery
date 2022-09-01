using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Objects.Editor
{
	public class WallEditorWidget : UIPositionable, ITick, IRenderable
	{
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				list.Position = value + new UIPos(0, 2048);

				var offset = 6144 + 512;

				placementCheck.Position = value + new UIPos(-1536, offset);
				placementText.Position = value + new UIPos(-1024, offset);

				offset += 776;

				healthText.Position = value + new UIPos(0, offset);
				healthSlider.Position = value + new UIPos(0, offset + 512);
			}
		}

		readonly PanelList list;

		readonly CheckBox placementCheck;
		readonly UIText placementText;

		readonly UIText healthText;
		readonly SliderBar healthSlider;

		public WallType CurrentType { get; private set; }
		public bool Horizontal => placementCheck.Checked;
		public float RelativeHP => healthSlider.Value;

		public WallEditorWidget() : base()
		{
			list = new PanelList(new UIPos(2048, 4096), new UIPos(512, 1024), "wooden");
			foreach (var a in WallCache.Types.Values)
				list.Add(new PanelListItem(new BatchObject(a.GetTexture(true, 0, a.Texture)), new UIPos(512, 512), a.ID.ToString(), new string[0], () => CurrentType = a));

			placementCheck = new CheckBox("wooden");
			placementText = new UIText(FontManager.Default);
			placementText.SetText("horizontal");

			healthText = new UIText(FontManager.Default, TextOffset.MIDDLE);
			healthText.SetText("health");
			healthSlider = new SliderBar(3072, "wooden", tooltipDigits: 0, displayAsPercent: true)
			{
				Value = 1f
			};
		}

		public void DisableTooltip()
		{
			list.DisableTooltip();
		}

		public void Render()
		{
			list.Render();
			placementCheck.Render();
			placementText.Render();

			healthSlider.Render();
			healthText.Render();
		}

		public override void DebugRender()
		{
			list.DebugRender();
			placementCheck.DebugRender();
			placementText.DebugRender();

			healthSlider.DebugRender();
			healthText.DebugRender();
		}

		public void Tick()
		{
			list.Tick();
			placementCheck.Tick();
			placementText.Tick();

			healthSlider.Tick();
			healthText.Tick();
		}
	}
}
