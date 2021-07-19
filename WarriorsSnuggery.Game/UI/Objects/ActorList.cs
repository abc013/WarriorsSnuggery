using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.UI.Objects
{
	public class ActorList : PanelList
	{
		static readonly Color disabled = new Color(0f, 0f, 0f, 0.3f);

		readonly Game game;

		readonly List<ActorType> actorTypes = new List<ActorType>();

		public int CurrentActor
		{
			get => currentActor;
			set
			{
				currentActor = value;

				currentActor %= actorTypes.Count;

				if (currentActor < 0)
					currentActor = actorTypes.Count - 1;

				SelectedPos = (currentActor % Size.X, currentActor / Size.X);
			}
		}
		int currentActor;

		public ActorList(Game game, MPos bounds, MPos itemSize, string typeName) : this(game, bounds, itemSize, PanelManager.Types[typeName]) { }

		public ActorList(Game game, MPos bounds, MPos itemSize, PanelType type) : base(bounds, itemSize, type, false)
		{
			this.game = game;

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
				var scale = MasterRenderer.PixelSize / (float)Math.Max(sprite.Width, sprite.Height) - 0.1f;
				var item = new PanelListItem(new BatchObject(sprite), ItemSize, actorType.Playable.Name, new[] { Color.Grey + "Cost: " + Color.Yellow + actorType.Playable.Cost }, () => { changePlayer(actorType); }) { Scale = scale };
				if (!game.Stats.ActorAvailable(actorType.Playable))
					item.SetColor(disabled);

				actorTypes.Add(actorType);
				Add(item);
			}
		}

		public void Update()
		{
			for (int i = 0; i < actorTypes.Count; i++)
				Container[i].SetColor(game.Stats.ActorAvailable(actorTypes[i].Playable) ? Color.White : disabled);
		}

		public override void Tick()
		{
			base.Tick();

			if (KeyInput.IsKeyDown(Keys.LeftShift))
			{
				CurrentActor += MouseInput.WheelState;

				if (KeyInput.IsKeyDown(Settings.KeyDictionary["Activate"]) || !KeyInput.IsKeyDown(Keys.LeftControl) && MouseInput.IsRightClicked)
					changePlayer(actorTypes[CurrentActor]);

				for (int i = 0; i < Math.Max(actorTypes.Count, 10); i++)
				{
					if (KeyInput.IsKeyDown(Keys.D0 + i))
						game.SpellManager.Activate((i + 9) % 10);
				}
			}
		}

		void changePlayer(ActorType type)
		{
			if (game.Stats.Money < type.Playable.Cost)
				return;

			if (game.World.LocalPlayer.Type == type)
				return;

			if (!game.Stats.ActorAvailable(type.Playable))
				return;

			game.Stats.Money -= type.Playable.Cost;

			game.World.BeginPlayerSwitch(type);
		}
	}
}
