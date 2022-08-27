using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Networking;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class JoinNetworkGameScreen : Screen
	{
		readonly Game game;

		readonly UIText connection;
		readonly TextBox username, address, port, password;

		public JoinNetworkGameScreen(Game game) : base("Join Server")
		{
			this.game = game;
			Title.Position = new UIPos(0, -4096);

			var join = new UIText(FontManager.Default, TextOffset.MIDDLE) { Position = new UIPos(0, -2048) };
			join.SetText("Select a username, then directly connect to a server.");
			Add(join);

			connection = new UIText(FontManager.Default, TextOffset.MIDDLE) { Position = new UIPos(0, -1024) };
			Add(connection);

			var usernameText = new UIText(FontManager.Default, TextOffset.RIGHT) { Position = new UIPos(-4096, 0) };
			usernameText.SetText("Username: ");
			Add(usernameText);

			username = new TextBox("wooden", 14)
			{
				Position = new UIPos(1024, 0),
				Text = Settings.Name
			};
			Add(username);

			var addressText = new UIText(FontManager.Default, TextOffset.RIGHT) { Position = new UIPos(-4096, 2048) };
			addressText.SetText("Address: ");
			Add(addressText);

			address = new TextBox("wooden", 16)
			{
				Position = new UIPos(0, 2048),
				Text = NetworkUtils.DefaultAddress
			};
			Add(address);

			var portText = new UIText(FontManager.Default, TextOffset.RIGHT) { Position = new UIPos(-4096, 3072) };
			portText.SetText("Port: ");
			Add(portText);

			port = new TextBox("wooden", 5, InputType.NUMBERS)
			{
				Position = new UIPos(0, 3072),
				Text = NetworkUtils.DefaultPort.ToString()
			};
			Add(port);

			var passwordText = new UIText(FontManager.Default, TextOffset.RIGHT) { Position = new UIPos(-4096, 4096) };
			passwordText.SetText("Password: ");
			Add(passwordText);

			password = new TextBox("wooden", 16)
			{
				Position = new UIPos(1024, 4096),
				EmptyText = "Leave empty if none"
			};
			Add(password);

			Add(new Button("Cancel", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)) { Position = new UIPos(-4096, 6144) });
			Add(new Button("Join", "wooden", () => tryConnect(address.Text, port.Text)) { Position = new UIPos(4096, 6144) });
		}

		void tryConnect(string address, string port)
		{
			if (!string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(port))
			{
				if (!GameController.Connect(address, int.Parse(port), password.Text))
					connection.SetText($"{Color.Red}Failed to connect. Check logs for more details.");
			}
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.DEFAULT, false);
		}
	}
}
