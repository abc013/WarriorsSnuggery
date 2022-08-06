using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	class GameSaveItem : PanelListItem
	{
		readonly int width;
		readonly int imagePathWidth;
		readonly int currentImageDelta;
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				content.Position = value - new CPos(width - 256, 1024 - FontManager.Default.MaxHeight, 0);
				type.Position = value + new CPos(width - 256, 0, 0);
				image.Position = value + new CPos(currentImageDelta, 0, 0);
			}
		}

		public readonly GameSave Save;

		readonly UIText content;
		readonly UIText type;
		readonly UIImage image;

		public GameSaveItem(GameSave save, int width, Action action) : base(new BatchObject(0f), new MPos(width, 1024), save.Name, new[] { Color.Grey + "Difficulty: " + save.Difficulty, Color.Grey + "Money: " + save.Money }, action)
		{
			this.width = width;
			imagePathWidth = width - 1024;
			Save = save;

			content = new UIText(FontManager.Default);
			content.SetText(save.Name);
			content.AddText($"{Color.Yellow}score: {save.CalculateScore()}");
			var levelColor = save.Level >= save.FinalLevel ? new Color(0, 200, 0) : Color.Grey;
			content.AddText($"{levelColor}level: {save.Level}/{save.FinalLevel}");
			if (save.GameSaveFormat < Constants.CurrentGameSaveFormat)
				content.AddText($"{Color.Red}Outdated! Game may crash.");

			type = new UIText(FontManager.Header, TextOffset.RIGHT);
			type.SetText(save.CurrentMission == MissionType.STORY || save.CurrentMission == MissionType.STORY_MENU ? Color.Cyan + "Story" : Color.Yellow + "Normal");

			image = new UIImage(new BatchObject(UISpriteManager.Get("UI_save")[0]))
			{
				Scale = 2f,
				Color = save.CurrentMission == MissionType.STORY || save.CurrentMission == MissionType.STORY_MENU ? new Color(200, 200, 255, 128) : new Color(255, 255, 200, 128)
            };
			currentImageDelta = width - 1024;

			Position = CPos.Zero;
		}

		public override void Render()
		{
			base.Render();
			if (Visible)
			{
				var delta = Math.Abs(image.Position.X / (float)imagePathWidth);
				var speed = Math.Max(4, (int)((1f - delta * delta) * 256));
				image.Position = new CPos(Math.Clamp(image.Position.X + (ContainsMouse ? -speed : speed), -imagePathWidth, imagePathWidth), image.Position.Y, image.Position.Z);
				image.Render();
				type.Render();
				content.Render();
			}
		}
	}
}
