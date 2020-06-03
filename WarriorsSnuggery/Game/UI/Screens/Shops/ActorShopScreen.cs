using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class ActorShopScreen : Screen
	{
		readonly Game game;

		readonly MoneyDisplay money;

		readonly List<string> actorTypes = new List<string>();
		readonly PanelList actors;

		readonly TextBlock information;
		ActorType selected;

		public ActorShopScreen(Game game) : base("Actor Shop")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			actors = new PanelList(new CPos(0, -2048, 0), new MPos(8120, 1024), new MPos(1024, 1024), PanelManager.Get("stone"));
			foreach (var n in ActorCreator.GetNames())
			{
				var a = ActorCreator.GetType(n);
				if (a.Playable == null)
					continue;


				var sprite = a.GetPreviewSprite();
				var scale = (sprite.Width > sprite.Height ? 24f / sprite.Width : 24f / sprite.Height) - 0.1f;
				var item = new PanelItem(CPos.Zero, new BatchObject(sprite, Color.White), new MPos(512, 512), a.Playable.Name, new string[0], () => selectActor(a))
				{
					Scale = scale * 2f
				};
				if (!game.Statistics.ActorAvailable(a.Playable))
					item.SetColor(Color.Black);

				actors.Add(item);
				actorTypes.Add(a.Playable.InternalName);
			}

			Content.Add(new Panel(new CPos(0, 920, 0), new Vector(8, 2, 0), PanelManager.Get("wooden")));
			Content.Add(ButtonCreator.Create("wooden", new CPos(-6144, 3072, 0), "Buy", () => buyActor(selected)));

			Content.Add(ButtonCreator.Create("wooden", new CPos(0, 6144, 0), "Resume", () => { game.Pause(false); game.ScreenControl.ShowScreen(ScreenType.DEFAULT); }));

			information = new TextBlock(new CPos(-7900, 0, 0), FontManager.Pixel16, TextLine.OffsetType.LEFT, "Select an actor for further information.", "", "", "Cost: -");

			money = new MoneyDisplay(game, new CPos(-(int)(WindowInfo.UnitWidth / 2 * 1024) + 1024, 7192, 0));
		}

		void selectActor(ActorType actor)
		{
			selected = actor;
			information.Lines[0].WriteText(actor.Playable.Name);
			information.Lines[1].WriteText(Color.Grey + actor.Playable.Description);
			if (game.Statistics.ActorAvailable(actor.Playable))
				information.Lines[3].WriteText(Color.White + "Cost: " + Color.Green + "Bought");
			else
				information.Lines[3].WriteText(Color.White + "Cost: " + Color.Yellow + actor.Playable.UnlockCost);
		}

		void buyActor(ActorType actor)
		{
			if (actor == null)
				return;

			if (game.Statistics.ActorAvailable(actor.Playable))
				return;

			if (game.Statistics.Money < actor.Playable.UnlockCost)
				return;

			game.Statistics.Money -= actor.Playable.UnlockCost;
			if (game.Statistics.UnlockedActors.ContainsKey(actor.Playable.InternalName))
				game.Statistics.UnlockedActors[actor.Playable.InternalName] = true;
			else
				game.Statistics.UnlockedActors.Add(actor.Playable.InternalName, true);

			actors.Container[actorTypes.IndexOf(actor.Playable.InternalName)].SetColor(Color.White);
			information.Lines[3].WriteText(Color.White + "Cost: " + Color.Green + "Bought");

			game.ScreenControl.UpdateActors();
		}

		public override void Render()
		{
			base.Render();

			actors.Render();
			information.Render();
			money.Render();
		}

		public override void Tick()
		{
			base.Tick();

			actors.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
			{
				game.Pause(false);
				game.ChangeScreen(ScreenType.DEFAULT);
			}

			information.Tick();
			money.Tick();
		}

		public override void Hide()
		{
			actors.DisableTooltip();

			game.ScreenControl.UpdateActors();
		}
	}
}
