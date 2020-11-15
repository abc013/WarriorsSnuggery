using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace WarriorsSnuggery
{
	public class WorldSaver
	{
		public const int MapFormat = 1;

		readonly World world;
		readonly MPos bounds;
		readonly bool isSavegame;

		public WorldSaver(World world, bool isSavegame)
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

			writeTerrainLayer(writer);
			writeWallLayer(writer);
			writeActorLayer(writer);

			if (isSavegame)
			{
				writeWeaponLayer(writer);
				writeParticleLayer(writer); 
			}

			writer.Flush();
			writer.Close();
		}

		void writeTerrainLayer(StreamWriter writer)
		{
			const string text = "Terrain=";

			var builder = new StringBuilder(text.Length + bounds.X * bounds.Y * 2, text.Length + bounds.X * bounds.Y * 4);
			builder.Append(text);

			for (int y = 0; y < bounds.Y; y++)
			{
				for (int x = 0; x < bounds.X; x++)
				{
					builder.Append(world.TerrainLayer.Terrain[x, y].Type.ID);
					builder.Append(',');
				}
			}

			builder.Remove(builder.Length - 1, 1);
			writer.WriteLine(builder);
			builder.Clear();
		}

		void writeWallLayer(StreamWriter writer)
		{
			const string text = "Walls=";

			var wallSize = world.WallLayer.Bounds;
			var builder = new StringBuilder(text.Length + wallSize.X * wallSize.Y * 5, text.Length + wallSize.X * wallSize.Y * 12);

			builder.Append(text);
			for (int y = 0; y < wallSize.Y - 1; y++)
			{
				for (int x = 0; x < wallSize.X - 1; x++)
				{
					var wall = world.WallLayer.Walls[x, y];
					if (wall == null)
						builder.Append("-1,0,");
					else
						builder.Append(wall.Type.ID + "," + wall.Health + ",");
				}
			}

			builder.Remove(builder.Length - 1, 1);
			writer.WriteLine(builder);
			builder.Clear();
		}

		void writeActorLayer(StreamWriter writer)
		{
			writer.WriteLine("Actors=");

			var i = 0u;
			foreach (var a in world.ActorLayer.Actors)
			{
				var id = isSavegame ? a.ID : i++;

				writer.WriteLine("\t" + id + "=");

				foreach (var node in a.Save())
					writer.WriteLine("\t\t" + node);
			}
		}

		void writeWeaponLayer(StreamWriter writer)
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

		void writeParticleLayer(StreamWriter writer)
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

		public static List<string> GetSaveFields<T>(T @object, bool inherit = true)
		{
			var list = new List<string>();

			var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var prop in props)
			{
				var attributes = prop.GetCustomAttributes(inherit);
				foreach (var attribute in attributes)
				{
					if (!(attribute is SaveAttribute saveAttribute))
						continue;

					var key = string.IsNullOrEmpty(saveAttribute.Name) ? prop.Name : saveAttribute.Name;
					var value = prop.GetValue(@object);
					list.Add($"{key}={value}");
				}
			}
			var varis = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var vari in varis)
			{
				var attributes = vari.GetCustomAttributes(inherit);
				foreach (var attribute in attributes)
				{
					if (!(attribute is SaveAttribute saveAttribute))
						continue;

					var key = string.IsNullOrEmpty(saveAttribute.Name) ? vari.Name : saveAttribute.Name;
					var value = vari.GetValue(@object);
					list.Add($"{key}={value}");
				}
			}

			return list;
		}
	}
}
