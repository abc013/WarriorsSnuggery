using System.IO;
using System.Linq;
using System.Text;

namespace WarriorsSnuggery.Maps.Pieces
{
	public class PieceSaver
	{
		readonly World world;
		MPos bounds => world.Map.Bounds;
		readonly bool gameSave;

		public static Piece SaveEmpty(MPos size, string directory, string name)
		{
			using (var stream = new StreamWriter(File.Create(directory + name + ".yaml")))
			{
				stream.WriteLine("MapFormat=" + Piece.CurrentMapFormat);
				stream.WriteLine("Name=" + name);
				stream.WriteLine("Size=" + size);

				var terrain = string.Join(",", Enumerable.Repeat("0", size.X * size.Y));
				stream.WriteLine("Terrain=" + terrain);

				var walls = string.Join(",", Enumerable.Repeat("-1", (size.X + 1) * (size.Y + 1) * 2 * 2));
				stream.WriteLine("Walls=" + walls);
			}

			// Load piece into cache, overwrite the old if there is one.
			return PieceManager.ReloadPiece(name);
		}

		public static Piece SaveWorld(World world, string directory, string name, bool gameSave = false)
		{
			var saver = new PieceSaver(world, gameSave);

			saver.save(directory, name);

			// If no gameSave, load piece into cache, overwrite the old if there is one.
			return gameSave ? null : PieceManager.ReloadPiece(name);
		}

		PieceSaver(World world, bool gameSave)
		{
			this.world = world;
			this.gameSave = gameSave;
		}

		void save(string directory, string name)
		{
			using var writer = new StreamWriter(directory + name + ".yaml", false);

			writer.WriteLine("MapFormat=" + Piece.CurrentMapFormat);
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
