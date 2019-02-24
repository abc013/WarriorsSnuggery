﻿/*
 * User: Andreas
 * Date: 28.04.2018
 * Time: 01:48
 */
using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class PausedScreen : Screen
	{
		readonly Text paused;
		readonly Game game;
		public PausedScreen(Game game) : base("Paused")
		{
			this.game = game;
			paused = new Text(new CPos(0,2048, 0), IFont.Pixel16, Text.OffsetType.MIDDLE);
			paused.WriteText(new Color(128, 128, 255) + "To unpause, press '" + Color.Yellow + "P" + new Color(128, 128, 255) + "'");
		}

		public override void Render()
		{
			base.Render();

			paused.Render();
		}

		public override void Tick()
		{
			base.Tick();

			paused.Tick();

			if(KeyInput.IsKeyDown("p", 10))
			{
				game.Pause(false);
				game.ChangeScreen(ScreenType.DEFAULT);
			}
		}

		public override void Dispose()
		{
			base.Dispose();

			paused.Dispose();
		}
	}
}
