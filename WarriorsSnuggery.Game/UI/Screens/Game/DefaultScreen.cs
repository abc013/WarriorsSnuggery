using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class DefaultScreen : Screen
	{
		readonly Game game;

		readonly SquareParticleManager particleManager;

		readonly DisplayBar healthBar;
		readonly DisplayBar manaBar;

		readonly ActorList actorList;
		readonly List<ActorType> actorTypes = new List<ActorType>();
		readonly SpellList spellList;

		int particleCollector;

		public DefaultScreen(Game game) : base(string.Empty, 0)
		{
			this.game = game;

			particleManager = new SquareParticleManager();
			Content.Add(particleManager);

			const int shift = 256;
			var right = (int)(WindowInfo.UnitWidth * 512);
			var left = -(int)(WindowInfo.UnitWidth * 512);

			// Actors
			actorList = new ActorList(new CPos(left + 768, 768 + shift, 0), new MPos(512, 11 * 512), new MPos(512, 512), PanelManager.Get("wooden"));
			foreach (var a in ActorCreator.Types.Values)
			{
				if (a.Playable == null)
					continue;

				actorTypes.Add(a);
				var sprite = a.GetPreviewSprite();
				var scale = 24f / Math.Max(sprite.Width, sprite.Height) - 0.1f;
				var item = new PanelItem(new BatchObject(sprite, Color.White), new MPos(512, 512), a.Playable.Name, new[] { Color.Grey + "Cost: " + Color.Yellow + a.Playable.Cost }, () => { changePlayer(a); })
				{
					Scale = scale
				};
				if (!game.Statistics.ActorAvailable(a.Playable))
					item.SetColor(Color.Black);

				actorList.Add(item);
			}
			actorList.CurrentActor = 0;

			Content.Add(actorList);

			// Spells
			spellList = new SpellList(new CPos(right - 768, 0, 0), new MPos(512, 13 * 512), new MPos(512, 512), PanelManager.Get("stone"));
			int index = 0;
			foreach (var spell in Spells.SpellTreeLoader.SpellTree)
			{
				var item = new SpellListItem(new MPos(512, 512), spell, game.SpellManager.spellCasters[index++], game, true);
				spellList.Add(item);
			}
			spellList.CurrentSpell = 0;

			Content.Add(spellList);

			var width = (int)(WindowInfo.UnitWidth * 512);

			manaBar = new DisplayBar(new CPos(0, 8192 - 2048 + shift, 0), new MPos(width - 1536, 256), PanelManager.Get("stone"), new Color(0, 0, 255, 196));
			Content.Add(manaBar);
			healthBar = new DisplayBar(new CPos(0, 8192 - 1024 + shift, 0), new MPos(width - 256, 512), PanelManager.Get("wooden"), new Color(255, 0, 0, 196));
			Content.Add(healthBar);

			var top = -8120 + 512 + shift;

			Content.Add(new MoneyDisplay(game, new CPos(left + 1536 + shift, top, 0)));

			if (game.ObjectiveType == ObjectiveType.FIND_EXIT)
				Content.Add(new KeyDisplay(game, new CPos(left + 712 + shift, top + 1536 + shift + 128, 0)));
			else if (game.ObjectiveType == ObjectiveType.SURVIVE_WAVES)
				Content.Add(new WaveDisplay(game, new CPos(left + 512 + shift, top + 1536 + shift + 128, 0)));

			Content.Add(CheckBoxCreator.Create("menu", new CPos(right - 512, -8120 + 512, 0), onTicked: (t) => game.ScreenControl.ShowScreen(ScreenType.MENU)));

			// mission text
			var missionText = new UITextLine(new CPos(0, top, 0), FontManager.Pixel16, TextOffset.MIDDLE);
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

			if (game.Statistics.Level == game.Statistics.FinalLevel)
				missionText.SetColor(Color.Blue);
			else if (game.Statistics.Level > game.Statistics.FinalLevel)
				missionText.SetColor(Color.Green);

			Content.Add(missionText);

			Content.Add(new EnemyPointer(game));
		}

		public void UpdateSpells()
		{
			spellList.Update();
		}

		public void UpdateActors()
		{
			for (int i = 0; i < actorTypes.Count; i++)
				actorList.Container[i].SetColor(game.Statistics.ActorAvailable(actorTypes[i].Playable) ? Color.White : Color.Black);
		}

		public override void Hide()
		{
			spellList.DisableTooltip();
			actorList.DisableTooltip();
		}

		public override bool CursorOnUI()
		{
			var mouse = MouseInput.WindowPosition;

			// Info Panel
			if (mouse.Y > 4992)
				return true;
			// Effects Panel
			if (Math.Abs(mouse.X) < 8120 && mouse.Y > 4992 - 64 - 256)
				return true;
			// Actor Panel
			if (mouse.X > WindowInfo.Ratio * 6.9f * 1024)
				return true;

			return false;
		}

		public override void Tick()
		{
			base.Tick();

			var player = game.World.LocalPlayer;
			if (player != null)
			{
				// TODO: Why arent the spellLists doing that?
				if (KeyInput.IsKeyDown(Keys.LeftShift))
				{
					actorList.CurrentActor += MouseInput.WheelState;

					if (!KeyInput.IsKeyDown(Keys.LeftControl) && MouseInput.IsRightClicked)
						changePlayer(actorTypes[actorList.CurrentActor]);
				}
				else
				{
					spellList.CurrentSpell += MouseInput.WheelState;

					if (!KeyInput.IsKeyDown(Keys.LeftControl) && MouseInput.IsRightClicked)
						game.SpellManager.Activate(spellList.CurrentSpell);
				}

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
							particle.Position = new CPos(Program.SharedRandom.Next((int)(WindowInfo.UnitWidth * 1024)) - (int)(WindowInfo.UnitWidth * 512), (invert ? 1 : -1) * (int)(WindowInfo.UnitHeight * 512), 0);
							particle.Velocity = new CPos(Program.SharedRandom.Next(-2, 2), (invert ? -1 : 1) * (Program.SharedRandom.Next(10) + 10), 0);
							particle.Color = new Color(Program.SharedRandom.Next(96) + 127, 0, 0, 192);
						}
					}
				}

				manaBar.WriteText($"{game.Statistics.Mana}/{game.Statistics.MaxMana}");
				manaBar.DisplayPercentage = game.Statistics.Mana / (float)game.Statistics.MaxMana;
			}
		}

		// TODO: this code has nothing to do here, it should be somewhere else
		void changePlayer(ActorType type)
		{
			if (game.Statistics.Money < type.Playable.Cost)
				return;

			if (game.World.LocalPlayer.Type == type)
				return;

			if (!game.Statistics.ActorAvailable(type.Playable))
				return;

			game.Statistics.Money -= type.Playable.Cost;

			game.World.BeginPlayerSwitch(type);
		}
	}
}
