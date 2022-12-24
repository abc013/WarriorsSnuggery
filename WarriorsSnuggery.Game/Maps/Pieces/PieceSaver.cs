using System.IO;
using System.Linq;
using System.Text;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Maps.Pieces
{
	public class PieceSaver
	{
		readonly World world;
		MPos bounds => world.Map.Bounds;
		readonly bool gameSave;

		public static Piece SaveEmpty(MPos size, string name)
		{
			var filepath = PackageManager.Core.PiecesDirectory + name + ".yaml";

			using (var stream = new StreamWriter(File.Create(filepath)))
			{
				stream.WriteLine("MapFormat=" + Constants.CurrentMapFormat);
				stream.WriteLine("Name=" + name);
				stream.WriteLine("Size=" + size);

				var terrain = string.Join(",", Enumerable.Repeat("0", size.X * size.Y));
				stream.WriteLine("Terrain=" + terrain);
			}

			// Load piece into cache, overwrite the old if there is one.
			return PieceManager.LoadPiece(new PackageFile(PackageManager.Core, name), filepath);
		}

		public static void SaveWorld(World world, string directory, string name, bool gameSave = false)
		{
			var saver = new PieceSaver(world, gameSave);
			saver.save(directory, name);
		}

		PieceSaver(World world, bool gameSave)
		{
			this.world = world;
			this.gameSave = gameSave;
		}

		void save(string directory, string name)
		{
			using var writer = new StreamWriter(directory + name + ".yaml", false);

			writer.WriteLine("MapFormat=" + Constants.CurrentMapFormat);
			writer.WriteLine("Name=" + name);
			writer.WriteLine("Size=" + bounds);

			writeTerrainLayer(writer);
			writeWallLayer(writer);

			writeActorLayer(writer);
			writeWeaponLayer(writer);
			writeParticleLayer(writer);

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
			if (world.WallLayer.WallList.Count == 0)
				return;

			writer.WriteLine("Walls=");

			var i = 0u;
			foreach (var wall in world.WallLayer.WallList)
			{
				writer.WriteLine($"\t{i++}=");
				writer.WriteLine($"\t\tType={wall.Type.ID}");
				writer.WriteLine($"\t\tPosition={wall.LayerPosition}");
				if (wall.Health != 0)
					writer.WriteLine($"\t\tHealth={wall.Health}");
			}
		}

		void writeActorLayer(StreamWriter writer)
		{
			if (world.ActorLayer.Actors.Count == 0)
				return;

			writer.WriteLine("Actors=");

			var i = 0u;
			foreach (var a in world.ActorLayer.Actors)
			{
				writer.WriteLine("\t" + (gameSave ? a.ID : i++) + "=");

				foreach (var node in a.Save())
					writer.WriteLine("\t\t" + node);
			}
		}

		void writeWeaponLayer(StreamWriter writer)
		{
			if (!gameSave || world.WeaponLayer.Weapons.Count == 0)
				return;

			writer.WriteLine("Weapons=");

			var i = 0;
			foreach(var weapon in world.WeaponLayer.Weapons)
			{
				writer.WriteLine("\t" + i++ + "=");

				foreach(var node in weapon.Save())
					writer.WriteLine("\t\t" + node);
			}
		}

		void writeParticleLayer(StreamWriter writer)
		{
			if (!gameSave || world.ParticleLayer.Particles.Count == 0)
				return;

			writer.WriteLine("Particles=");

			var i = 0;
			foreach (var particle in world.ParticleLayer.Particles)
			{
				writer.WriteLine("\t" + i++ + "=");

				foreach (var node in particle.Save())
					writer.WriteLine("\t\t" + node);
			}
		}
	}
}
