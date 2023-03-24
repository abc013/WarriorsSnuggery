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
			var saver = new TextNodeSaver();
			saver.Add("MapFormat", Constants.CurrentMapFormat);
			saver.Add("Name", name);
			saver.Add("Size", bounds);

			saver.Append(world.TerrainLayer.Save());
			saver.AddChildren("Walls", world.WallLayer.Save(), true);
			saver.AddChildren("Actors", world.ActorLayer.Save(gameSave), true);

			if (gameSave)
			{
				saver.AddChildren("Weapons", world.WeaponLayer.Save(), true);
				saver.AddChildren("Particles", world.ParticleLayer.Save(), true);
			}

			using var writer = new StreamWriter(directory + name + ".yaml", false);

			foreach (var savedString in saver.GetStrings())
				writer.WriteLine(savedString);

			writer.Flush();
			writer.Close();
		}
	}
}
