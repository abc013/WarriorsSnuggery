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

	public class MessageBox : UIObject
	{
		readonly Panel background;
		readonly UIText text;

		Button decline, agree;
		Button @continue;

		public bool MouseOnBox => ContainsMouse;
		public bool Visible;

		public MessageBox(CPos position)
		{
			Position = position;
			background = new Panel(new MPos(8120, 1024), "wooden") { Position = position };
			text = new UIText(FontManager.Default, TextOffset.LEFT) { Position = position - new CPos(8120 - FontManager.Default.WidthGap, 1024 - FontManager.Default.HeightGap, 0) };

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
				agree = new Button("Agree", "wooden", () => closeAfterAction(message.OnAgree)) { Position = Position + new CPos(4096, 1024 + FontManager.Default.MaxHeight, 0) };
				decline = new Button("Decline", "wooden", () => closeAfterAction(message.OnDecline)) { Position = Position + new CPos(1024, 1024 + FontManager.Default.MaxHeight, 0) };
			}
			else
			{
				agree = null;
				decline = null;
				@continue = new Button("Continue", "wooden", () => closeAfterAction()) { Position = Position + new CPos(3072, 1024 + FontManager.Default.MaxHeight, 0) };
			}
		}

		public override void Tick()
		{
			if (!Visible)
				return;

			CheckMouse();

			decline?.Tick();
			agree?.Tick();
			@continue?.Tick();
		}

		public override void Render()
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
