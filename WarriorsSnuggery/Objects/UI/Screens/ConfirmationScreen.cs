using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class ConfirmationScreen : Screen
	{
		readonly TextLine text;

		Action onDecline;
		Action onAgree;
		Button decline;
		Button agree;

		public ConfirmationScreen() : base("Are you sure?")
		{
			Title.Position = new CPos(0, -2048, 0);

			text = new TextLine(CPos.Zero, FontManager.Pixel16, TextLine.OffsetType.MIDDLE);
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

		public override void Tick()
		{
			base.Tick();
			if (KeyInput.IsKeyDown("escape", 5))
				onDecline();

			if (KeyInput.IsKeyDown("enter", 5))
				onAgree();
		}
	}
}