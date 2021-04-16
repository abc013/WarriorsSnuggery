using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Trophies;

namespace WarriorsSnuggery.UI.Screens
{
	public class TrophyScreen : Screen
	{
		readonly Game game;

		readonly PanelList trophies;
		readonly UITextBlock information;

		public TrophyScreen(Game game) : base("Trophy Collection")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			trophies = new PanelList(new MPos(8120, 1024), new MPos(512, 1024), "wooden") { Position = new CPos(0, -1024, 0) };
			foreach (var key in TrophyManager.Trophies.Keys)
			{
				var value = TrophyManager.Trophies[key];

				var sprite = value.Image.GetTextures()[0];
				var scale = 24f / Math.Max(sprite.Width, sprite.Height) - 0.1f;
				var item = new PanelItem(new BatchObject(sprite, Color.White), new MPos(512, 512), value.Name, new string[0], () => selectTrophy(key, value))
				{
					Scale = scale * 2f
				};
				if (!game.Statistics.UnlockedTrophies.Contains(key))
					item.SetColor(Color.Black);

				trophies.Add(item);
			}
			Content.Add(trophies);

			Content.Add(new Panel(new MPos(8 * 1024, 1024), "stone") { Position = new CPos(0, 1024, 0) });

			information = new UITextBlock(FontManager.Pixel16, TextOffset.LEFT, "Select a trophy for further information.", "", "") { Position = new CPos(-7900, 512, 0) };
			Content.Add(information);

			Content.Add(new Button("Resume", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)) { Position = new CPos(0, 6144, 0) });
		}

		void selectTrophy(string name, Trophy trophy)
		{
			if (!game.Statistics.UnlockedTrophies.Contains(name))
			{
				information.Lines[0].WriteText(Color.Red + "Locked Trophy");
				information.Lines[1].WriteText(Color.Grey + " ");
				information.Lines[2].WriteText(Color.Grey + " ");
				return;
			}

			information.Lines[0].WriteText(Color.White + trophy.Name);
			information.Lines[1].WriteText(Color.Grey + trophy.Description);
			information.Lines[2].WriteText(Color.Grey + (trophy.MaxManaIncrease != 0 ? "Gives " + Color.Blue + trophy.MaxManaIncrease + Color.Grey + " additional mana storage!" : " "));
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.DEFAULT, false);
		}

		public override void Show()
		{
			var i = 0;
			foreach (var key in TrophyManager.Trophies.Keys)
			{
				if (!game.Statistics.UnlockedTrophies.Contains(key))
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
