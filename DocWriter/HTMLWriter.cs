using System;
using System.IO;

namespace WarriorsSnuggery
{
	public static class HTMLWriter
	{
		public static readonly string[] LightModeColors = new[]
		{
			"#EEEEEE",
			"#DDDDDD",
			"#CCCCCC"
		};
		public static readonly string[] DarkModeColors = new[]
		{
			"#111111",
			"#222222",
			"#333333"
		};
		public static string[] Colors;

		public static void WriteHead(StreamWriter writer)
		{
			writer.WriteLine("<html>");
			writer.WriteLine("\t<head>");
			writer.WriteLine("\t\t<title>Rule Documentation</title>");
			writeStyle(writer);
			writer.WriteLine("\t</head>");
			writer.WriteLine("\t<body>");
		}

		static void writeStyle(StreamWriter writer)
		{
			writer.WriteLine("\t\t<style>");
			if (Program.DarkMode)
			{
				writer.WriteLine("body { background-color: #000000; color: #BBBBBB; }");
				writer.WriteLine("table { margin: 10px; box-shadow: 1px 0px 10px " + Colors[0] + "; width: 70%; border-collapse: collapse; }");
				writer.WriteLine("td { border: 1px solid " + Colors[2] + "; padding: 8px; }");
				writer.WriteLine("th { border: 1px solid " + Colors[2] + "; padding: 8px; color: #DDDD55; }");
				writer.WriteLine("tr:nth-child(even) { background-color: " + Colors[0] + "; }");
				writer.WriteLine("tr { background-color: " + Colors[1] + "; color: #DDDDDD; }");
				writer.WriteLine("hr { color: " + Colors[2] + "; }");
				writer.WriteLine("h1 { margin-bottom: 0px; color: #FFFFFF; }");
				writer.WriteLine("h2 { margin-bottom: 0px; color: #FFFFFF; }");
				writer.WriteLine("h3 { margin-bottom: 0px; color: #EEEEEE; }");
			}
			else
			{
				writer.WriteLine("table { margin: 10px; box-shadow: 1px 0px 10px " + Colors[0] + "; width: 70%; border-collapse: collapse; }");
				writer.WriteLine("td { border: 1px solid " + Colors[1] + "; padding: 8px; }");
				writer.WriteLine("th { border: 1px solid " + Colors[1] + "; padding: 8px; }");
				writer.WriteLine("tr:nth-child(even) { background-color: " + Colors[0] + "; }");
				writer.WriteLine("h1 { margin-bottom: 0px; }");
				writer.WriteLine("h2 { margin-bottom: 0px; }");
				writer.WriteLine("h3 { margin-bottom: 0px; }");
			}
			writer.WriteLine("\t\t</style>");
		}

		public static void WriteDoc(StreamWriter writer, DocumentationType type)
		{
			TypeWriter.Initialize();
			writer.WriteLine();
			writer.WriteLine("\t\t<h1>" + type.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture) + "</h1>");
			writer.WriteLine("\t\t<hr>");
			writer.WriteLine();

			switch (type)
			{
				case DocumentationType.ACTORS:
					ObjectWriter.WriteActors(writer);

					break;
				case DocumentationType.PARTICLES:
					ObjectWriter.WriteParticles(writer);

					break;
				case DocumentationType.TERRAIN:
					ObjectWriter.WriteTerrain(writer);

					break;
				case DocumentationType.WALLS:
					ObjectWriter.WriteWalls(writer);

					break;
				case DocumentationType.WEAPONS:
					ObjectWriter.WriteWeapons(writer);

					break;
				case DocumentationType.MAPS:
					ObjectWriter.WriteMaps(writer);

					break;
				case DocumentationType.SPELLS:
					ObjectWriter.WriteSpells(writer);
					break;
			}
		}

		public static void WriteRuleHead(StreamWriter writer, string rule, string[] description, bool h2 = true)
		{
			writer.WriteLine("\t\t<" + (h2 ? "h2" : "h3") + ">" + rule + "</h2>");
			writer.WriteLine("\t\t<hr color=\"" + Colors[2] + "\">");
			foreach (var descLine in description)
				writer.WriteLine("\t\t" + descLine + "<br><br>");
			writer.WriteLine();
		}

		public static void WriteHead(StreamWriter writer, string head)
		{
			writer.WriteLine("\t\t<h2>" + head + "</h2>");
			writer.WriteLine("\t\t<hr>");
		}

		public static void WriteTable(StreamWriter writer, TableCell[] cells, bool showValues)
		{
			writer.WriteLine("\t\t<table>");

			writeCell(writer, TableCell.Head, showValues, true);
			foreach (var cell in cells)
				writeCell(writer, cell, showValues);

			writer.WriteLine("\t\t</table>");
		}

		static void writeCell(StreamWriter writer, TableCell cell, bool showValues, bool head = false)
		{
			var style = "td";
			if (head)
				style = "th style=\"background-color: " + Colors[1] + ";\" ";

			writer.WriteLine();
			writer.WriteLine("\t\t\t<tr>");

			writer.WriteLine("\t\t\t\t<" + style + ">" + cell.Name + "</td>");

			writer.WriteLine("\t\t\t\t<" + style + ">" + cell.Type + "</td>");

			var desc = "";
			foreach (var desc1 in cell.Desc)
				desc += desc1 + "<br>";
			writer.WriteLine("\t\t\t\t<" + style + ">" + desc + "</td>");

			if (showValues)
				writer.WriteLine("\t\t\t\t<" + style + ">" + cell.Value + "</td>");

			writer.WriteLine("\t\t\t</tr>");
		}

		public static void WriteEnd(StreamWriter writer)
		{
			writer.WriteLine("\t<p>Warrior's Snuggery Rules Document. Generated for " + Settings.Version + " at " + DateTime.Now + ".</p>");
			writer.WriteLine("\t</body>");
			writer.WriteLine("</html>");
		}
	}

	static class ObjectWriter
	{
		public static void WriteActors(StreamWriter writer)
		{
			TypeWriter.WriteAll(writer, "WarriorsSnuggery.Objects.Parts", "PartInfo", new object[] { string.Empty, new MiniTextNode[0] });
		}

		public static void WriteParticles(StreamWriter writer)
		{
			HTMLWriter.WriteHead(writer, "ParticleSpawner");
			TypeWriter.Write(writer, "WarriorsSnuggery.Objects.Particles.ParticleSpawner", new[] { new MiniTextNode[0] });

			HTMLWriter.WriteHead(writer, "Particle");
			TypeWriter.Write(writer, "WarriorsSnuggery.Objects.Particles.ParticleType", new[] { new MiniTextNode[0] });
		}

		public static void WriteTerrain(StreamWriter writer)
		{
			TypeWriter.Write(writer, "WarriorsSnuggery.Objects.TerrainType", new object[] { (ushort)0, new MiniTextNode[0], true });
		}

		public static void WriteWalls(StreamWriter writer)
		{
			TypeWriter.Write(writer, "WarriorsSnuggery.Objects.WallType", new object[] { 0, new MiniTextNode[0], true });
		}

		public static void WriteWeapons(StreamWriter writer)
		{
			HTMLWriter.WriteHead(writer, "WeaponType");
			TypeWriter.Write(writer, "WarriorsSnuggery.Objects.Weapons.WeaponType", new[] { new MiniTextNode[0] });

			HTMLWriter.WriteHead(writer, "ProjectileTypes");
			TypeWriter.WriteAll(writer, "WarriorsSnuggery.Objects.Weapons", "ProjectileType", new[] { new MiniTextNode[0] });

			HTMLWriter.WriteHead(writer, "Warheads");
			TypeWriter.WriteAll(writer, "WarriorsSnuggery.Objects.Weapons", "Warhead", new[] { new MiniTextNode[0] });
		}

		public static void WriteMaps(StreamWriter writer)
		{
			HTMLWriter.WriteHead(writer, "Map");
			TypeWriter.Write(writer, "WarriorsSnuggery.Maps.MapInfo", new[] { new MiniTextNode[0] });

			HTMLWriter.WriteHead(writer, "MapGenerators");
			TypeWriter.WriteAll(writer, "WarriorsSnuggery.Maps", "GeneratorInfo", new object[] { -1, new MiniTextNode[0] });
		}

		public static void WriteSpells(StreamWriter writer)
		{
			HTMLWriter.WriteHead(writer, "SpellNode");
			TypeWriter.Write(writer, "WarriorsSnuggery.Spells.SpellTreeNode", new object[] { new MiniTextNode[0], "", true });

			HTMLWriter.WriteHead(writer, "Spell");
			TypeWriter.Write(writer, "WarriorsSnuggery.Spells.Spell", new[] { new MiniTextNode[0] });
		}
	}
}
