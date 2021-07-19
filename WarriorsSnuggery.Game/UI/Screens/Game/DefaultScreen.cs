using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class DefaultScreen : Screen
	{
		const int margin = 256;

		readonly Game game;

		readonly SquareParticleManager particleManager;

		readonly DisplayBar healthBar;
		readonly DisplayBar manaBar;

		readonly ActorList actorList;
		readonly SpellList spellList;

		int particleCollector;

		public DefaultScreen(Game game) : base(string.Empty, 0)
		{
			this.game = game;

			particleManager = new SquareParticleManager();
			Add(particleManager);

			const int shift = margin;

			// Actors
			actorList = new ActorList(game, new MPos(512, 11 * 512), new MPos(512, 512), "wooden") { Position = new CPos(Left + 512 + margin, 768 + shift, 0) };
			Add(actorList);

			// Spells
			spellList = new SpellList(game, new MPos(512, 13 * 512), new MPos(512, 512), "stone") { Position = new CPos(Right - 512 - margin, 0, 0) };
			Add(spellList);

			manaBar = new DisplayBar(new MPos(Width / 2 - 1536, 256), PanelCache.Types["stone"], new Color(0, 0, 255, 196)) { Position = new CPos(0, Bottom - 2048 + margin, 0) };
			Add(manaBar);
			healthBar = new DisplayBar(new MPos(Width / 2 - 256, 512), PanelCache.Types["wooden"], new Color(255, 0, 0, 196)) { Position = new CPos(0, Bottom - 1024 + margin, 0) };
			Add(healthBar);

			var top = Top + 512 + margin;

			Add(new MoneyDisplay(game) { Position = new CPos(Left + 1536 + shift, top, 0) });
			Add(new HealthDisplay(game) { Position = new CPos(Left + 4096 + shift + margin, top, 0) });

			if (game.ObjectiveType == ObjectiveType.FIND_EXIT)
				Add(new KeyDisplay(game) { Position = new CPos(Left + 712 + shift, top + 1536 + shift + 128, 0) });
			else if (game.ObjectiveType == ObjectiveType.SURVIVE_WAVES)
				Add(new WaveDisplay(game) { Position = new CPos(Left + 512 + shift, top + 1536 + shift + 128, 0) });

			var menu = new CheckBox("menu", onTicked: (t) => game.ShowScreen(ScreenType.MENU, true))
			{
				Position = new CPos(Right - 512 - margin, Top + 512 + margin, 0),
				Scale = 2.5f
			};
			Add(menu);

			// mission text
			var missionText = new UITextLine(FontManager.Header, TextOffset.MIDDLE) { Position = new CPos(0, top, 0) };
			var missionContent = string.Empty;
			switch (game.ObjectiveType)
			{
				case ObjectiveType.FIND_EXIT:
					missionContent = "Search for the exit and gain access to it!";
					break;
				case ObjectiveType.KILL_ENEMIES:
					missionContent = "Wipe out all enemies on the map!";
					break;
				case ObjectiveType.SURVIVE_WAVES:
					missionContent = "Defend your position from incoming waves!";
					break;
			}
			missionText.SetText(missionContent);

			if (game.Save.Level == game.Save.FinalLevel)
				missionText.SetColor(Color.Blue);
			else if (game.Save.Level > game.Save.FinalLevel)
				missionText.SetColor(Color.Green);

			Add(missionText);

			Add(new EnemyPointer(game));
		}

		public void UpdateSpells()
		{
			spellList.Update();
		}

		public void UpdateActors()
		{
			actorList.Update();
		}

		public override void Hide()
		{
			spellList.DisableTooltip();
			actorList.DisableTooltip();
		}

		public override bool CursorOnUI()
		{
			var mouse = MouseInput.WindowPosition;

			// Actorlist area
			if (mouse.X < Left + 1024 + margin)
				return true;

			// Spellist area
			if (mouse.X > Right - 1024 - margin)
				return true;

			// Area around health & Mana bars
			if (mouse.Y > 6144)
				return true;

			return false;
		}

		public override void Tick()
		{
			base.Tick();

			var player = game.World.LocalPlayer;
			if (player.Health != null)
			{
				var percentage = player.Health.RelativeHP;

				healthBar.WriteText($"{player.Health.HP}/{player.Health.MaxHP}");
				healthBar.DisplayPercentage = percentage;

				if (percentage < 0.3f)
				{
					var inverse = 0.3f - percentage;
					particleCollector += (int)(inverse * 50) + 1;

					var count = particleCollector / 16;
					particleCollector -= count * 16;

					for (int i = 0; i < count * 2; i++)
					{
						var invert = i % 2 == 0;
						var particle = particleManager.Add((int)(percentage * 200) + 300);
						particle.Radius = Program.SharedRandom.Next(150) + (int)(inverse * inverse * 2000) + 10;
						particle.Position = new CPos(Program.SharedRandom.Next(Width) - Width / 2, (invert ? 1 : -1) * Bottom, 0);
						particle.Velocity = new CPos(Program.SharedRandom.Next(-2, 2), (invert ? -1 : 1) * (Program.SharedRandom.Next(10) + 10), 0);
						particle.Color = new Color(Program.SharedRandom.Next(96) + 127, 0, 0, 192);
					}
				}
			}

			manaBar.WriteText($"{game.Stats.Mana}/{game.Stats.MaxMana}");
			manaBar.DisplayPercentage = game.Stats.Mana / (float)game.Stats.MaxMana;
		}
	}
}
