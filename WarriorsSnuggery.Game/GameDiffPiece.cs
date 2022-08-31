using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery
{
	public class GameDiffPiece
	{
		public readonly List<ActorInit> actorInits = new List<ActorInit>();
		public readonly List<WeaponInit> weaponInits = new List<WeaponInit>();

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
								// throw new InvalidPieceException($"[{PackageFile}] Unable to load actor '{actor.Key}'.", e);
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
								// throw new InvalidPieceException($"[{PackageFile}] Unable to load weapon '{weapon.Key}'.", e);
							}
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
