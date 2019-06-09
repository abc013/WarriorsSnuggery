using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class ConfirmationScreen : Screen
	{
		readonly Game game;
		readonly TextLine text;

		Action onDecline;
		Action onAgree;
		Button decline;
		Button agree;

		public ConfirmationScreen(Game game) : base("Are you sure?")
		{
			this.game = game;
			Title.Position = new CPos(0, -2048, 0);

			text = new TextLine(CPos.Zero, Graphics.IFont.Pixel16, TextLine.OffsetType.MIDDLE);
		}

		public void SetAction(Action onDecline, Action onAgree, string text)
		{
			this.text.SetText(text);

			this.onDecline = onDecline;
			this.onAgree = onAgree;

			if (decline != null)
			{
				decline.Dispose();
				agree.Dispose();
			}
			decline = ButtonCreator.Create("wooden", new CPos(-2048, 1024, 0), "Nope", onDecline);
			agree = ButtonCreator.Create("wooden", new CPos(2048, 1024, 0), "Yup", onAgree);
		}

		public override void Tick()
		{
			base.Tick();
			if (KeyInput.IsKeyDown("escape", 5))
			{
				onDecline();
			}
			if (KeyInput.IsKeyDown("enter", 5))
			{
				onAgree();
			}

			decline.Tick();
			agree.Tick();
		}

		public override void Render()
		{
			base.Render();

			text.Render();
			decline.Render();
			agree.Render();
		}

		public override void Dispose()
		{
			text.Dispose();
			if (decline != null)
			{
				decline.Dispose();
				agree.Dispose();
			}
			base.Dispose();
		}
	}
}