using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class DefaultScreen : Screen
	{
		const int margin = 256;

		readonly Game game;

		readonly UIParticleManager particleManager;

		readonly DisplayBar healthBar;
		readonly DisplayBar manaBar;

		readonly ActorList actorList;
		readonly SpellList spellList;

		readonly EnemyPointer pointer;

		int particleCollector;
		int winParticleCount = 5;

		public DefaultScreen(Game game) : base(string.Empty, 0)
		{
			this.game = game;

			particleManager = new UIParticleManager();
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
				Add(new WaveDisplay(game) { Position = new CPos(Left + 1280 + shift, top + 1536 + shift + 128, 0) });

			var menu = new CheckBox("menu", onTicked: (t) => game.ShowScreen(ScreenType.MENU, true))
			{
				Position = new CPos(Right - 512 - margin, Top + 512 + margin, 0),
				Scale = 2.5f
			};
			Add(menu);

			Add(new MissionTextLine(game.ObjectiveType));

			pointer = new EnemyPointer(game);
			Add(pointer);
		}

		public void UpdateSpells() => spellList.Update();
		public void UpdateActors() => actorList.Update();

		public void HideArrow() => pointer.HideArrow();
		public void ShowArrow() => pointer.ShowArrow();

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

			if (game.WinConditionsMet)
			{
				if (game.LocalTick % 30 == 0 && winParticleCount-- > 0)
				{
					for (int i = 0; i < 5; i++)
						generateWinParticles();
				}
			}

			var player = game.World.LocalPlayer;
			if (player.Health != null)
			{
				var percentage = player.Health.RelativeHP;

				healthBar.WriteText($"{player.Health.HP}/{player.Health.MaxHP}");
				healthBar.DisplayPercentage = percentage;

				if (percentage < 0.3f)
					generateBloodParticles(percentage);
			}

			manaBar.WriteText($"{game.Stats.Mana}/{game.Stats.MaxMana}");
			manaBar.DisplayPercentage = game.Stats.Mana / (float)game.Stats.MaxMana;
		}

		void generateWinParticles()
		{
			const int count = 50;

			var random = Program.SharedRandom;

			var width = (Width / 2);
			if (random.Next(2) > 0)
				width *= -1;

			var randomX = random.Next(200, 400) * Math.Sign(-width);

			for (int i = 0; i < count; i++)
			{
				var particle = new UIParticle(random.Next(200, 400))
				{
					Radius = random.Next(200, 500),
					Position = new CPos(width, random.Next(-500, 500), 0),
					Velocity = new CPos(random.Next(-50, 50) + randomX, -random.Next(150) - 250, 0),
					Force = new CPos(0, random.Next(5, 15), 0)
				};

				static int invert(bool invert, int color) => invert ? 255 - color : color;

				var color = random.Next(255);
				var inverse = random.Next(2) > 0;
				particle.Color = new Color(invert(inverse, color), invert(!inverse, color), invert(random.Next(2) > 0, color), 255);
			}
		}

		void generateBloodParticles(float percentage)
		{
			var random = Program.SharedRandom;

			var inverse = 0.3f - percentage;
			particleCollector += (int)(inverse * 50) + 1;

			var count = particleCollector / 16;
			particleCollector -= count * 16;

			for (int i = 0; i < count * 2; i++)
			{
				var invert = i % 2 == 0 ? -1 : 1;
				var particle = new UIParticle((int)(percentage * 200) + 300)
				{
					Radius = random.Next(10, 160) + (int)(inverse * inverse * 2000),
					Position = new CPos(random.Next(Width) - Width / 2, invert * Bottom, 0),
					Velocity = new CPos(random.Next(-2, 2), invert * random.Next(10, 20), 0),
					Color = new Color(random.Next(128, 192), 0, 0, 192)
				};

				particleManager.Add(particle);
			}
		}

		class MissionTextLine : UITextLine
		{
			const int start = 240;
			const int duration = 120;
			int tick;

			public MissionTextLine(ObjectiveType type) : base(FontManager.Header, TextOffset.MIDDLE)
			{
				switch (type)
				{
					case ObjectiveType.FIND_EXIT:
						SetText("Search for the exit and gain access to it!");
						break;
					case ObjectiveType.KILL_ENEMIES:
						SetText("Wipe out all enemies on the map!");
						break;
					case ObjectiveType.SURVIVE_WAVES:
						SetText("Defend your position from incoming waves!");
						break;
				}
			}

			public override void Render()
			{
				const int rectWidth = 640;

				// TODO: move
				if (tick++ < start)
					ColorManager.DrawRect(new CPos(Right, rectWidth, 0), new CPos(Left, -rectWidth, 0), new Color(0, 0, 0, 128));
				else if (tick < start + duration)
				{
					var top = Top + 512 + margin;
					var linearTime = (((tick - start) / (float)duration) - 0.5f) * 2f;
					var squaredTime = -0.25f * (linearTime * linearTime * linearTime) + 0.75f * linearTime + 0.5f;
					Position = new CPos(0, (int)(top * squaredTime), 0);
					ColorManager.DrawRect(new CPos(Right, rectWidth, 0) + Position, new CPos(Left, -rectWidth, 0) + Position, new Color(0, 0, 0, (int)(128 * (1f - (linearTime + 1f) / 2f))));
				}

				base.Render();
			}
		}
	}
}
