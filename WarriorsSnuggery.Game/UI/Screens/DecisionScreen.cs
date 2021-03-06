﻿using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class DecisionScreen : Screen
	{
		readonly UITextLine text;

		Action onDecline;
		Action onAgree;
		Button decline;
		Button agree;

		public DecisionScreen(Game game) : base("Are you sure?")
		{
			Title.Position = new CPos(0, -2048, 0);

			text = new UITextLine(CPos.Zero, FontManager.Pixel16, TextOffset.MIDDLE);
			Content.Add(text);
		}

		public void SetAction(Action onDecline, Action onAgree, string text)
		{
			this.text.SetText(text);

			this.onDecline = onDecline;
			this.onAgree = onAgree;

			Content.Remove(decline);
			Content.Remove(agree);

			decline = new Button(new CPos(-2048, 1024, 0), "Nope", "wooden", onDecline);
			agree = new Button(new CPos(2048, 1024, 0), "Yup", "wooden", onAgree);

			Content.Add(decline);
			Content.Add(agree);
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (key == Keys.Escape)
				onDecline();

			if (key == Keys.Enter)
				onAgree();
		}
	}
}