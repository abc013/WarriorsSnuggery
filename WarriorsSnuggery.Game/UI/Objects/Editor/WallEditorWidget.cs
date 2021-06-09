using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Objects.Editor
{
	public class WallEditorWidget : UIObject
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				list.Position = value + new CPos(0, 2048, 0);

				var offset = 6144 + 512;

				placementCheck.Position = value + new CPos(-1536, offset, 0);
				placementText.Position = value + new CPos(-1024, offset, 0);

				offset += 776;

				healthText.Position = value + new CPos(0, offset, 0);
				healthSlider.Position = value + new CPos(0, offset + 512, 0);
			}
		}

		readonly PanelList list;

		readonly CheckBox placementCheck;
		readonly UITextLine placementText;

		readonly UITextLine healthText;
		readonly SliderBar healthSlider;

		public WallType CurrentType { get; private set; }
		public bool Horizontal => placementCheck.Checked;
		public float RelativeHP => healthSlider.Value;

		public WallEditorWidget() : base()
		{
			list = new PanelList(new MPos(2048, 4096), new MPos(512, 1024), "wooden");
			foreach (var a in WallCreator.Types.Values)
				list.Add(new PanelListItem(new BatchObject(a.GetTexture(true, 0, a.Texture)), new MPos(512, 512), a.ID.ToString(), new string[0], () => CurrentType = a));

			placementCheck = new CheckBox("wooden");
			placementText = new UITextLine(FontManager.Default);
			placementText.SetText("vertical");

			healthText = new UITextLine(FontManager.Default, TextOffset.MIDDLE);
			healthText.SetText("health");
			healthSlider = new SliderBar(3072, "wooden")
			{
				Value = 1f
			};
		}

		public void DisableTooltip()
		{
			list.DisableTooltip();
		}

		public override void Render()
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

		public override void Tick()
		{
			list.Tick();
			placementCheck.Tick();
			placementText.Tick();

			healthSlider.Tick();
			healthText.Tick();
		}
	}
}
