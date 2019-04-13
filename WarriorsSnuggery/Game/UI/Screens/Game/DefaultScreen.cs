using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class DefaultScreen : Screen
	{
		readonly Game game;
		readonly Text health;
		readonly ColoredRect hollow;
		ColoredRect healthBar;
		readonly Text mana;
		readonly ColoredCircle manaComb;
		readonly CPos topLineDeco;

		readonly GameObject money;
		readonly Text moneyText;
		readonly Text menu, pause;
		readonly PanelList panel;
		readonly PanelList manaPanel;
		readonly ActorType[] panelContent;
		int cashCooldown;
		int lastCash;
		int healthCooldown;
		int lastHealth;

		public DefaultScreen(Game game) : base("Level " + game.Statistics.Level + "/" + game.Statistics.FinalLevel, 0)
		{
			this.game = game;
			Title.Position += new CPos(0,-7120,0);
			if (game.Statistics.Level >= game.Statistics.FinalLevel)
				Title.SetColor(Color.Green);

			var corner = (int) (WindowInfo.UnitWidth / 2 * 1024);

			var mid = (int) -WindowInfo.UnitHeight * 400;
			topLineDeco = new CPos((int)(WindowInfo.UnitWidth * 512), mid, 0);

			health = new Text(new CPos(-(int) (WindowInfo.UnitWidth * 300) - 1536,-7120,0), IFont.Papyrus24, Text.OffsetType.MIDDLE);
			healthBar = new ColoredRect(new CPos(-(int)(WindowInfo.UnitWidth * 300) + 1024, -7120, 0), Color.Red, 5f, 1f);
			hollow = new ColoredRect(new CPos(-(int)(WindowInfo.UnitWidth * 300) - 2048, -7120, 0), Color.White, 5.2f, 1.2f, isFilled: false);
			mana = new Text(new CPos((int) (WindowInfo.UnitWidth * 300),-7120,0), IFont.Papyrus24);
			manaComb = new ColoredCircle(new CPos((int) (WindowInfo.UnitWidth * 300) + 2048,-7120, 0), Color.White, resolution: 6);

			money = new GameObject(new CPos(-corner + 1024, 7192,0), new ImageRenderable(TextureManager.Texture("UI_money")));
			moneyText = new Text(new CPos(-corner + 2048,7192,0), IFont.Papyrus24);
			moneyText.SetText(game.Statistics.Money);

			pause = new Text(new CPos(5000,(int) mid + 512,0), IFont.Pixel16);
			pause.WriteText("Pause: '" + Color.Blue + "P" + Color.White + "'");

			menu = new Text(new CPos(-8000,(int) mid + 512,0), IFont.Pixel16);
			menu.WriteText("Menu: '" + Color.Blue + "Escape" + Color.White + "'");

			panel = new PanelList(new CPos(0,(int)(WindowInfo.UnitHeight * 512 - 1536), 0), new MPos(8192,512), new MPos(512, 512), 6, "UI_wood1", "UI_wood3", "UI_wood2");
			var list = new List<ActorType>();
			foreach(var n in ActorCreator.GetNames())
			{
				var a = ActorCreator.GetType(n);
				if (a.Playable != null && a.Playable.Playable)
				{
					panel.Add(new PanelItem(CPos.Zero, n + ": " + a.Playable.ChangeCost, new ImageRenderable(TextureManager.Sprite(a.Idle)[0], 0.5f), new MPos(512,512),
						() => {
								changePlayer(game.World.LocalPlayer, a);
						}));
					list.Add(a);
				}
			}
			panelContent = list.ToArray();

			manaPanel = new PanelList(new CPos((int) (WindowInfo.UnitWidth * 512 - 1024), (int)-(WindowInfo.UnitHeight * 128), 0), new MPos(512, 4096), new MPos(512, 512), 6, "UI_stone1", "UI_stone2");
			if (game.World.LocalPlayer.Type.ManaWeapon != null)
			{
				manaPanel.Add(new PanelItem(CPos.Zero, "Cost:" + game.World.LocalPlayer.Type.ManaCost, new ImageRenderable(game.World.LocalPlayer.Type.ManaWeapon.Textures.GetTextures()[0], 0.5f), new MPos(512, 512), () => { }));
			}
		}

		public override void Render()
		{
			ColorManager.DrawRect(topLineDeco, new CPos(-topLineDeco.X, topLineDeco.Y - (int) (WindowInfo.UnitHeight * 112), 0), new Color(0,0,0,128));
			ColorManager.DrawLine(topLineDeco, new CPos(-topLineDeco.X, topLineDeco.Y, 0), Color.White);
			base.Render();

			hollow.Render();
			healthBar.Render();
			health.Render();
			manaComb.Render();
			mana.Render();

			money.Render();
			moneyText.Render();

			menu.Render();
			pause.Render();

			panel.Render();
			manaPanel.Render();
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

					if (cur != lastHealth)
					{
						healthBar.Dispose();
						healthBar = new ColoredRect(new CPos(-(int)(WindowInfo.UnitWidth * 300) - 2048, -7120, 0), Color.Red, (cur / (float)max) * 5f, 1f);
						if (cur < lastHealth) // To avoid scaling when regenerating
							healthCooldown = 10;
					}
					lastHealth = cur;
					if (healthCooldown-- >= 0)
						healthBar.Scale = (healthCooldown / 100f) + 1f;
				}

				mana.SetText(game.Statistics.Mana);

				var b = game.Statistics.MaxMana <= 0 ? 0 : (game.Statistics.Mana / (float) game.Statistics.MaxMana * 256) - 1;
				manaComb.setColor(new Color(0,0,b,240));
				manaComb.Rotation = new CPos(0,0,Convert.ToInt32(game.LocalTick) / 8);

				if (MouseInput.WheelState != 0)
				{
					var current = Array.FindIndex(panelContent, (a) => a == player.Type);
					current += MouseInput.WheelState;
					if (current < 0)
						current = panelContent.Length - 1;
					if (current >= panelContent.Length)
						current = 0;
					changePlayer(player, panelContent[current]);
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

			panel.Tick();
			manaPanel.Tick();
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
			if (game.World.LocalPlayer.Type.ManaWeapon != null)
			{
				foreach(var o in manaPanel.Container)
				{
					o.Dispose();
				}
				manaPanel.Container.Clear();
				manaPanel.Add(new PanelItem(CPos.Zero, "Cost:" + game.World.LocalPlayer.Type.ManaCost, new ImageRenderable(game.World.LocalPlayer.Type.ManaWeapon.Textures.GetTextures()[0], 0.5f), new MPos(512, 512), () => { }));
			}
		}

		public override void Dispose()
		{
			base.Dispose();

			health.Dispose();
			healthBar.Dispose();
			hollow.Dispose();
			mana.Dispose();
			manaComb.Dispose();

			money.Dispose();
			moneyText.Dispose();

			menu.Dispose();
			pause.Dispose();
		}
	}
}
