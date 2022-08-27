using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Networking.Orders;

namespace WarriorsSnuggery.UI.Objects
{
	public sealed class ChatBox : UIPositionable, ITick, IRenderable, ICheckKeys
	{
		readonly TextPanel panel;
		readonly TextBox input;
		readonly Button send;

		public bool Visible;

		public ChatBox(UIPos position)
		{
			panel = new TextPanel(new UIPos(8120, 2048), FontManager.Default, "stone") { Position = position };

			input = new TextBox("wooden", 45)
			{
				Position = position + new UIPos(-800, 2048 + 512),
				OnEnter = SendText,
				EmptyText = "Type here..."
			};
			send = new Button("send", "wooden", SendText) { Position = position + new UIPos(8120 - 800, 2048 + 512) };

			Position += new UIPos(0, 5632);
			Bounds = new UIPos(8120 + 512, 4096);
			SelectableBounds = Bounds;
		}

		public void OpenChat()
		{
			input.Selected = true;
			Visible = true;
		}

		public void CloseChat()
		{
			input.Text = string.Empty;
			Visible = false;
		}

		public void SendText()
		{
			input.Selected = true;
			if (string.IsNullOrWhiteSpace(input.Text))
				return;

			SendText(input.Text);
			input.Text = string.Empty;
		}

		public void SendText(string message)
		{
			GameController.SendOrder(new ChatOrder($"{Settings.Name}: {Color.White.WithAlpha(0.8f)}{message}"));
		}

		public void ReceiveText(string message)
		{
			panel.Add(message);
		}

		public void Tick()
		{
			if (!Visible)
				return;

			panel.Tick();
			input.Tick();
			send.Tick();
		}

		public void Render()
		{
			if (!Visible)
				return;

			ColorManager.DrawRect(new UIPos(Position.X - 8120 - 512, Position.Y + 2560 - 512), new UIPos(Position.X + 8120 + 512, Position.Y - 4606 + 512), new Color(0, 0, 0, 0.25f));
			panel.Render();
			input.Render();
			send.Render();
		}

		public void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (!Visible)
				return;

			input.KeyDown(key, isControl, isShift, isAlt);
		}
	}
}
