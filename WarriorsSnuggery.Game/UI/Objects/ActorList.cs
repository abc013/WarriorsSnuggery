using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Actors.Parts;

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

		public ActorList(Game game, MPos bounds, MPos itemSize, string typeName) : this(game, bounds, itemSize, PanelCache.Types[typeName]) { }

		public ActorList(Game game, MPos bounds, MPos itemSize, PanelType type) : base(bounds, itemSize, type, false)
		{
			this.game = game;

			addActors();

			CurrentActor = 0;
		}

		void addActors()
		{
			foreach (var actorType in ActorCache.Types.Values)
			{
				if (actorType.Playable == null)
					continue;

				actorTypes.Add(actorType);
				Add(updateSingle(actorType));
			}
		}

		public void Update()
		{
			int index = 0;
			foreach (var actorType in actorTypes)
				Container[index++] = updateSingle(actorType);

			UpdatePositions();
		}

		PanelListItem updateSingle(ActorType actorType)
		{
			var available = game.Stats.ActorAvailable(actorType.Playable);
			var sprite = actorType.GetPreviewSprite();
			var scale = Constants.PixelSize / (float)Math.Max(sprite.Width, sprite.Height) - 0.1f;

			var title = available ? actorType.Playable.Name : Color.Red + "Locked";
			var description = available ? new[] { Color.Grey + "Changing cost: " + Color.Yellow + actorType.Playable.Cost } : new[] { new Color(128, 0, 0) + "Unlock cost: " + actorType.Playable.UnlockCost };

			var item = new PanelListItem(new BatchObject(sprite), ItemSize, title, description, () => { changePlayer(actorType); }) { Scale = scale };

			if (!available)
				item.SetColor(disabled);

			return item;
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

			if (game.World.LocalPlayer.GetPartOrDefault<PlayablePart>() == null)
				return;

			game.Stats.Money -= type.Playable.Cost;

			game.World.BeginPlayerSwitch(type);
		}
	}
}
