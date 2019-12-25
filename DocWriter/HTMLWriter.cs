using System;
using System.IO;
using System.Linq;
using System.Reflection;

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
					writer.WriteLine("\t\t<h2> Map Information</h2>");
					writer.WriteLine("\t\t<hr>");
					writer.WriteLine();
					ObjectWriter.WriteMaps(writer);
					writer.WriteLine();
					writer.WriteLine("\t\t<h2> Map Generators</h2>");
					writer.WriteLine("\t\t<hr>");
					writer.WriteLine();
					ObjectWriter.WriteMapGenerators(writer);

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
			var infos = Assembly.Load("WarriorsSnuggery").GetTypes().Where(t => t.Name.EndsWith("Info") && t.Namespace == "WarriorsSnuggery.Objects.Parts" && !t.IsAbstract);

			bool first = true;
			foreach (var info in infos)
			{
				var attrib = info.GetCustomAttribute(typeof(DescAttribute));
				HTMLWriter.WriteRuleHead(writer, info.Name.Replace("PartInfo", ""), attrib == null ? new string[] { "No Description." } : ((DescAttribute)attrib).Desc);

				writeTypeAndValues(writer, info, new[] { new MiniTextNode[0] });

				Console.Write((first ? "" : ", ") + info.Name.Replace("PartInfo", ""));
				if (first)
					first = false;
			}
			Console.WriteLine();
		}

		public static void WriteParticles(StreamWriter writer)
		{
			writer.WriteLine("\t\t<h2>ParticleSpawner</h2>");
			writer.WriteLine("\t\t<hr>");
			var spawner = Assembly.Load("WarriorsSnuggery").GetType("WarriorsSnuggery.Objects.Particles.ParticleSpawner");
			writeTypeAndValues(writer, spawner, new[] { new MiniTextNode[0] });

			writer.WriteLine("\t\t<h2>Particle</h2>");
			writer.WriteLine("\t\t<hr>");
			var info = Assembly.Load("WarriorsSnuggery").GetType("WarriorsSnuggery.Objects.Particles.ParticleType");
			writeTypeAndValues(writer, info, new[] { new MiniTextNode[0] });
		}

		public static void WriteTerrain(StreamWriter writer)
		{
			var info = Assembly.Load("WarriorsSnuggery").GetType("WarriorsSnuggery.Objects.TerrainType");

			writeTypeAndValues(writer, info, new object[] { ushort.MaxValue, new MiniTextNode[0] });
		}

		public static void WriteWalls(StreamWriter writer)
		{
			var info = Assembly.Load("WarriorsSnuggery").GetType("WarriorsSnuggery.Objects.WallType");

			writeTypeAndValues(writer, info, new object[] { -1, new MiniTextNode[0] });
		}

		public static void WriteWeapons(StreamWriter writer)
		{
			writer.WriteLine("\t\t<h2>WeaponType</h2>");
			writer.WriteLine("\t\t<hr>");
			var weaponType = Assembly.Load("WarriorsSnuggery").GetType("WarriorsSnuggery.Objects.Weapons.WeaponType");
			writeTypeAndValues(writer, weaponType, new object[] { new MiniTextNode[0] });

			writer.WriteLine("\t\t<h2>ProjectileType</h2>");
			writer.WriteLine("\t\t<hr>");
			var infos = Assembly.Load("WarriorsSnuggery").GetTypes().Where(t => t.Name.EndsWith("ProjectileType") && t.Namespace == "WarriorsSnuggery.Objects.Weapons" && !t.IsInterface);

			bool first = true;
			foreach (var info in infos)
			{
				var attrib = info.GetCustomAttribute(typeof(DescAttribute));
				HTMLWriter.WriteRuleHead(writer, info.Name.Replace("ProjectileType", ""), attrib == null ? new string[] { "No Description." } : ((DescAttribute)attrib).Desc, false);

				writeTypeAndValues(writer, info, new object[] { new MiniTextNode[0] });

				Console.Write((first ? "" : ", ") + info.Name.Replace("ProjectileType", ""));
				first = false;
			}
			Console.WriteLine();

			writer.WriteLine("\t\t<h2>Warhead</h2>");
			writer.WriteLine("\t\t<hr>");
			var infos2 = Assembly.Load("WarriorsSnuggery").GetTypes().Where(t => t.Name.EndsWith("Warhead") && t.Namespace == "WarriorsSnuggery.Objects.Weapons" && !t.IsInterface);
			first = true;
			foreach (var info in infos2)
			{
				var attrib = info.GetCustomAttribute(typeof(DescAttribute));
				HTMLWriter.WriteRuleHead(writer, info.Name.Replace("Warhead", ""), attrib == null ? new string[] { "No Description." } : ((DescAttribute)attrib).Desc, false);

				writeTypeAndValues(writer, info, new object[] { new MiniTextNode[0] });

				Console.Write((first ? "" : ", ") + info.Name.Replace("Warhead", ""));
				first = false;
			}
			Console.WriteLine();
		}

		public static void WriteMaps(StreamWriter writer)
		{
			var info = Assembly.Load("WarriorsSnuggery").GetType("WarriorsSnuggery.Maps.MapInfo");

			writeType(writer, info);
		}

		public static void WriteMapGenerators(StreamWriter writer)
		{
			var infos = Assembly.Load("WarriorsSnuggery").GetTypes().Where(t => t.Name.EndsWith("Info") && t.Namespace == "WarriorsSnuggery.Maps" && !t.IsAbstract);

			bool first = true;
			foreach (var info in infos)
			{
				if (info.Name == "MapInfo")
					continue;

				var attrib = info.GetCustomAttribute(typeof(DescAttribute));
				HTMLWriter.WriteRuleHead(writer, info.Name.Replace("Info", ""), attrib == null ? new string[] { "No Description." } : ((DescAttribute)attrib).Desc);

				writeType(writer, info);

				Console.Write((first ? "" : ", ") + info.Name.Replace("Info", ""));
				first = false;
			}
			Console.WriteLine();
		}

		public static void WriteSpells(StreamWriter writer)
		{
			writer.WriteLine("\t\t<h2> Spell basic information</h2>");
			writer.WriteLine("\t\t<hr>");
			writer.WriteLine();
			var info = Assembly.Load("WarriorsSnuggery").GetType("WarriorsSnuggery.Spells.SpellTreeNode");
			writeType(writer, info);

			writer.WriteLine("\t\t<h2> Spell</h2>");
			writer.WriteLine("\t\t<hr>");
			writer.WriteLine();
			var info2 = Assembly.Load("WarriorsSnuggery").GetType("WarriorsSnuggery.Spells.Spell");
			writeType(writer, info2);
		}

		static void writeType(StreamWriter writer, Type info)
		{
			var variables = info.GetFields().Where(f => f.IsInitOnly && f.GetCustomAttribute(typeof(DescAttribute)) != null).ToArray();
			var cells = new TableCell[variables.Length];
			for (int i = 0; i < variables.Length; i++)
			{
				var variable = variables[i];
				var name = variable.Name;
				var type = getNameOfType(variable.FieldType.Name);
				var desc = ((DescAttribute)variable.GetCustomAttribute(typeof(DescAttribute))).Desc;
				cells[i] = new TableCell(name, type, desc, "Not given");
			}
			HTMLWriter.WriteTable(writer, cells, false);
		}

		static void writeTypeAndValues(StreamWriter writer, Type info, object[] args)
		{
			var obj = Activator.CreateInstance(info, args);
			var variables = info.GetFields().Where(f => f.IsInitOnly && f.GetCustomAttribute(typeof(DescAttribute)) != null).ToArray();
			var cells = new TableCell[variables.Length];
			for (int i = 0; i < variables.Length; i++)
			{
				var variable = variables[i];
				var name = variable.Name;
				var type = getNameOfType(variable.FieldType.Name);
				var desc = ((DescAttribute)variable.GetCustomAttribute(typeof(DescAttribute))).Desc;
				var value = variable.GetValue(obj);
				cells[i] = new TableCell(name, type, desc, value == null ? "" : value.ToString());
			}
			HTMLWriter.WriteTable(writer, cells, true);
		}

		static string getNameOfType(string name)
		{
			name = name.Replace("Single", "Float");
			name = name.Replace("Int32", "Integer");

			return name;
		}
	}
}
