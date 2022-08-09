using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class ActorShopScreen : Screen
	{
		readonly Game game;

		readonly List<string> actorTypes = new List<string>();
		readonly PanelList actors;

		readonly UIText information;
		ActorType selected;

		public ActorShopScreen(Game game) : base("Actor Shop")
		{
			this.game = game;
			Title.Position = new UIPos(0, -4096);

			Add(new Panel(new UIPos(8 * 1024, 2 * 1024), "wooden") { Position = new UIPos(0, 920) });

			actors = new PanelList(new UIPos(8120, 1024), new UIPos(1024, 1024), "stone") { Position = new UIPos(0, -2048) };
			foreach (var a in ActorCache.Types.Values)
			{
				if (a.Playable == null)
					continue;

				var sprite = a.GetPreviewSprite(out var color);
				var scale = Constants.PixelSize / (float)Math.Max(sprite.Width, sprite.Height) - 0.1f;
				var item = new PanelListItem(new BatchObject(Mesh.Image(sprite, color)), new UIPos(1024, 1024), a.Playable.Name, new string[0], () => selectActor(a)) { Scale = scale * 2 };

				if (!game.Stats.ActorUnlocked(a.Playable))
					item.SetColor(Color.Black);

				actors.Add(item);
				actorTypes.Add(a.Playable.InternalName);
			}
			Add(actors);

			Add(new Button("Buy", "wooden", () => buyActor(selected)) { Position = new UIPos(-6144, 3072) });
			Add(new Button("Resume", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)) { Position = new UIPos(0, 6144) });

			information = new UIText(FontManager.Default, TextOffset.LEFT, "Select an actor for further information.", "", "", "Cost: -") { Position = new UIPos(-7900, 0) };
			Add(information);

			Add(new MoneyDisplay(game) { Position = new UIPos(Left + 2048, Bottom - 1024) });
		}

		void selectActor(ActorType actor)
		{
			selected = actor;
			information.SetText(
				actor.Playable.Name,
				Color.Grey + actor.Playable.Description,
				string.Empty,
				Color.White + "Cost: " + (game.Stats.ActorUnlocked(actor.Playable) ? Color.Green + "Bought" : Color.Yellow.ToString() + actor.Playable.UnlockCost)
			);
		}

		void buyActor(ActorType actor)
		{
			if (actor == null)
				return;

			if (game.Stats.ActorUnlocked(actor.Playable))
				return;

			if (game.Stats.Money < actor.Playable.UnlockCost)
				return;

			UIUtils.PlaySellSound();

			game.Stats.Money -= actor.Playable.UnlockCost;

			game.Stats.AddActor(actor.Playable);

			actors.Container[actorTypes.IndexOf(actor.Playable.InternalName)].SetColor(Color.White);
			selectActor(actor);

			game.ScreenControl.UpdateActors();
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.DEFAULT, false);
		}

		public override void Hide()
		{
			actors.DisableTooltip();

			game.ScreenControl.UpdateActors();
		}
	}
}
