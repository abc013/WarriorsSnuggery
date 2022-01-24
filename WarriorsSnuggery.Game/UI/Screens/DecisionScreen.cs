using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class DecisionScreen : Screen
	{
		readonly UIText text;

		Action onDecline;
		Action onAgree;
		Button decline;
		Button agree;

		public DecisionScreen(Game game) : base("Are you sure?")
		{
			Title.Position = new CPos(0, -2048, 0);

			text = new UIText(FontManager.Default, TextOffset.MIDDLE);
			Add(text);
		}

		public void SetAction(Action onDecline, Action onAgree, string text)
		{
			this.text.SetText(text);

			this.onDecline = onDecline;
			this.onAgree = onAgree;

			Remove(decline);
			Remove(agree);

			decline = new Button("Nope", "wooden", onDecline) { Position = new CPos(-2048, 1024, 0) };
			agree = new Button("Yup", "wooden", onAgree) { Position = new CPos(2048, 1024, 0) };

			Add(decline);
			Add(agree);
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Keys.Escape)
				onDecline();

			if (key == Keys.Enter)
				onAgree();
		}
	}
}