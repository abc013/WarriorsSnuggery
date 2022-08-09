using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class ModSettingsScreen : Screen
	{
		readonly Game game;

		readonly PackageList inactive;
		readonly PackageList active;

		public ModSettingsScreen(Game game) : base("")
		{
			this.game = game;
			Title.Position = new UIPos(0, -4096);
			Add(new SettingsChooser(game, new UIPos(0, -5120), ScreenType.MODSETTINGS, save));

			var inactiveText = new UIText(FontManager.Default, TextOffset.MIDDLE) { Position = new UIPos(-4096, -4096) };
			inactiveText.SetText("Inactive Mods");
			Add(inactiveText);
			inactive = new PackageList(4096, "wooden", false) { Position = new UIPos(-4096, 512) };

			var activeText = new UIText(FontManager.Default, TextOffset.MIDDLE) { Position = new UIPos(4096, -4096) };
			activeText.SetText("Active Mods");
			Add(activeText);
			active = new PackageList(4096, "wooden", true) { Position = new UIPos(4096, 512) };

			Add(inactive);
			Add(active);

			Add(new Button("→", "wooden", () => switchPackage(inactive)) { Position = new UIPos(0, -1024) });
			Add(new Button("←", "wooden", () => switchPackage(active)) { Position = new UIPos(0, 1024) });

			var warning = new UIText(FontManager.Default, TextOffset.MIDDLE)
			{
				Position = new UIPos(0, 5450),
				Color = Color.Red
			};
			warning.SetText("Mods changes take effect after restarting and may corrupt saved games.");
			Add(warning);
		}

		void switchPackage(PackageList from)
		{
			if (from.SelectedPackage == null)
				return;

			if (from == active)
				Settings.PackageList.Remove(from.SelectedPackage.InternalName);
			else
				Settings.PackageList.Add(from.SelectedPackage.InternalName);

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
