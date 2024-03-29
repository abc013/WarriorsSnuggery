﻿using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery.UI.Objects.Editor
{
	public class ActorEditorWidget : UIPositionable, IDisableTooltip, IRenderable, ITick, ICheckKeys
	{
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				list.Position = value + new UIPos(0, 2048);

				var offset = 6144 + 512; 

				rasterizationCheck.Position = value + new UIPos(-1536, offset);
				rasterizationText.Position = value + new UIPos(-1024, offset);

				offset += 776;

				teamTextBox.Position = value + new UIPos(-1536, offset);
				teamTextText.Position = value + new UIPos(-1024, offset);

				offset += 776;

				botCheck.Position = value + new UIPos(-1536, offset);
				botText.Position = value + new UIPos(-1024, offset);

				offset += 776;

				healthText.Position = value + new UIPos(0, offset);
				healthSlider.Position = value + new UIPos(0, offset + 512);

				offset += 1536;

				facingText.Position = value + new UIPos(0, offset);
				facingSlider.Position = value + new UIPos(0, offset + 512);
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
			list = new PanelList(new UIPos(2048, 4096), new UIPos(512, 512), "wooden");
			foreach (var key in ActorCache.Types.Keys)
			{
				var actor = ActorCache.Types[key];

				var worldTrait = actor.PartInfos.FirstOrDefault(p => p is WorldPartInfo);
				if (worldTrait != null && !(worldTrait as WorldPartInfo).ShowInEditor)
					continue;

				var sprite = actor.GetPreviewSprite(out var color);
				var scale = Constants.PixelSize / (float)Math.Max(sprite.Width, sprite.Height) - 0.1f;
				list.Add(new PanelListItem(new BatchObject(Mesh.Image(sprite, color)), new UIPos(512, 512), actor.Playable == null ? key : actor.Playable.Name, new string[0], () => CurrentType = actor) { Scale = scale });
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

		public void Render()
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

		public void Tick()
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

		public void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			teamTextBox.KeyDown(key, isControl, isShift, isAlt);
		}
	}
}
