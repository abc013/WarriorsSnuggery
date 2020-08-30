using OpenToolkit.Windowing.Common.Input;
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class DefaultScreen : Screen
	{
		readonly Game game;
		readonly UITextLine health;
		readonly UITextLine mana;

		readonly MoneyDisplay money;
		readonly UITextLine waveText;
		readonly Panel background;
		readonly ActorList actorList;
		readonly List<ActorType> actorTypes = new List<ActorType>();
		readonly SpellList spellList;

		readonly BatchObject enemyArrow;
		Actor targetedEnemy;
		float healthPercentage;
		float manaPercentage;

		public DefaultScreen(Game game) : base(string.Empty, 0)
		{
			this.game = game;

			if (game.Statistics.Level == game.Statistics.FinalLevel)
				Title.SetColor(Color.Blue);
			else if (game.Statistics.Level > game.Statistics.FinalLevel)
				Title.SetColor(Color.Green);

			// SECTION ACTORS
			actorList = new ActorList(new CPos((int)(WindowInfo.UnitWidth * 512) - 512, -1536, 0), new MPos(512, 5 * 1024), new MPos(512, 512), PanelManager.Get("wooden"));

			foreach (var a in ActorCreator.Types.Values)
			{
				if (a.Playable == null)
					continue;

				actorTypes.Add(a);
				var sprite = a.GetPreviewSprite();
				var scale = (sprite.Width > sprite.Height ? 24f / sprite.Width : 24f / sprite.Height) - 0.1f;
				var item = new PanelItem(new BatchObject(sprite, Color.White), new MPos(512, 512), a.Playable.Name, new[] { Color.Grey + "Cost: " + Color.Yellow + a.Playable.Cost }, () => { changePlayer(a); })
				{
					Scale = scale
				};
				if (!game.Statistics.ActorAvailable(a.Playable))
					item.SetColor(Color.Black);

				actorList.Add(item);
			}

			// SECTION EFFECTS
			spellList = new SpellList(new CPos(0, (int)(WindowInfo.UnitHeight * 512) - 3072 - 128, 0), new MPos(8 * 1024, 512), new MPos(512, 512), PanelManager.Get("stone"));
			int index = 0;
			foreach (var effect in Spells.SpellTreeLoader.SpellTree)
			{
				var item = new SpellListItem(new MPos(256, 256), effect, game.SpellManager.spellCasters[index], game, true);

				spellList.Add(item);
				index++;
			}

			background = new Panel(new CPos(0, (int)(WindowInfo.UnitHeight * 512) - 1024, 0), new MPos(16 * 1024, 2 * 1024), PanelManager.Get("wooden"));

			// SECTION MONEY
			money = new MoneyDisplay(game, new CPos(6120 + 128 + 1536, 8192 - 1024, 0));
			// SECTION MENUS
			var pause = new UITextLine(new CPos(-2560, 8192 - 256, 0), FontManager.Pixel16, TextOffset.MIDDLE);
			pause.WriteText("Pause: '" + new Color(0.5f, 0.5f, 1f) + "P" + Color.White + "'");
			Content.Add(pause);

			var menu = new UITextLine(new CPos(2560, 8192 - 256, 0), FontManager.Pixel16, TextOffset.MIDDLE);
			menu.WriteText("Menu: '" + new Color(0.5f, 0.5f, 1f) + "Escape" + Color.White + "'");
			Content.Add(pause);

			// SECTION HEALTH
			health = new UITextLine(new CPos(0, 8192 - 2048, 0), FontManager.Papyrus24, TextOffset.MIDDLE);

			// SECTION MANA
			mana = new UITextLine(new CPos(0, 8192 - 1024, 0), FontManager.Papyrus24, TextOffset.MIDDLE);

			// SECTION MISSION
			var missionText = new UITextLine(new CPos(0, -8192 + 512, 0), FontManager.Pixel16, TextOffset.MIDDLE);
			var missionContent = "No mission.";
			switch (game.Mode)
			{
				case GameMode.TUTORIAL:
					missionContent = "Step on blue pads and get to the exit!";
					break;
				case GameMode.FIND_EXIT:
					missionContent = "Search for the exit and gain access to it!";
					break;
				case GameMode.WAVES:
					missionContent = "Defend your position from incoming waves!";
					break;
				case GameMode.KILL_ENEMIES:
					missionContent = "Wipe out all enemies on the map!";
					break;
			}
			missionText.SetText(missionContent);
			Content.Add(missionText);

			var levelText = new UITextLine(new CPos((int)-(WindowInfo.UnitWidth * 512) + 776, 8192 - 2048, 0), FontManager.Pixel16);
			levelText.SetText("Level " + game.Statistics.Level + "/" + game.Statistics.FinalLevel);
			Content.Add(levelText);
			waveText = new UITextLine(new CPos((int)-(WindowInfo.UnitWidth * 512) + 776, 8192 - 1536, 0), FontManager.Pixel16);

			enemyArrow = new BatchObject(UITextureManager.Get("UI_enemy_arrow")[0], Color.White);
		}

		public void SetWave(int wave, int final)
		{
			if (wave == final)
				waveText.Color = Color.Green;
			waveText.SetText("Wave " + wave + "/" + final);
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

		void selectNewEnemy()
		{
			targetedEnemy = game.World.ActorLayer.NonNeutralActors.Find(a => a.Team != Actor.PlayerTeam && a.WorldPart != null && a.WorldPart.KillForVictory);
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

		public override void Render()
		{
			// SECTION BASE
			background.Render();

			// SECTION MONEY
			if (!Settings.EnableInfoScreen)
			{
				ColorManager.DrawRect(new CPos(6120 + 128, 8192, 0), new CPos((int)(WindowInfo.UnitWidth * 512) - 2048, 8192 - 2560, 0), new Color(0, 0, 0, 128));
				money.Render();
			}

			ColorManager.DrawRect(new CPos(-6120, 8192 - 2560, 0), new CPos(6120, 8192, 0), new Color(0, 0, 0, 128));

			const int edge = 64;
			// SECTION HEALTH
			ColorManager.DrawRect(new CPos(-6120 + edge, 8192 - 1536 - edge, 0), new CPos(6120 - edge, 8192 - 2560 + edge, 0), new Color(255, 0, 0, 64));
			ColorManager.DrawRect(new CPos(-6120 + edge, 8192 - 1536 - edge, 0), new CPos(-6120 + edge + (int)((12288 - 2 * edge) * healthPercentage), 8192 - 2560 + edge, 0), new Color(255, 0, 0, 196));
			health.Render();

			// SECTION MANA
			ColorManager.DrawRect(new CPos(-6120 + edge, 8192 - 512 - edge, 0), new CPos(6120 - edge, 8192 - 1024 - 512 + edge, 0), new Color(0, 0, 255, 64));
			ColorManager.DrawRect(new CPos(-6120 + edge, 8192 - 512 - edge, 0), new CPos(-6120 + edge + (int)((12288 - 2 * edge) * manaPercentage), 8192 - 1024 - 512 + edge, 0), new Color(0, 0, 255, 196));
			mana.Render();

			// SECTION ACTORS
			actorList.Render();

			// SECTION EFFECTS
			spellList.Render();

			// SECTION MISSION
			ColorManager.DrawRect(new CPos((int)-(WindowInfo.UnitWidth * 512) + 256, 8192, 0), new CPos(-6120 - 128, 8192 - 2560, 0), new Color(0, 0, 0, 128));
			waveText.Render();

			if (targetedEnemy != null)
				enemyArrow.PushToBatchRenderer();

			base.Render();
		}

		public override void Tick()
		{
			base.Tick();

			var player = game.World.LocalPlayer;
			if (player != null)
			{
				if (KeyInput.IsKeyDown(Key.ShiftLeft))
				{
					actorList.CurrentActor += MouseInput.WheelState;

					if (!KeyInput.IsKeyDown(Key.ControlLeft) && MouseInput.IsRightClicked)
						changePlayer(actorTypes[actorList.CurrentActor]);
				}
				else
				{
					spellList.CurrentSpell += MouseInput.WheelState;

					if (!KeyInput.IsKeyDown(Key.ControlLeft) && MouseInput.IsRightClicked)
						game.SpellManager.Activate(spellList.CurrentSpell);
				}

				if (player.Health != null)
				{
					var max = player.Health.MaxHP;
					var cur = player.Health.HP;

					health.SetText(cur + "/" + max);
					healthPercentage = player.Health.RelativeHP;
				}
				if (game.Type != GameType.NORMAL || game.World.PlayerDamagedTick < Settings.UpdatesPerSecond * 60)
					targetedEnemy = null;
				else if (targetedEnemy != null && targetedEnemy.IsAlive)
					setEnemyArrow();
				else
					selectNewEnemy();

				mana.SetText(game.Statistics.Mana + "/" + game.Statistics.MaxMana);
				manaPercentage = game.Statistics.Mana / (float)game.Statistics.MaxMana;
			}

			money.Tick();

			actorList.Tick();
			spellList.Tick();
		}

		void setEnemyArrow()
		{
			var pos = Camera.LookAt + new CPos(0, -2048, 0) - targetedEnemy.GraphicPosition;

			enemyArrow.Visible = pos.SquaredFlatDist > 8192 * 8192;
			if (!enemyArrow.Visible)
				return;

			var angle = pos.FlatAngle;
			enemyArrow.SetRotation(new VAngle(0, 0, -angle) + new VAngle(0, 0, 270));
			enemyArrow.SetPosition(new CPos((int)(Math.Cos(angle) * 2048), (int)(Math.Sin(angle) * 2048) - 2048, 0));
		}

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
