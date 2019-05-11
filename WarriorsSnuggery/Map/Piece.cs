using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Maps
{
	public class Piece
	{
		public readonly MPos Size;
		public readonly string Name;

		readonly int[] groundData;
		readonly int[] wallData;

		readonly ActorNode[] actors;

		Piece(MPos size, int[] groundData, int[] wallData, string name, ActorNode[] actors)
		{
			Size = size;
			Name = name;
			this.groundData = groundData;
			this.wallData = wallData;
			this.actors = actors;
		}

		public static Piece LoadPiece(MiniTextNode[] nodes)
		{
			MPos size = MPos.Zero;

			int[] groundData = new int[0];
			int[] wallData = new int[0];

			string name = "unknown";
			ActorNode[] actors = new ActorNode[0];

			foreach (var rule in nodes)
			{
				switch (rule.Key)
				{
					case "Size":
						size = rule.ToMPos();

						break;
					case "Terrain":
						var groundDataStrings = rule.ToArray();
						groundData = new int[groundDataStrings.Length];

						for (int i = 0; i < groundData.Length; i++)
						{
							if (!int.TryParse(groundDataStrings[i], out groundData[i]))
								throw new YamlInvalidNodeException(string.Format(@"unable to load terrain-ID '{0}' in piece '{1}'.", groundDataStrings[i], name));
						}

						break;
					case "Walls":
						var wallDataStrings = rule.ToArray();
						wallData = new int[wallDataStrings.Length];

						for (int i = 0; i < wallData.Length; i++)
						{
							if (!int.TryParse(wallDataStrings[i], out wallData[i]))
								throw new YamlInvalidNodeException(string.Format(@"unable to load terrain-ID '{0}' in piece '{1}'.", wallDataStrings[i], name));
						}

						break;
					case "Name":
						name = rule.Value;

						break;
					case "Actors":
						var actorNodes = rule.Children.ToArray();
						var actorList = new List<ActorNode>();

						foreach(var actor in actorNodes)
						{
							try
							{
								if (actor.Children.Count > 0) // New Actor System
								{
									var id = int.Parse(actor.Key);
									var position = actor.ToCPos();

									actorList.Add(new ActorNode(id, position, actor.Children.ToArray()));
								}
								else // Old Actor System
								{
									var split = actor.Key.Split(';');
									var type = split[0];
									var team = split.Length > 1 ? int.Parse(split[1]) : 0;
									var bot = split.Length > 1 ? split[2].Equals("true") : false;
									actorList.Add(new ActorNode(0, actor.ToCPos(), type, team, 1f, bot));
								}
							}
							catch (Exception e)
							{
								throw new YamlInvalidNodeException(string.Format(@"unable to load actor '{0}' in piece '{1}'.", actor.Key, name), e);
							}
						}

						actors = actorList.ToArray();
						break;
				}
			}

			if (groundData.Length < size.X * size.Y)
				throw new YamlInvalidNodeException(string.Format(@"The count of given terrains ({0}) is not the same as the size ({1}) on the piece '{2}'", groundData.Length, size.X * size.Y, name));

			if (wallData.Length != 0 && wallData.Length < size.X * size.Y)
				throw new YamlInvalidNodeException(string.Format(@"The count of given walls ({0}) is not the same as the size ({1}) on the piece '{2}'", groundData.Length, size.X * 2 * size.Y, name));

			return new Piece(size, groundData, wallData, name, actors);
		}

		public void PlacePiece(MPos position, World world)
		{
			// generate Terrain
			for (int y = position.Y; y < (Size.Y + position.Y); y++)
			{
				for (int x = position.X; x < (Size.X + position.X); x++)
				{
					world.TerrainLayer.Set(TerrainCreator.Create(world, new WPos(x, y, 0), groundData[(y - position.Y) * Size.X + (x - position.X)]));
				}
			}

			// generate Walls
			if (wallData.Length != 0)
			{
				for (int y = position.Y; y < (Size.Y + position.Y); y++)
				{
					for (int x = position.X * 2; x < (Size.X + position.X) * 2; x++)
					{
						if (wallData[(y - position.Y) * Size.X * 2 + (x - position.X * 2)] >= 0)
							world.WallLayer.Set(WallCreator.Create(new WPos(x, y, 0), wallData[(y - position.Y) * Size.X * 2 + (x - position.X * 2)]));
						else
							world.WallLayer.Remove(new MPos(x, y));
					}
				}
			}

			// generate Actors
			foreach (var actor in actors)
			{
				world.Add(ActorCreator.Create(world, actor.Type, actor.Position + position.ToCPos(), actor.Team, actor.IsBot, false, actor.Health));
			}
		}

		public bool IsInMap(MPos position, MPos mapSize)
		{
			if (Size.X + position.X > mapSize.X)
				return false;
			if (Size.Y + position.Y > mapSize.Y)
				return false;

			return true;
		}
	}
}
