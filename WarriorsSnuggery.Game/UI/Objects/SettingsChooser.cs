using System;
using WarriorsSnuggery.UI.Screens;

namespace WarriorsSnuggery.UI.Objects
{
	public class SettingsChooser : UIPositionable, ITick, IRenderable, IDebugRenderable
	{
		readonly Button[] chooserButtons = new Button[3 + 2];

		public SettingsChooser(Game game, UIPos position, ScreenType type, Action save) : base()
		{
			chooserButtons[0] = new Button("Settings", type == ScreenType.BASESETTINGS ? "stone" : "wooden", () => game.ScreenControl.ShowScreen(ScreenType.BASESETTINGS));
			chooserButtons[1] = new Button("Key Settings", type == ScreenType.KEYSETTINGS ? "stone" : "wooden", () => game.ScreenControl.ShowScreen(ScreenType.KEYSETTINGS));
			chooserButtons[2] = new Button("Mod Settings", type == ScreenType.MODSETTINGS ? "stone" : "wooden", () => game.ScreenControl.ShowScreen(ScreenType.MODSETTINGS));

			var width = (chooserButtons[0].Bounds.X + chooserButtons[1].Bounds.X);
			chooserButtons[0].Position = position - new UIPos(width, 0);
			chooserButtons[1].Position = position;
			width = chooserButtons[1].Bounds.X + chooserButtons[2].Bounds.X;
			chooserButtons[2].Position = position + new UIPos(width, 0);

			chooserButtons[3] = new Button("Apply", "wooden", save) { Position = new UIPos(-5120, 6144) };
			chooserButtons[4] = new Button("Save & Back", "wooden", () => game.ShowScreen(ScreenType.MENU)) { Position = new UIPos(5120, 6144) };
		}

		public void Tick()
		{
			foreach (var button in chooserButtons)
				button.Tick();
		}

		public void Render()
		{
			foreach (var button in chooserButtons)
				button.Render();
		}

		public override void DebugRender()
		{
			foreach (var button in chooserButtons)
				button.DebugRender();
		}
	}
}
