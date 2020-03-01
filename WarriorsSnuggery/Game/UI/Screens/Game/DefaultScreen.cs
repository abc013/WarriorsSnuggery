using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class DefaultScreen : Screen
	{
		readonly Game game;
		readonly TextLine health;
		readonly TextLine mana;

		readonly BatchObject money;
		readonly TextLine moneyText;
		readonly TextLine menu, pause;
		readonly TextLine waveText;
		readonly Panel background;
		readonly ActorList actorList;
		readonly List<ActorType> actorTypes = new List<ActorType>();
		readonly SpellList spellList;
		int cashCooldown;
		int lastCash;
		float healthPercentage;
		float manaPercentage;

		public DefaultScreen(Game game) : base("", 0)
		{
			Speed = int.MaxValue;
			this.game = game;

			if (game.Statistics.Level == game.Statistics.FinalLevel)
				Title.SetColor(Color.Blue);
			else if (game.Statistics.Level > game.Statistics.FinalLevel)
				Title.SetColor(Color.Green);

			// SECTION ACTORS
			actorList = new ActorList(new CPos((int)(WindowInfo.UnitWidth * 512) - 512, -1536, 0), new MPos(512, 5120), new MPos(512, 512), PanelManager.Get("wooden"));

			foreach (var n in ActorCreator.GetNames())
			{
				var a = ActorCreator.GetType(n);
				if (a.Playable != null)
				{
					actorTypes.Add(a);
					var sprite = a.GetPreviewSprite();
					var scale = (sprite.Width > sprite.Height ? 24f / sprite.Width : 24f / sprite.Height) - 0.1f;
					actorList.Add(new PanelItem(CPos.Zero, new BatchObject(sprite, Color.White), new MPos(512, 512), a.Playable.Name, new[] { Color.Grey + "Cost: " + Color.Yellow + a.Playable.Cost, Color.Grey + a.Playable.Description }, () => { changePlayer(game.World.LocalPlayer, a); })
					{
						Scale = scale
					});
				}
			}

			// SECTION EFFECTS
			spellList = new SpellList(new CPos(0, (int)(WindowInfo.UnitHeight * 512) - 3072 - 128, 0), new MPos(8192, 512), new MPos(512, 512), PanelManager.Get("stone"));
			int index = 0;
			foreach (var effect in Spells.SpellTreeLoader.SpellTree)
			{
				var item = new SpellListItem(CPos.Zero, new MPos(256, 256), effect, game.SpellManager.spellCasters[index], game, true);

				spellList.Add(item);
				index++;
			}

			background = new Panel(new CPos(0, (int)(WindowInfo.UnitHeight * 512) - 1024, 0), new Vector(16, 2, 0), PanelManager.Get("wooden"));

			// SECTION MONEY
			money = new BatchObject(UITextureManager.Get("UI_money")[0], Color.White);
			money.SetPosition(new CPos(6120 + 128 + 1536, 8192 - 1024, 0));
			moneyText = new TextLine(new CPos(6120 + 256 + 2560, 8192 - 1024, 0), Font.Papyrus24);
			moneyText.SetText(game.Statistics.Money);

			// SECTION MENUS
			pause = new TextLine(new CPos(-2560, 8192 - 256, 0), Font.Pixel16, TextLine.OffsetType.MIDDLE);
			pause.WriteText("Pause: '" + new Color(0.5f, 0.5f, 1f) + "P" + Color.White + "'");

			menu = new TextLine(new CPos(2560, 8192 - 256, 0), Font.Pixel16, TextLine.OffsetType.MIDDLE);
			menu.WriteText("Menu: '" + new Color(0.5f, 0.5f, 1f) + "Escape" + Color.White + "'");

			// SECTION HEALTH
			health = new TextLine(new CPos(0, 8192 - 2048, 0), Font.Papyrus24, TextLine.OffsetType.MIDDLE);

			// SECTION MANA
			mana = new TextLine(new CPos(0, 8192 - 1024, 0), Font.Papyrus24, TextLine.OffsetType.MIDDLE);

			// SECTION MISSION
			var missionText = new TextLine(new CPos(0, -8192 + 512, 0), Font.Pixel16, TextLine.OffsetType.MIDDLE);
			switch (game.Mode)
			{
				case GameMode.NONE:
					missionText.SetText("No mission.");
					break;
				case GameMode.TUTORIAL:
					missionText.SetText("Follow the blue panels!");
					break;
				case GameMode.FIND_EXIT:
					missionText.SetText("Search for the exit!");
					break;
				case GameMode.WAVES:
					missionText.SetText("Defend your position!");
					break;
				case GameMode.KILL_ENEMIES:
					missionText.SetText("Wipe out all enemies!");
					break;
			}
			Content.Add(missionText);

			var levelText = new TextLine(new CPos((int)-(WindowInfo.UnitWidth * 512) + 776, 8192 - 2048, 0), Font.Pixel16);
			levelText.SetText("Level " + game.Statistics.Level + "/" + game.Statistics.FinalLevel);
			Content.Add(levelText);
			waveText = new TextLine(new CPos((int)-(WindowInfo.UnitWidth * 512) + 776, 8192 - 1536, 0), Font.Pixel16);
			if (game.Mode == GameMode.WAVES)
				waveText.SetText("Wave 1");
		}

		public void SetWave(int wave, int final)
		{
			if (wave == final)
				waveText.SetColor(Color.Green);
			waveText.SetText("Wave " + wave + "/" + final);
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
				money.PushToBatchRenderer();
				moneyText.Render();
			}

			ColorManager.DrawRect(new CPos(-6120, 8192 - 2560, 0), new CPos(6120, 8192, 0), new Color(0, 0, 0, 128));

			// SECTION MENUS
			menu.Render();
			pause.Render();

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

			base.Render();
		}

		public override void Tick()
		{
			base.Tick();

			var player = game.World.LocalPlayer;
			if (player != null)
			{
				if (player.Health != null)
				{
					var max = player.Health.MaxHP;
					var cur = player.Health.HP;

					health.SetText(cur + "/" + max);
					healthPercentage = (cur / (float)max);
				}

				mana.SetText(game.Statistics.Mana + "/" + game.Statistics.MaxMana);
				manaPercentage = game.Statistics.Mana / (float)game.Statistics.MaxMana;

				if (KeyInput.IsKeyDown("shiftleft"))
				{
					actorList.CurrentActor += MouseInput.WheelState;

					if (!KeyInput.IsKeyDown("controlleft") && MouseInput.IsRightClicked)
						changePlayer(game.World.LocalPlayer, actorTypes[actorList.CurrentActor]);
				}
				else
				{
					spellList.CurrentSpell += MouseInput.WheelState;

					if (!KeyInput.IsKeyDown("controlleft") && MouseInput.IsRightClicked)
						game.SpellManager.Activate(spellList.CurrentSpell);
				}
			}

			if (lastCash != game.Statistics.Money)
			{
				lastCash = game.Statistics.Money;
				moneyText.SetText(game.Statistics.Money);
				cashCooldown = 10;
			}
			if (cashCooldown-- > 0)
				moneyText.Scale = (cashCooldown / 10f) + 1f;

			actorList.Tick();
			spellList.Tick();
		}

		void changePlayer(Actor player, ActorType type)
		{
			if (game.Statistics.Money < type.Playable.Cost)
				return;

			if (game.World.LocalPlayer.Type == type)
				return;

			game.Statistics.Money -= type.Playable.Cost;

			var oldHP = player.Health != null ? player.Health.HPRelativeToMax : 1;
			var newActor = ActorCreator.Create(game.World, type, player.Position, player.Team, isPlayer: true);

			player.Dispose();
			game.World.LocalPlayer = newActor;
			game.World.Add(newActor);

			if (newActor.Health != null)
				newActor.Health.HP = (int)(oldHP * newActor.Health.MaxHP);

			game.Statistics.Actor = ActorCreator.GetName(type);
			VisibilitySolver.ShroudUpdated();
		}
	}
}
