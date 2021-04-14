using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery.UI.Objects
{
	public class ActorEditorWidget : UIObject
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				list.Position = value + new CPos(0, 2048, 0);

				var offset = 6144 + 512; 

				rasterizationCheck.Position = value + new CPos(-1536, offset, 0);
				rasterizationText.Position = value + new CPos(-1024, offset, 0);

				offset += 776;

				teamTextBox.Position = value + new CPos(-1536, offset, 0);
				teamTextText.Position = value + new CPos(-1024, offset, 0);

				offset += 776;

				botCheck.Position = value + new CPos(-1536, offset, 0);
				botText.Position = value + new CPos(-1024, offset, 0);

				offset += 776;

				healthText.Position = value + new CPos(0, offset, 0);
				healthSlider.Position = value + new CPos(0, offset + 512, 0);

				offset += 1536;

				facingText.Position = value + new CPos(0, offset, 0);
				facingSlider.Position = value + new CPos(0, offset + 512, 0);
			}
		}

		readonly PanelList list;

		readonly CheckBox rasterizationCheck;
		readonly UITextLine rasterizationText;

		readonly TextBox teamTextBox;
		readonly UITextLine teamTextText;
		readonly CheckBox botCheck;
		readonly UITextLine botText;

		readonly UITextLine healthText;
		readonly SliderBar healthSlider;
		readonly UITextLine facingText;
		readonly SliderBar facingSlider;

		public ActorType CurrentType { get; private set; }
		public bool Rasterization => rasterizationCheck.Checked;

		public bool Bot => botCheck.Checked;
		public byte Team => byte.Parse(teamTextBox.Text);
		public float RelativeHP => healthSlider.Value;
		public float RelativeFacing => facingSlider.Value;

		public ActorEditorWidget(CPos position) : base()
		{
			list = new PanelList(CPos.Zero, new MPos(2048, 4096), new MPos(512, 512), "wooden");
			foreach (var pair in ActorCreator.Types)
			{
				var a = pair.Value;

				var worldTrait = a.PartInfos.FirstOrDefault(p => p is WorldPartInfo);
				if (worldTrait != null && !(worldTrait as WorldPartInfo).ShowInEditor)
					continue;

				var sprite = a.GetPreviewSprite();
				var scale = (sprite.Width > sprite.Height ? 24f / sprite.Width : 24f / sprite.Height) - 0.1f;
				list.Add(new PanelItem(new BatchObject(sprite, Color.White), new MPos(512, 512), a.Playable == null ? pair.Key : a.Playable.Name, new string[0], () => CurrentType = a)
				{
					Scale = scale
				});
			}
			rasterizationCheck = new CheckBox(CPos.Zero, "wooden");
			rasterizationText = new UITextLine(CPos.Zero, FontManager.Pixel16);
			rasterizationText.SetText("align");

			teamTextBox = new TextBox(CPos.Zero, "0", "wooden", 1, true);
			teamTextText = new UITextLine(CPos.Zero, FontManager.Pixel16);
			teamTextText.SetText("team");

			botCheck = new CheckBox(CPos.Zero, "wooden");
			botText = new UITextLine(CPos.Zero, FontManager.Pixel16);
			botText.SetText("bot");

			healthSlider = new SliderBar(CPos.Zero, 3072, "wooden");
			healthSlider.Value = 1f;
			healthText = new UITextLine(CPos.Zero, FontManager.Pixel16, TextOffset.MIDDLE);
			healthText.SetText("health");

			facingSlider = new SliderBar(CPos.Zero, 3072, "wooden");
			facingSlider.Value = 0f;
			facingText = new UITextLine(CPos.Zero, FontManager.Pixel16, TextOffset.MIDDLE);
			facingText.SetText("facing");

			Position = position;
		}

		public void DisableTooltip()
		{
			list.DisableTooltip();
		}

		public override void Render()
		{
			list.Render();
			rasterizationCheck.Render();
			rasterizationText.Render();

			teamTextBox.Render();
			teamTextText.Render();

			botCheck.Render();
			botText.Render();

			healthSlider.Render();
			healthText.Render();

			facingSlider.Render();
			facingText.Render();
		}

		public override void DebugRender()
		{
			list.DebugRender();
			rasterizationCheck.DebugRender();
			rasterizationText.DebugRender();

			teamTextBox.DebugRender();
			teamTextText.DebugRender();

			botCheck.DebugRender();
			botText.DebugRender();

			healthSlider.DebugRender();
			healthText.DebugRender();

			facingSlider.DebugRender();
			facingText.DebugRender();
		}

		public override void Tick()
		{
			list.Tick();
			rasterizationCheck.Tick();
			rasterizationText.Tick();

			teamTextBox.Tick();
			teamTextText.Tick();

			botCheck.Tick();
			botText.Tick();

			healthSlider.Tick();
			healthText.Tick();

			facingSlider.Tick();
			facingText.Tick();
		}
	}
}
