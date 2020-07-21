using System.IO;
using System.Text;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery
{
	public class MapSaver
	{
		public const int MapFormat = 0;

		readonly World world;
		readonly MPos bounds;
		readonly bool isSavegame;

		public MapSaver(World world, bool isSavegame)
		{
			this.world = world;
			bounds = world.Map.Bounds;
			this.isSavegame = isSavegame;
		}

		public void Save(string directory, string name)
		{
			using var writer = new StreamWriter(directory + name + ".yaml", false);

			writer.WriteLine("MapFormat=" + MapFormat);
			writer.WriteLine("Name=" + name);
			writer.WriteLine("Size=" + bounds);

			var builder = new StringBuilder(8 + bounds.X * bounds.Y * 3, 8 + bounds.X * bounds.Y * 4);
			builder.Append("Terrain=");
			for (int y = 0; y < bounds.Y; y++)
			{
				for (int x = 0; x < bounds.X; x++)
				{
					builder.Append(world.TerrainLayer.Terrain[x, y].Type.ID);
					if (x == bounds.X - 1 && y == bounds.Y - 1)
						break;
					builder.Append(",");
				}
			}
			writer.WriteLine(builder);
			builder.Clear();

			var wallSize = world.WallLayer.Bounds;
			var builder2 = new StringBuilder(6 + wallSize.X * wallSize.Y * 6, 6 + wallSize.X * wallSize.Y * 12);
			builder2.Append("Walls=");
			for (int y = 0; y < wallSize.Y - 1; y++)
			{
				for (int x = 0; x < wallSize.X - 1; x++)
				{
					var wall = world.WallLayer.Walls[x, y];
					if (wall == null)
						builder2.Append("-1,0,");
					else
						builder2.Append(wall.Type.ID + "," + wall.Health + ",");
				}
			}
			builder2.Remove(builder2.Length - 1, 1);
			writer.WriteLine(builder2);
			builder2.Clear();

			writeActors(writer);
			if (isSavegame)
			{
				writeWeapons(writer);
				writeParticles(writer); 
			}

			writer.Flush();
			writer.Close();
		}

		void writeActors(StreamWriter writer)
		{
			writer.WriteLine("Actors=");
			var i = 0u;
			foreach (var a in world.ActorLayer.Actors)
			{
				var id = isSavegame ? a.ID : i++;
				writer.WriteLine("\t" + id + "=" + a.Position);
				writer.WriteLine("\t\t" + "Type=" + ActorCreator.GetName(a.Type));

				if (a.Team != Actor.NeutralTeam)
					writer.WriteLine("\t\t" + "Team=" + a.Team);
				if (a.Health != null && a.Health.HP != a.Health.MaxHP)
					writer.WriteLine("\t\t" + "Health=" + a.Health.RelativeHP.ToString(Settings.FloatFormat));

				if (a.IsBot)
				{
					writer.WriteLine("\t\t" + "IsBot=" + a.IsBot);

					if (a.BotPart.Target != null)
						writer.WriteLine("\t\t" + "BotTarget=" + a.BotPart.Target.Position);
				}

				if (isSavegame)
				{
					if (a.IsPlayer)
						writer.WriteLine("\t\t" + "IsPlayer=true");
					if (a.IsPlayerSwitch)
					{
						writer.WriteLine("\t\t" + "IsPlayerSwitch=true");
						var part = (PlayerSwitchPart)a.Parts.Find(p => p is PlayerSwitchPart);

						writer.WriteLine("\t\t" + "Duration=" + part.CurrentTick);
						writer.WriteLine("\t\t" + "ToActor=" + ActorCreator.GetName(part.ActorType));
						writer.WriteLine("\t\t" + "RelativeHP=" + part.RelativeHP);
					}
				}
			}
		}

		void writeWeapons(StreamWriter writer)
		{
			writer.WriteLine("Weapons=");
			var i = 0;
			foreach(var weapon in world.WeaponLayer.Weapons)
			{
				var list = weapon.Save();

				writer.WriteLine("\t" + i + "=");

				foreach(var rule in list)
					writer.WriteLine("\t\t" + rule);
			}
		}

		void writeParticles(StreamWriter writer)
		{
			writer.WriteLine("Particles=");
			var i = 0;
			foreach (var particle in world.ParticleLayer.Particles)
			{
				var list = particle.Save();

				writer.WriteLine("\t" + i + "=");

				foreach (var rule in list)
					writer.WriteLine("\t\t" + rule);
			}
		}
	}
}
