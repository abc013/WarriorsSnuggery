using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	class ModItem : PanelListItem
	{
		readonly int width;
		readonly int imagePathWidth;
		int currentImageDelta;
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				content.Position = value - new CPos(width - 256, 512 - FontManager.Default.MaxHeight, 0);
				outdated.Position = value + new CPos(width - 256, 0, 0);
				image.Position = value + new CPos(currentImageDelta, 0, 0);
			}
		}

		public readonly Mod Mod;

		readonly UIText content;
		readonly UIText outdated;
		readonly UIImage image;

		public ModItem(Mod mod, int width, Action action) : base(new BatchObject(0f), new MPos(width, ModList.ModHeight), mod.Name + Color.Grey + " | " + mod.Version, new[] { Color.Grey + mod.Description }, action)
		{
			this.width = width;
			imagePathWidth = width - 1024;
			Mod = mod;

			content = new UIText(FontManager.Default);
			content.SetText(mod.Name);
			content.AddText(Color.Grey + mod.Author);

			outdated = new UIText(FontManager.Header, TextOffset.RIGHT);
			if (mod.Outdated)
			{
				outdated.Color = Color.Red.WithAlpha(0.5f);
				outdated.SetText("Outdated!");
			}

			image = new UIImage(new BatchObject(UISpriteManager.Get("UI_gear")[0]))
			{
				Scale = 1.5f,
				Color = Color.White.WithAlpha(0.5f)
			};
			currentImageDelta = width - 1024;

			Position = CPos.Zero;
		}

		public override void Render()
		{
			base.Render();
			if (Visible)
			{
				currentImageDelta = Position.X - image.Position.X;
				var delta = Math.Abs(currentImageDelta / (float)imagePathWidth);
				var speed = Math.Max(4, (int)((1f - delta * delta) * 256));
				image.Position = new CPos(Math.Clamp(image.Position.X + (ContainsMouse ? -speed : speed), Position.X - imagePathWidth, Position.X + imagePathWidth), image.Position.Y, image.Position.Z);
				image.Rotation = new VAngle(0, 0, Math.Clamp(image.Rotation.Z + (ContainsMouse ? -speed : speed), -delta * Angle.MaxRange, 0));
				image.Render();
				outdated.Render();
				content.Render();
			}
		}
	}
}
