using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	class PackageItem : PanelListItem
	{
		readonly int width;
		readonly int imagePathWidth;
		int currentImageDelta;
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				content.Position = value - new UIPos(width - 256, 512 - FontManager.Default.MaxHeight);
				outdated.Position = value + new UIPos(width - 256, 0);
				image.Position = value + new UIPos(currentImageDelta, 0);
			}
		}

		public readonly Package Package;

		readonly UIText content;
		readonly UIText outdated;
		readonly UIImage image;

		public PackageItem(Package package, int width, Action action) : base(new BatchObject(0f), new UIPos(width, PackageList.ItemHeight), package.Name + Color.Grey + " | " + package.Version, new[] { Color.Grey + package.Description }, action)
		{
			this.width = width;
			imagePathWidth = width - 1024;
			Package = package;

			content = new UIText(FontManager.Default);
			content.SetText(package.Name);
			content.AddText(Color.Grey + package.Author);

			outdated = new UIText(FontManager.Header, TextOffset.RIGHT);
			if (package.Outdated)
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

			Position = UIPos.Zero;
		}

		public override void Render()
		{
			base.Render();
			if (Visible)
			{
				currentImageDelta = Position.X - image.Position.X;
				var delta = Math.Abs(currentImageDelta / (float)imagePathWidth);
				var speed = Math.Max(4, (int)((1f - delta * delta) * 256));
				var containsMouse = UIUtils.ContainsMouse(this);
				image.Position = new UIPos(Math.Clamp(image.Position.X + (containsMouse ? -speed : speed), Position.X - imagePathWidth, Position.X + imagePathWidth), image.Position.Y);
				image.Rotation = new VAngle(0, 0, Math.Clamp(image.Rotation.Z + (containsMouse ? -speed : speed), -delta * Angle.MaxRange, 0));
				image.Render();
				outdated.Render();
				content.Render();
			}
		}
	}
}
