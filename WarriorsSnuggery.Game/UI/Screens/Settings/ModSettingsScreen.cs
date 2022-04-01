using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class ModSettingsScreen : Screen
	{
		readonly Game game;

		readonly ModList inactive;
		readonly ModList active;

		public ModSettingsScreen(Game game) : base("")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);
			Add(new SettingsChooser(game, new CPos(0, -5120, 0), ScreenType.MODSETTINGS, save));

			var inactiveText = new UIText(FontManager.Default, TextOffset.MIDDLE) { Position = new CPos(-4096, -4096, 0) };
			inactiveText.SetText("Inactive Mods");
			Add(inactiveText);
			inactive = new ModList(4096, "wooden", false) { Position = new CPos(-4096, 512, 0) };

			var activeText = new UIText(FontManager.Default, TextOffset.MIDDLE) { Position = new CPos(4096, -4096, 0) };
			activeText.SetText("Active Mods");
			Add(activeText);
			active = new ModList(4096, "wooden", true) { Position = new CPos(4096, 512, 0) };

			Add(inactive);
			Add(active);

			Add(new Button("→", "wooden", () => switchMod(inactive)) { Position = new CPos(0, -1024, 0) });
			Add(new Button("←", "wooden", () => switchMod(active)) { Position = new CPos(0, 1024, 0) });

			var warning = new UIText(FontManager.Default, TextOffset.MIDDLE)
			{
				Position = new CPos(0, 5450, 0),
				Color = Color.Red
			};
			warning.SetText("Mod changes take effect after restarting and may corrupt saved games.");
			Add(warning);
		}

		void switchMod(ModList from)
		{
			if (from.SelectedMod == null)
				return;

			if (from == active)
				Settings.ModList.Remove(from.SelectedMod.InternalName);
			else
				Settings.ModList.Add(from.SelectedMod.InternalName);

			active.Refresh();
			inactive.Refresh();
		}

		public override void Hide()
		{
			base.Hide();
			save();
		}

		void save()
		{
			Settings.Save();

			game.AddInfoMessage(150, "Mods Saved!");
			Log.Debug("Saved key bindings.");
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.MENU);
		}
	}
}
