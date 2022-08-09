using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class Message
	{
		public enum MessageType
		{
			INFORMATION,
			DECISION
		}

		public readonly MessageType Type;
		public readonly Action OnContinue;
		public readonly Action OnDecline;
		public readonly Action OnAgree;
		public readonly string[] Text;

		public Message(Action onContinue, params string[] text)
		{
			Type = MessageType.INFORMATION;

			OnContinue = onContinue;
			Text = text;

			if (Text.Length > 4)
				throw new ArgumentOutOfRangeException($"Message can only have max 4 lines. ({Text.Length}/4)");
		}

		public Message(Action onDecline, Action onAgree, Action onContinue, params string[] text)
		{
			Type = MessageType.DECISION;

			OnDecline = onDecline;
			OnAgree = onAgree;
			OnContinue = onContinue;
			Text = text;

			if (Text.Length > 4)
				throw new ArgumentOutOfRangeException($"Message can only have max 4 lines. ({Text.Length}/4)");
		}
	}

	public sealed class MessageBox : UIPositionable, IRenderable, ITick
	{
		readonly Panel background;
		readonly UIText text;

		Button decline, agree;
		Button @continue;

		public bool Visible;

		public MessageBox(UIPos position)
		{
			Position = position;
			background = new Panel(new UIPos(8120, 1024), "wooden") { Position = position };
			text = new UIText(FontManager.Default, TextOffset.LEFT) { Position = position - new UIPos(8120 - FontManager.Default.WidthGap, 1024 - FontManager.Default.HeightGap) };

			Bounds = background.Bounds;
			SelectableBounds = Bounds;
		}

		public void OpenMessage(Message message)
		{
			Visible = true;

			text.SetText(message.Text);

			void closeAfterAction(Action action = null)
			{
				Visible = false;
				action?.Invoke();
				message.OnContinue.Invoke();
			};

			if (message.Type == Message.MessageType.DECISION)
			{
				@continue = null;
				agree = new Button("Agree", "wooden", () => closeAfterAction(message.OnAgree)) { Position = Position + new UIPos(4096, 1024 + FontManager.Default.MaxHeight) };
				decline = new Button("Decline", "wooden", () => closeAfterAction(message.OnDecline)) { Position = Position + new UIPos(1024, 1024 + FontManager.Default.MaxHeight) };
			}
			else
			{
				agree = null;
				decline = null;
				@continue = new Button("Continue", "wooden", () => closeAfterAction()) { Position = Position + new UIPos(3072, 1024 + FontManager.Default.MaxHeight) };
			}
		}

		public void Tick()
		{
			if (!Visible)
				return;

			decline?.Tick();
			agree?.Tick();
			@continue?.Tick();
		}

		public void Render()
		{
			if (!Visible)
				return;

			background.Render();
			text.Render();

			decline?.Render();
			agree?.Render();
			@continue?.Render();
		}
	}
}
