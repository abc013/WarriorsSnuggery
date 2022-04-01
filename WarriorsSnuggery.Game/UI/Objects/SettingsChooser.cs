using System;
using WarriorsSnuggery.UI.Screens;

namespace WarriorsSnuggery.UI.Objects
{
	public class SettingsChooser : UIObject
	{
		readonly Button[] chooserButtons = new Button[3 + 2];

		public SettingsChooser(Game game, CPos position, ScreenType type, Action save) : base()
		{
			chooserButtons[0] = new Button("Settings", type == ScreenType.BASESETTINGS ? "stone" : "wooden", () => game.ScreenControl.ShowScreen(ScreenType.BASESETTINGS));
			chooserButtons[1] = new Button("Key Settings", type == ScreenType.KEYSETTINGS ? "stone" : "wooden", () => game.ScreenControl.ShowScreen(ScreenType.KEYSETTINGS));
			chooserButtons[2] = new Button("Mod Settings", type == ScreenType.MODSETTINGS ? "stone" : "wooden", () => game.ScreenControl.ShowScreen(ScreenType.MODSETTINGS));

			var width = (chooserButtons[0].Bounds.X + chooserButtons[1].Bounds.X);
			chooserButtons[0].Position = position - new CPos(width, 0, 0);
			chooserButtons[1].Position = position;
			width = chooserButtons[1].Bounds.X + chooserButtons[2].Bounds.X;
			chooserButtons[2].Position = position + new CPos(width, 0, 0);

			chooserButtons[3] = new Button("Apply", "wooden", save) { Position = new CPos(-5120, 6144, 0) };
			chooserButtons[4] = new Button("Save & Back", "wooden", () => game.ShowScreen(ScreenType.MENU)) { Position = new CPos(5120, 6144, 0) };
		}

		public override void Tick()
		{
			base.Tick();

			foreach (var button in chooserButtons)
				button.Tick();
		}

		public override void Render()
		{
			base.Render();

			foreach (var button in chooserButtons)
				button.Render();
		}

		public override void DebugRender()
		{
			base.DebugRender();

			foreach (var button in chooserButtons)
				button.DebugRender();
		}
	}
}
