using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Parts;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Maps
{
	public class Piece
	{
		public readonly MPos Size;
		public readonly string Name;
		public readonly string InnerName;
		public readonly string Path;

		readonly ushort[] groundData;
		readonly short[] wallData;

		readonly List<ActorNode> actors;
		readonly List<WeaponInit> weapons;
		readonly List<ParticleInit> particles;

		Piece(MPos size, ushort[] groundData, short[] wallData, string name, string innerName, string path, List<ActorNode> actors, List<WeaponInit> weapons, List<ParticleInit> particles)
		{
			Size = size;
			Name = name;
			InnerName = innerName;
			Path = path;

			this.groundData = groundData;
			this.wallData = wallData;
			this.actors = actors;
			this.weapons = weapons;
			this.particles = particles;
		}

		public static Piece Load(string innerName, string path, MiniTextNode[] nodes)
		{
			MPos size = MPos.Zero;

			ushort[] groundData = new ushort[0];
			short[] wallData = new short[0];

			string name = "unknown";
			var actors = new List<ActorNode>();
			var weapons = new List<WeaponInit>();
			var particles = new List<ParticleInit>();

			foreach (var rule in nodes)
			{
				switch (rule.Key)
				{
					case "Size":
						size = rule.Convert<MPos>();

						break;
					case "Terrain":
						groundData = rule.Convert<ushort[]>();

						break;
					case "Walls":
						wallData = rule.Convert<short[]>();

						break;
					case "Name":
						name = rule.Convert<string>();

						break;
					case "Actors":
						var actorNodes = rule.Children.ToArray();

						foreach (var actor in actorNodes)
						{
							try
							{
								var id = uint.Parse(actor.Key);
								var position = actor.Convert<CPos>();

								actors.Add(new ActorNode(id, position, actor.Children.ToArray()));
							}
							catch (Exception e)
							{
								throw new InvalidPieceException(string.Format(@"unable to load actor '{0}' in piece '{1}'.", actor.Key, name), e);
							}
						}
						break;
					case "Weapons":
						var weaponNodes = rule.Children.ToArray();

						foreach (var weapon in weaponNodes)
						{
							try
							{
								var id = uint.Parse(weapon.Key);
								weapons.Add(new WeaponInit(path, id, weapon.Children));
							}
							catch (Exception e)
							{
								throw new InvalidPieceException(string.Format(@"unable to load weapon '{0}' in piece '{1}'.", weapon.Key, name), e);
							}
						}
						break;
					case "Particles":
						var particleNodes = rule.Children.ToArray();

						foreach (var particle in particleNodes)
						{
							try
							{
								particles.Add(new ParticleInit(path, particle.Children));
							}
							catch (Exception e)
							{
								throw new InvalidPieceException(string.Format(@"unable to load particle '{0}' in piece '{1}'.", particle.Key, name), e);
							}
						}
						break;
				}
			}

			if (groundData.Length != size.X * size.Y)
				throw new InvalidPieceException(string.Format(@"The count of given terrains ({0}) is not the size ({1}) of the piece '{2}'", groundData.Length, size.X * size.Y, name));

			if (wallData.Length != (size.X + 1) * (size.Y + 1) * 2 * 2)
				throw new InvalidPieceException(string.Format(@"The count of given walls ({0}) is smaller as the size ({1}) on the piece '{2}'", groundData.Length, size.X * size.Y, name));

			return new Piece(size, groundData, wallData, name, innerName, path, actors, weapons, particles);
		}

		public void PlacePiece(MPos position, World world)
		{
			// generate Terrain
			for (int y = position.Y; y < (Size.Y + position.Y); y++)
			{
				for (int x = position.X; x < (Size.X + position.X); x++)
				{
					world.TerrainLayer.Set(TerrainCreator.Create(world, new MPos(x, y), groundData[(y - position.Y) * Size.X + (x - position.X)]));
				}
			}

			// generate Walls
			if (wallData.Length != 0)
			{
				var maxY = (Size.Y + 1 + position.Y);
				var maxX = (Size.X + 1 + position.X) * 2;
				for (int y = position.Y; y < maxY; y++)
				{
					for (int x = position.X * 2; x < maxX; x++)
					{
						var dataPos = (y - position.Y) * (Size.X + 1) * 2 + (x - position.X * 2);
						dataPos *= 2;

						if (wallData[dataPos] >= 0)
						{
							var wall = WallCreator.Create(new MPos(x, y), world.WallLayer, wallData[dataPos]);
							world.WallLayer.Set(wall);

							wall.Health = wallData[dataPos + 1];
						}
						else if (world.WallLayer.Walls[x, y] != null && x != position.X * 2 && y != position.Y && y != maxY - 1 && !(x >= maxX - 2))
							world.WallLayer.Remove(new MPos(x, y));
					}
				}
			}

			if (!world.Map.Type.FromSave)
			{
				if (actors.Any())
					world.Game.CurrentActorID = actors.Max(n => n.ID) + 1;
				if (weapons.Any())
					world.Game.CurrentWeaponID = weapons.Max(n => n.ID) + 1;
			}
			// generate Actors
			foreach (var actor in actors)
			{
				var a = world.Map.Type.FromSave ? 
					ActorCreator.Create(world, actor.Type, actor.Position + position.ToCPos(), actor.Team, actor.IsBot, actor.IsPlayer, actor.Health, actor.ID) : 
					ActorCreator.Create(world, actor.Type, actor.Position + position.ToCPos(), actor.Team, actor.IsBot, actor.IsPlayer, actor.Health);
				if (actor.IsPlayer) world.LocalPlayer = a;
				if (actor.BotTarget.X != int.MaxValue)
					a.BotPart.Target = new Target(actor.BotTarget, 0);
				if (actor.IsPlayerSwitch)
				{
					var part = (PlayerSwitchPart)a.Parts.Find(p => p is PlayerSwitchPart);
					part.ActorType = actor.ToActor;
					part.CurrentTick = actor.Duration;
					part.RelativeHP = actor.RelativeHP;
				}
				world.Add(a);
			}

			// generate Weapons
			foreach (var weapon in weapons)
				world.Add(WeaponCreator.Create(world, weapon));

			// generate Particles
			foreach (var particle in particles)
				world.Add(ParticleCreator.Create(world, particle, world.Game.SharedRandom));
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
