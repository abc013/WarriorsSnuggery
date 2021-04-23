﻿using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.IO;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class CreditsScreen : Screen
	{
		readonly Game game;

		readonly List<UITextLine> lines = new List<UITextLine>();

		readonly UIImage wsImage;
		int wsImageTick;

		readonly string[] lineData;
		int currentLine;

		readonly int lineHeight;
		int currentHeight;

		public CreditsScreen(Game game) : base("", 255)
		{
			this.game = game;

			lineData = File.ReadAllLines(FileExplorer.FindIn(FileExplorer.Rules, "Credits", ".yaml"));

			lineHeight = FontManager.Pixel16.Height * 2;

			wsImage = new UIImage(new BatchObject(UITextureManager.Get("logo")[0])) { Color = new Color(0, 0, 0, 0) };
		}

		public override void Tick()
		{
			const int movement = 12;

			base.Tick();

			var toRemove = new List<UITextLine>();

			foreach (var line in lines)
			{
				line.Position -= new CPos(0, movement, 0);
				if (line.Position.Y <= Top - lineHeight / 2)
					toRemove.Add(line);
			}

			foreach (var line in toRemove)
				lines.Remove(line);

			currentHeight += movement;
			if (currentHeight >= 0 && currentLine < lineData.Length)
			{
				var newLine = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE)
				{
					Position = new CPos(0, Bottom + lineHeight / 2, 0)
				};
				newLine.SetText(lineData[currentLine++]);

				lines.Add(newLine);

				currentHeight -= lineHeight;
			}

			if (lines.Count == 0)
			{
				wsImageTick++;

				var color = Math.Min((float)wsImageTick / (Settings.UpdatesPerSecond * 10) + Math.Min(wsImageTick / 100f, 0.1f), 1f);
				wsImage.Color = new Color(color, color, color, color);

				var scale = Math.Min((float)Math.Log10(wsImageTick + 100) / 3f, 1f);
				wsImage.Scale = scale;

				if (wsImageTick == 900)
				{
					var text = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE)
					{
						Color = Color.Green,
						Position = new CPos(0, 2048, 0)
					};
					text.SetText("Press any key to continue");

					Add(text);
				}
			}
		}

		public override void Render()
		{
			base.Render();

			foreach (var line in lines)
				line.Render();

			if (wsImage.Color.A != 0)
				wsImage.Render();
		}

		public override void DebugRender()
		{
			base.DebugRender();

			foreach (var line in lines)
				line.DebugRender();
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			game.ShowScreen(ScreenType.DEFAULT, false);
		}
	}
}