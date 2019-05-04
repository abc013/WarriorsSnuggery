using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class DefaultScreen : Screen
	{
		readonly Game game;
		readonly TextLine health;
		readonly TextLine mana;

		readonly ImageRenderable money;
		readonly TextLine moneyText;
		readonly TextLine menu, pause;
		readonly TextLine missionText;
		readonly Panel background;
		readonly PanelList actorPanel;
		readonly PanelList effectPanel;
		readonly ActorType[] actorPanelContent;
		int cashCooldown;
		int lastCash;
		float healthPercentage;
		float manaPercentage;

		public DefaultScreen(Game game) : base("Level " + game.Statistics.Level + "/" + game.Statistics.FinalLevel, 0)
		{
			this.game = game;
			Title.Position += new CPos(0,-7120,0);
			if (game.Statistics.Level >= game.Statistics.FinalLevel)
				Title.SetColor(Color.Green);

			// SECTION ACTORS
			actorPanel = new PanelList(new CPos((int) (WindowInfo.UnitWidth * 512) - 1080, -3072/2,0), new MPos(1024, 8192 - 3072/2), new MPos(512, 512), 6, "UI_wood1", "UI_wood3", "UI_wood2");
			var list = new List<ActorType>();
			foreach (var n in ActorCreator.GetNames())
			{
				var a = ActorCreator.GetType(n);
				if (a.Playable != null && a.Playable.Playable)
				{
					actorPanel.Add(new PanelItem(CPos.Zero, a.Playable.ChangeCost.ToString(), new ImageRenderable(TextureManager.Sprite(a.Idle)[0], 0.5f), new MPos(512, 512),
						() => {
							changePlayer(game.World.LocalPlayer, a);
						}));
					list.Add(a);
				}
			}
			actorPanelContent = list.ToArray();

			// SECTION EFFECTS
			effectPanel = new PanelList(new CPos(0, (int)(WindowInfo.UnitHeight * 512) - 3072 - 128, 0), new MPos(8192, 256), new MPos(256, 256), 6, "UI_stone1", "UI_stone2");
			foreach (var effect in TechTreeLoader.TechTree)
			{
				var item = new PanelItem(CPos.Zero, effect.Name, new ImageRenderable(TextureManager.Texture(effect.Icon)), new MPos(256, 256), () => { });

				if (!(effect.Unlocked || game.Statistics.UnlockedNodes.ContainsKey(effect.InnerName) && game.Statistics.UnlockedNodes[effect.InnerName]))
					item.SetColor(new Color(0, 0, 0, 1f));

				effectPanel.Add(item);
			}

			background = new Panel(new CPos(0, (int)(WindowInfo.UnitHeight * 512) - 3072/2 + 64, 0), new MPos(8192 / 64 * 6, (3072 - 64) / 64 / 2 * 3), 6, "UI_wood1", "UI_wood3", "UI_wood2");

			// SECTION MONEY
			money = new ImageRenderable(TextureManager.Texture("UI_money"));
			money.SetPosition(new CPos((int)(WindowInfo.UnitWidth * 512) - 8120 + 512, 8192 - 1024, 0));
			moneyText = new TextLine(new CPos((int)(WindowInfo.UnitWidth * 512) - 7096 + 512, 8192 - 1024, 0), IFont.Papyrus24);
			moneyText.SetText(game.Statistics.Money);

			// SECTION MENUS
			pause = new TextLine(new CPos((int)(WindowInfo.UnitWidth * 512) - 4096, 8192 - 1536, 0), IFont.Pixel16);
			pause.WriteText("Pause: '" + new Color(0.5f,0.5f,1f) + "P" + Color.White + "'");

			menu = new TextLine(new CPos((int)(WindowInfo.UnitWidth * 512) - 4096, 8192 - 512, 0), IFont.Pixel16);
			menu.WriteText("Menu: '" + new Color(0.5f, 0.5f, 1f) + "Escape" + Color.White + "'");

			// SECTION HEALTH
			health = new TextLine(new CPos(0, 8192 - 2048, 0), IFont.Papyrus24, TextLine.OffsetType.MIDDLE);

			// SECTION MANA
			mana = new TextLine(new CPos(0, 8192 - 772, 0), IFont.Papyrus24, TextLine.OffsetType.MIDDLE);

			// SECTION MISSION
			missionText = new TextLine(new CPos((int)(-WindowInfo.UnitWidth * 512) + 1024, 8192 - 2048, 0), IFont.Pixel16);
			switch(game.Mode)
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
				case GameMode.TOWER_DEFENSE:
					missionText.SetText("Defend your position!");
					break;
				case GameMode.WIPE_OUT_ENEMIES:
					missionText.SetText("Wipe out all enemies!");
					break;
			}

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
			base.Render();

			// SECTION BASE
			background.Render();

			// SECTION MONEY
			ColorManager.DrawRect(new CPos((int)(WindowInfo.UnitWidth * 512) - 8120, 8192, 0), new CPos((int)(WindowInfo.UnitWidth * 512) - 8120 + 3120, 8192 - 2560, 0), new Color(0, 0, 0, 128));
			money.Render();
			moneyText.Render();

			// SECTION MENUS
			if (!Settings.EnableInfoScreen)
			{
				ColorManager.DrawRect(new CPos((int)(WindowInfo.UnitWidth * 512) - 4096 - 512, 8192, 0), new CPos((int)(WindowInfo.UnitWidth * 512) - 1024 + 512, 8192 - 2560, 0), new Color(0, 0, 0, 128));
				menu.Render();
				pause.Render();
			}

			// SECTION HEALTH
			ColorManager.DrawRect(new CPos(-6120, 8192 - 1536, 0), new CPos(6120, 8192 - 2560, 0), new Color(0, 0, 0, 128));
			// draw line
			ColorManager.DrawRect(new CPos(-6120 + 128, 8192 - 1536 - 128, 0), new CPos(-6120 + 256 + (int) (11856 * healthPercentage), 8192 - 2560 + 128, 0), new Color(255, 0, 0, 128));
			health.Render();

			// SECTION MANA
			ColorManager.DrawRect(new CPos(-6120, 8192 - 256, 0), new CPos(6120, 8192 - 1280, 0), new Color(0, 0, 0, 128));
			//draw line
			ColorManager.DrawRect(new CPos(-6120 + 128, 8192 - 256 - 128, 0), new CPos(-6120 + 256 + (int)(11856 * manaPercentage), 8192 - 1280 + 128, 0), new Color(0, 0, 255, 128));
			mana.Render();

			// SECTION MISSION
			ColorManager.DrawRect(new CPos((int)-(WindowInfo.UnitWidth * 512) + 256, 8192, 0), new CPos(-6120 - 128, 8192 - 2560, 0), new Color(0, 0, 0, 128));
			missionText.Render();

			// SECTION ACTORS
			actorPanel.Render();

			// SECTION EFFECTS
			effectPanel.Render();
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

				if (MouseInput.WheelState != 0)
				{
					var current = Array.FindIndex(actorPanelContent, (a) => a == player.Type);
					current += MouseInput.WheelState;
					if (current < 0)
						current = actorPanelContent.Length - 1;
					if (current >= actorPanelContent.Length)
						current = 0;
					changePlayer(player, actorPanelContent[current]);
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

			actorPanel.Tick();
			effectPanel.Tick();
		}

		void changePlayer(Actor player, ActorType type)
		{
			if (game.Statistics.Money < type.Playable.ChangeCost)
				return;

			game.Statistics.Money -= type.Playable.ChangeCost;

			var oldHP = player.Health != null ? player.Health.HP / (float) player.Health.MaxHP : 1;
			var oldMana = game.Statistics.Mana;
			var newActor = ActorCreator.Create(game.World, type, player.Position, player.Team, isPlayer: true);

			player.Dispose();
			game.World.LocalPlayer = newActor;
			game.World.Add(newActor);

			if (newActor.Health != null)
				newActor.Health.HP = (int) (oldHP * newActor.Health.MaxHP);

			game.Statistics.Actor = ActorCreator.GetName(type);
		}

		public override void Dispose()
		{
			base.Dispose();

			background.Dispose();
			health.Dispose();
			mana.Dispose();

			//money.Dispose();
			moneyText.Dispose();

			menu.Dispose();
			pause.Dispose();
			
			missionText.Dispose();

			actorPanel.Dispose();
			effectPanel.Dispose();
		}
	}
}
