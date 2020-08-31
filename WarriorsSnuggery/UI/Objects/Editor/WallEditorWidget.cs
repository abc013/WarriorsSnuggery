using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Objects
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

		public WallEditorWidget(CPos position) : base()
		{
			list = new PanelList(CPos.Zero, new MPos(2048, 4096), new MPos(512, 1024), PanelManager.Get("wooden"));
			foreach (var a in WallCreator.Types.Values)
				list.Add(new PanelItem(new BatchObject(a.GetTexture(true, 0), Color.White), new MPos(512, 512), a.ID.ToString(), new string[0], () => CurrentType = a));

			placementCheck = CheckBoxCreator.Create("wooden", CPos.Zero, false, (b) => { });
			placementText = new UITextLine(CPos.Zero, FontManager.Pixel16);
			placementText.SetText("vertical");

			healthText = new UITextLine(CPos.Zero, FontManager.Pixel16, TextOffset.MIDDLE);
			healthText.SetText("health");
			healthSlider = new SliderBar(CPos.Zero, 3072, PanelManager.Get("wooden"), () => { });
			healthSlider.Value = 1f;

			Position = position;
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
