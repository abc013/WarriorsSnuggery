﻿using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	class GameSaveItem : PanelListItem
	{
		readonly int width;
		readonly int imagePathWidth;
		readonly int currentImageDelta;
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				content.Position = value - new UIPos(width - 256, 1024 - FontManager.Default.MaxHeight);
				type.Position = value + new UIPos(width - 256, 0);
				image.Position = value + new UIPos(currentImageDelta, 0);
			}
		}

		public readonly GameSave Save;

		readonly UIText content;
		readonly UIText type;
		readonly UIImage image;

		public GameSaveItem(GameSave save, int width, Action action) : base(new BatchObject(0f), new UIPos(width, 1024), save.Name, generateTooltip(save), action)
		{
			this.width = width;
			imagePathWidth = width - 1024;
			Save = save;

			content = new UIText(FontManager.Default);
			content.SetText(save.Name);
			content.AddText($"{Color.Yellow}score: {save.CalculateScore()}");
			var levelColor = save.Level >= save.FinalLevel ? new Color(0, 200, 0) : Color.Grey;
			content.AddText($"{levelColor}level: {save.Level}/{save.FinalLevel}");

			if (save.OutdatedVersion() || save.InvalidMods())
				content.AddText($"{Color.Red}Outdated! Game may crash.");

			type = new UIText(FontManager.Header, TextOffset.RIGHT);
			type.SetText(save.CurrentMission == MissionType.STORY || save.CurrentMission == MissionType.STORY_MENU ? Color.Cyan + "Story" : Color.Yellow + "Normal");

			image = new UIImage(new BatchObject(UISpriteManager.Get("UI_save")[0]))
			{
				Scale = 2f,
				Color = save.CurrentMission == MissionType.STORY || save.CurrentMission == MissionType.STORY_MENU ? new Color(200, 200, 255, 128) : new Color(255, 255, 200, 128)
            };
			currentImageDelta = width - 1024;

			Position = UIPos.Zero;
		}

		static string[] generateTooltip(GameSave save)
		{
			var list = new List<string>
			{
				$"{Color.White}- Settings -",
				$"{Color.Grey}Difficulty: {save.Difficulty}",
				$"{Color.Grey}Hardcore: {(save.Hardcore ? "Yes" : "No")}",
				$"{Color.White}- Stats -",
				$"{Color.Grey}Mana: {save.Player.Mana}",
				$"{Color.Grey}Money: {save.Player.Money}",
				$"{Color.Grey}Lifes: {save.Player.Lifes}"
			};

			if (save.OutdatedVersion())
				list.Add($"{Color.Red}Warning: Save is from an older game version.");
			if (save.InvalidMods())
				list.Add($"{Color.Red}Warning: Save uses inactive or another order of mods.");

			return list.ToArray();
		}

		public override void Render()
		{
			base.Render();
			if (Visible)
			{
				var delta = Math.Abs(image.Position.X / (float)imagePathWidth);
				var speed = Math.Max(4, (int)((1f - delta * delta) * 256));
				image.Position = new UIPos(Math.Clamp(image.Position.X + (UIUtils.ContainsMouse(this) ? -speed : speed), -imagePathWidth, imagePathWidth), image.Position.Y);
				image.Render();
				type.Render();
				content.Render();
			}
		}
	}
}
