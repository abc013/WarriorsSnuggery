using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps.Pieces;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery
{
	public class GameDiffPiece
	{
		public readonly List<ActorInit> actorInits = new List<ActorInit>();
		public readonly List<WeaponInit> weaponInits = new List<WeaponInit>();
		public readonly List<WallInit> wallInits = new List<WallInit>();

		public GameDiffPiece(List<TextNode> nodes)
		{
			var fields = TypeLoader.GetFields(this);

			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Actors":
						foreach (var actor in node.Children)
						{
							try
							{
								var id = uint.Parse(actor.Key);
								var init = new ActorInit(id, actor.Children, Constants.CurrentMapFormat);

								actorInits.Add(init);
							}
							catch (Exception e)
							{
								throw new InvalidPieceException($"[Networking] Unable to load actor '{actor.Key}'.", e);
							}
						}
						break;
					case "Weapons":
						foreach (var weapon in node.Children)
						{
							try
							{
								var id = uint.Parse(weapon.Key);
								var init = new WeaponInit(id, weapon.Children, Constants.CurrentMapFormat);

								weaponInits.Add(init);
							}
							catch (Exception e)
							{
								throw new InvalidPieceException($"[Networking] Unable to load weapon '{weapon.Key}'.", e);
							}
						}
						break;
					case "Walls":
						foreach (var wall in node.Children)
						{
							var id = uint.Parse(wall.Key);

							wallInits.Add(new WallInit(id, wall.Children, Constants.CurrentMapFormat));
						}

						break;
					default:
						TypeLoader.SetValue(this, fields, node);

						break;
				}
			}
		}
	}
}
