﻿using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class ActorList : PanelList
	{
		readonly Game game;

		readonly BatchObject selector;

		readonly List<ActorType> actorTypes = new List<ActorType>();

		public int CurrentActor
		{
			get => currentActor;
			set
			{
				currentActor = value;

				if (currentActor >= Container.Count)
					currentActor = 0;

				if (currentActor < 0)
					currentActor = Container.Count - 1;

				selector.SetPosition(Container[currentActor].Position + new CPos(712, 0, 0));
			}
		}
		int currentActor;

		public ActorList(Game game, CPos pos, MPos bounds, MPos itemSize, string typeName) : this(game, pos, bounds, itemSize, PanelManager.Get(typeName)) { }

		public ActorList(Game game, CPos pos, MPos bounds, MPos itemSize, PanelType type) : base(pos, bounds, itemSize, type)
		{
			this.game = game;

			selector = new BatchObject(UITextureManager.Get("UI_selector2")[0], Color.White);

			addActors();

			CurrentActor = 0;
		}

		void addActors()
		{
			foreach (var actorType in ActorCreator.Types.Values)
			{
				if (actorType.Playable == null)
					continue;

				var sprite = actorType.GetPreviewSprite();
				var scale = 24f / Math.Max(sprite.Width, sprite.Height) - 0.1f;
				var item = new PanelItem(new BatchObject(sprite, Color.White), ItemSize, actorType.Playable.Name, new[] { Color.Grey + "Cost: " + Color.Yellow + actorType.Playable.Cost }, () => { changePlayer(actorType); })
				{
					Scale = scale
				};
				if (!game.Statistics.ActorAvailable(actorType.Playable))
					item.SetColor(Color.Black);

				actorTypes.Add(actorType);
				Add(item);
			}
		}

		public void Update()
		{
			for (int i = 0; i < actorTypes.Count; i++)
				Container[i].SetColor(game.Statistics.ActorAvailable(actorTypes[i].Playable) ? Color.White : Color.Black);
		}

		public override void Render()
		{
			base.Render();

			selector.PushToBatchRenderer();
		}

		public override void Tick()
		{
			base.Tick();

			if (KeyInput.IsKeyDown(Keys.LeftShift))
			{
				CurrentActor += MouseInput.WheelState;

				if (!KeyInput.IsKeyDown(Keys.LeftControl) && MouseInput.IsRightClicked)
					changePlayer(actorTypes[CurrentActor]);
			}
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
