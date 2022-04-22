using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery.UI.Objects.Editor
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
		readonly UIText rasterizationText;

		readonly TextBox teamTextBox;
		readonly UIText teamTextText;
		readonly CheckBox botCheck;
		readonly UIText botText;

		readonly UIText healthText;
		readonly SliderBar healthSlider;
		readonly UIText facingText;
		readonly SliderBar facingSlider;

		public ActorType CurrentType { get; private set; }
		public bool Rasterization => rasterizationCheck.Checked;

		public bool Bot => botCheck.Checked;
		public byte Team => byte.Parse(teamTextBox.Text);
		public float RelativeHP => healthSlider.Value;
		public float RelativeFacing => facingSlider.Value;

		public ActorEditorWidget() : base()
		{
			list = new PanelList(new MPos(2048, 4096), new MPos(512, 512), "wooden");
			foreach (var key in ActorCache.Types.Keys)
			{
				var actor = ActorCache.Types[key];

				var worldTrait = actor.PartInfos.FirstOrDefault(p => p is WorldPartInfo);
				if (worldTrait != null && !(worldTrait as WorldPartInfo).ShowInEditor)
					continue;

				var sprite = actor.GetPreviewSprite(out var color);
				var scale = Constants.PixelSize / (float)Math.Max(sprite.Width, sprite.Height) - 0.1f;
				list.Add(new PanelListItem(new BatchObject(Mesh.Image(sprite, color)), new MPos(512, 512), actor.Playable == null ? key : actor.Playable.Name, new string[0], () => CurrentType = actor) { Scale = scale });
			}
			rasterizationCheck = new CheckBox("wooden");
			rasterizationText = new UIText(FontManager.Default);
			rasterizationText.SetText("align");

			teamTextBox = new TextBox("wooden", 1, InputType.NUMBERS) { Text = "0" };
			teamTextText = new UIText(FontManager.Default);
			teamTextText.SetText("team");

			botCheck = new CheckBox("wooden");
			botText = new UIText(FontManager.Default);
			botText.SetText("bot");

			healthSlider = new SliderBar(3072, "wooden", tooltipDigits: 0, displayAsPercent: true)
			{
				Value = 1f
			};
			healthText = new UIText(FontManager.Default, TextOffset.MIDDLE);
			healthText.SetText("health");

			facingSlider = new SliderBar(3072, "wooden", tooltipDigits: 0, valueMultiplier: 360)
			{
				Value = 0f
			};
			facingText = new UIText(FontManager.Default, TextOffset.MIDDLE);
			facingText.SetText("facing");
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

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			teamTextBox.KeyDown(key, isControl, isShift, isAlt);
		}
	}
}
