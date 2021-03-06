﻿using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class ChatBox : UIObject
	{
		readonly TextPanel panel;
		readonly TextBox input;
		readonly Button send;

		public bool MouseOnChat => ContainsMouse;

		public ChatBox(CPos position)
		{
			panel = new TextPanel(position, new MPos(8120, 2048), FontManager.Pixel16, PanelManager.Get("stone"));

			input = new TextBox(position + new CPos(-800, 2048 + 512, 0), "", "wooden", 45);
			input.OnEnter = SendText;
			send = new Button(position + new CPos(8120 - 800, 2048 + 512, 0), "send", "wooden", SendText);

			Position += new CPos(0, 5632, 0);
			Bounds = new MPos(8120 + 512, 4096);
			SelectableBounds = Bounds;
		}

		public void OpenChat()
		{
			input.Selected = true;
		}

		public void CloseChat()
		{
			input.Text = string.Empty;
		}

		public void SendText()
		{
			input.Selected = true;
			if (string.IsNullOrWhiteSpace(input.Text))
				return;

			panel.Add(input.Text);
			input.Text = string.Empty;
		}

		public void SendText(string message)
		{
			panel.Add(message);
		}

		public override void Tick()
		{
			CheckMouse();
			panel.Tick();
			input.Tick();
			send.Tick();
		}

		public override void Render()
		{
			ColorManager.DrawRect(new CPos(Position.X - 8120 - 512, Position.Y + 2560 - 512, 0), new CPos(Position.X + 8120 + 512, Position.Y - 4606 + 512, 0), new Color(0, 0, 0, 0.25f));
			panel.Render();
			input.Render();
			send.Render();
		}
	}
}
