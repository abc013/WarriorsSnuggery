using System;
using System.Collections.Generic;
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
			writer.WriteLine($"\t\t<title>{Program.Title} - Rule Documentation</title>");
			writeStyle(writer);
			writer.WriteLine("\t</head>");
			writer.WriteLine("\t<body>");
			writer.WriteLine("<img src=\"misc/UI/logo.png\"/>");
		}

		static void writeStyle(StreamWriter writer)
		{
			writer.WriteLine("\t\t<style>");
			if (Program.DarkMode)
			{
				writer.WriteLine("body { background-color: #000000; color: #BBBBBB; }");
				writer.WriteLine("table { margin: 10px; box-shadow: 1px 0px 10px " + Colors[0] + "; width: 70%; border-collapse: collapse; }");
				writer.WriteLine("td { border: 1px solid " + Colors[2] + "; padding: 8px; }");
				writer.WriteLine("th { border: 1px solid " + Colors[2] + "; padding: 8px; color: #DDDDFF; }");
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

		public static void WriteIndex(StreamWriter writer, DocumentationType[] types)
		{
			WriteHeader(writer, "Table of Contents", 1);

			writer.WriteLine($"\t\t<ul>");

			int id = 1;
			foreach (var type in types)
			{
				var name = type.ToString();

				writer.WriteLine($"\t\t<li><h4><a href=\"#{id}\">");
				writer.WriteLine($"\t\t\t{id}. {name.Substring(0, 1) + name[1..].ToLower()}");
				writer.WriteLine($"\t\t</a></h4></li>");

				id++;
			}

			writer.WriteLine($"\t\t</ul>");
		}

		public static void WriteDoc(StreamWriter writer, DocumentationType type, int id)
		{
			var name = type.ToString();

			WriteHeader(writer, $"{id}. {name.Substring(0, 1) + name[1..].ToLower()}", 1, id);

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
				case DocumentationType.TROPHIES:
					ObjectWriter.WriteTrophies(writer);

					break;
				case DocumentationType.SOUNDS:
					ObjectWriter.WriteSounds(writer);

					break;
			}
		}

		public static void WriteHeader(StreamWriter writer, string head, int importance = 2, int id = -1)
		{
			var idString = id > 0 ? $" id=\"{id}\"" : string.Empty;
			writer.WriteLine($"\t\t<h{importance}{idString}> {head} </h{importance}>");
			writer.WriteLine("\t\t<hr>");
		}

		public static void WriteDescription(StreamWriter writer, string[] description)
		{
			foreach (var descLine in description)
				writer.WriteLine("\t\t" + descLine + "<br><br>");
			writer.WriteLine();
		}

		public static void WriteTable(StreamWriter writer, List<TableCell> cells, bool showValues)
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
			writer.WriteLine($"\t<p>{Program.Title} Rules Document. Generated for {Settings.Version} at {DateTime.Now}.</p>");
			writer.WriteLine("\t</body>");
			writer.WriteLine("</html>");
		}
	}

	static class ObjectWriter
	{
		public static void WriteActors(StreamWriter writer)
		{
			TypeWriter.WriteAll(writer, "WarriorsSnuggery.Objects.Parts", "PartInfo", new object[] { string.Empty, new List<MiniTextNode>() });
		}

		public static void WriteParticles(StreamWriter writer)
		{
			HTMLWriter.WriteHeader(writer, "ParticleSpawners");
			TypeWriter.WriteAll(writer, "WarriorsSnuggery.Objects.Particles", "ParticleSpawner", new[] { new List<MiniTextNode>() });

			HTMLWriter.WriteHeader(writer, "Particle");
			TypeWriter.Write(writer, "WarriorsSnuggery.Objects.Particles.ParticleType", new[] { new List<MiniTextNode>() });
		}

		public static void WriteTerrain(StreamWriter writer)
		{
			TypeWriter.Write(writer, "WarriorsSnuggery.Objects.TerrainType", new object[] { (ushort)0, new List<MiniTextNode>(), true });
		}

		public static void WriteWalls(StreamWriter writer)
		{
			TypeWriter.Write(writer, "WarriorsSnuggery.Objects.WallType", new object[] { (short)0, new List<MiniTextNode>(), true });
		}

		public static void WriteWeapons(StreamWriter writer)
		{
			HTMLWriter.WriteHeader(writer, "WeaponType");
			TypeWriter.Write(writer, "WarriorsSnuggery.Objects.Weapons.WeaponType", new[] { new List<MiniTextNode>() });

			HTMLWriter.WriteHeader(writer, "ProjectileTypes");
			TypeWriter.WriteAll(writer, "WarriorsSnuggery.Objects.Weapons", "ProjectileType", new[] { new List<MiniTextNode>() });

			HTMLWriter.WriteHeader(writer, "Warheads");
			TypeWriter.WriteAll(writer, "WarriorsSnuggery.Objects.Weapons", "Warhead", new[] { new List<MiniTextNode>() });
		}

		public static void WriteMaps(StreamWriter writer)
		{
			HTMLWriter.WriteHeader(writer, "Map");
			TypeWriter.Write(writer, "WarriorsSnuggery.Maps.MapInfo", Array.Empty<object>());

			HTMLWriter.WriteHeader(writer, "NoiseMap");
			TypeWriter.Write(writer, "WarriorsSnuggery.Maps.NoiseMapInfo", new object[] { -1, new List<MiniTextNode>() });

			HTMLWriter.WriteHeader(writer, "Generators");
			TypeWriter.WriteAll(writer, "WarriorsSnuggery.Maps.Generators", "GeneratorInfo", new object[] { -1, new List<MiniTextNode>() });
		}

		public static void WriteSpells(StreamWriter writer)
		{
			HTMLWriter.WriteHeader(writer, "SpellNode");
			TypeWriter.Write(writer, "WarriorsSnuggery.Spells.SpellTreeNode", new object[] { new List<MiniTextNode>(), "", true });

			HTMLWriter.WriteHeader(writer, "Spell");
			TypeWriter.Write(writer, "WarriorsSnuggery.Spells.Spell", new[] { new List<MiniTextNode>() });
		}

		public static void WriteTrophies(StreamWriter writer)
		{
			HTMLWriter.WriteHeader(writer, "Trophy");
			TypeWriter.Write(writer, "WarriorsSnuggery.Trophies.Trophy", new object[] { new List<MiniTextNode>() });
		}

		public static void WriteSounds(StreamWriter writer)
		{
			HTMLWriter.WriteHeader(writer, "Sound");
			TypeWriter.Write(writer, "WarriorsSnuggery.SoundType", new object[] { new List<MiniTextNode>(), true });
		}
	}
}
