﻿using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Trophies;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class TrophyScreen : Screen
	{
		readonly Game game;

		readonly PanelList trophies;
		readonly UIText information;

		public TrophyScreen(Game game) : base("Trophy Collection")
		{
			this.game = game;
			Title.Position = new UIPos(0, -4096);

			trophies = new PanelList(new UIPos(8120, 1024), new UIPos(512, 1024), "wooden") { Position = new UIPos(0, -1024) };
			foreach (var key in TrophyCache.Trophies.Keys)
			{
				var value = TrophyCache.Trophies[key];

				var sprite = value.Image.GetTextures()[0];
				var scale = Constants.PixelSize / (float)Math.Max(sprite.Width, sprite.Height) - 0.1f;
				var item = new PanelListItem(new BatchObject(sprite), new UIPos(512, 1024), value.Name, new string[0], () => selectTrophy(key, value)) { Scale = scale * 2f };
				if (!game.Player.HasTrophyUnlocked(key))
					item.SetColor(Color.Black);

				trophies.Add(item);
			}
			Add(trophies);

			Add(new Panel(new UIPos(8 * 1024, 1024), "stone") { Position = new UIPos(0, 1024) });

			information = new UIText(FontManager.Default, TextOffset.LEFT, "Select a trophy for further information.", "", "", "") { Position = new UIPos(-7900, 512 - 128) };
			Add(information);

			Add(new Button("Resume", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)) { Position = new UIPos(0, 6144) });
		}

		void selectTrophy(string name, Trophy trophy)
		{
			if (!game.Player.HasTrophyUnlocked(name))
			{
				information.SetText(Color.Red + "Trophy Locked.");
				return;
			}

			information.SetText(Color.White + trophy.Name, Color.Grey + trophy.Description);
			if (trophy.MaxManaIncrease != 0)
				information.AddText($"{Color.Grey}Gives {Color.Blue}{trophy.MaxManaIncrease} {Color.Grey}additional mana storage!");
			if (trophy.MaxLifesIncrease != 0)
				information.AddText($"{Color.Grey}Gives {Color.Red}{trophy.MaxLifesIncrease} {Color.Grey}additional life{(trophy.MaxLifesIncrease > 1 ? "s" : "")}!");
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.DEFAULT, false);
		}

		public override void Show()
		{
			var i = 0;
			foreach (var key in TrophyCache.Trophies.Keys)
			{
				if (!game.Player.HasTrophyUnlocked(key))
					trophies.Container[i].SetColor(Color.Black);
				else
					trophies.Container[i].SetColor(Color.White);

				i++;
			}
		}

		public override void Hide()
		{
			trophies.DisableTooltip();
		}
	}
}
