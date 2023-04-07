using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objectives;
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
			actorList = new ActorList(game, new UIPos(512, 11 * 512), new UIPos(512, 512), "wooden") { Position = new UIPos(Left + 512 + margin, 768 + shift) };
			Add(actorList);

			// Spells
			spellList = new SpellList(game, new UIPos(512, 13 * 512), new UIPos(512, 512), "stone") { Position = new UIPos(Right - 512 - margin, 0) };
			Add(spellList);

			manaBar = new DisplayBar(new UIPos(Width / 2 - 1536, 256), PanelCache.Types["stone"], new Color(0, 0, 255, 196)) { Position = new UIPos(0, Bottom - 2048 + margin) };
			Add(manaBar);
			healthBar = new DisplayBar(new UIPos(Width / 2 - 256, 512), PanelCache.Types["wooden"], new Color(255, 0, 0, 196)) { Position = new UIPos(0, Bottom - 1024 + margin) };
			Add(healthBar);

			var top = Top + 512 + margin;

			Add(new MoneyDisplay(game) { Position = new UIPos(Left + 1536 + shift, top) });
			Add(new HealthDisplay(game) { Position = new UIPos(Left + 4096 + shift + margin, top) });

			if (game.ObjectiveType == ObjectiveType.FIND_EXIT)
				Add(new KeyDisplay(game) { Position = new UIPos(Left + 712 + shift, top + 1536 + shift + 128) });
			else if (game.ObjectiveType == ObjectiveType.SURVIVE_WAVES)
				Add(new WaveDisplay((WaveObjectiveController)game.ObjectiveController) { Position = new UIPos(Left + 1280 + shift, top + 1536 + shift + 128) });

			var menu = new CheckBox("menu", onTicked: (t) => game.ShowScreen(ScreenType.MENU, true))
			{
				Position = new UIPos(Right - 512 - margin, Top + 512 + margin),
				Scale = 2.5f
			};
			Add(menu);

			if (game.MissionType.IsCampaign())
				Add(new MissionTextLine(game));

			const int pointerMargin = margin + 512;
			pointer = new EnemyPointer(game, new UIPos(Right - 1024 - pointerMargin, Bottom - 1024 - 512 - pointerMargin)) { Position = new UIPos(0, -512) };
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
				if (game.LocalTick % 20 == 0 && winParticleCount-- > 0)
				{
					for (int i = 0; i < 5; i++)
						generateWinParticles();
				}
			}

			var player = game.World.LocalPlayer;
			if (player.Health != null)
			{
				var percentage = player.Health.RelativeHP;

				healthBar.SetText($"{player.Health.HP}/{player.Health.MaxHP}");
				healthBar.DisplayPercentage = percentage;

				if (percentage < 0.3f)
					generateBloodParticles(percentage);
			}

			manaBar.SetText($"{game.Player.Mana}/{game.Player.MaxMana}");
			if (game.Player.MaxMana != 0)
				manaBar.DisplayPercentage = game.Player.Mana / (float)game.Player.MaxMana;
			else
				manaBar.DisplayPercentage = 0;
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
				var particle = new UIParticle(random.Next(150, 300))
				{
					Radius = random.Next(10, 160),
					Position = new UIPos(width, random.Next(-500, 500)),
					Velocity = new UIPos(random.Next(-50, 50) + randomX, -random.Next(120, 320)),
					Force = new UIPos(0, random.Next(5, 15))
				};

				static int invert(bool invert, int color) => invert ? 255 - color : color;

				var color = random.Next(255);
				var inverse = random.Next(2) > 0;
				particle.Color = new Color(invert(inverse, color), invert(!inverse, color), invert(random.Next(2) > 0, color), 255);

				particleManager.Add(particle);
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
					Radius = random.Next(10, 160) + (int)(inverse * inverse * 3000),
					Position = new UIPos(random.Next(Width) - Width / 2, invert * Top),
					Velocity = new UIPos(random.Next(-2, 2), invert * random.Next(10, 20)),
					Color = new Color(random.Next(128, 192), 0, 0, 192)
				};

				particleManager.Add(particle);
			}
		}

		class MissionTextLine : UIText
		{
			const int start = 240;
			const int duration = 120;
			int tick;

			readonly bool empty;

			public MissionTextLine(Game game) : base(FontManager.Header, TextOffset.MIDDLE)
			{
				empty = game.ObjectiveController == null; 
				if (!empty)
					SetText(game.ObjectiveController.MissionString);
			}

			public override void Tick()
			{
				tick++;

				base.Tick();
			}

			public override void Render()
			{
				if (empty)
					return;

				const int rectWidth = 640;

				if (tick < start)
					ColorManager.DrawRect(new UIPos(Right, rectWidth), new UIPos(Left, -rectWidth), new Color(0, 0, 0, 128));
				else if (tick < start + duration)
				{
					var top = Top + 512 + margin;
					var linearTime = (((tick - start) / (float)duration) - 0.5f) * 2f;
					var squaredTime = -0.25f * (linearTime * linearTime * linearTime) + 0.75f * linearTime + 0.5f;
					Position = new UIPos(0, (int)(top * squaredTime));
					ColorManager.DrawRect(new UIPos(Right, rectWidth) + Position, new UIPos(Left, -rectWidth) + Position, new Color(0, 0, 0, (int)(128 * (1f - (linearTime + 1f) / 2f))));
				}

				base.Render();
			}
		}
	}
}
