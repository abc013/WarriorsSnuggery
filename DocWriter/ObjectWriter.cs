using System;
using System.Collections.Generic;
using WarriorsSnuggery;

namespace DocWriter
{
	static class ObjectWriter
	{
		static readonly List<MiniTextNode> emptyTextNodes = new List<MiniTextNode>();

		public static void WriteActors()
		{
			TypeWriter.WriteAll("WarriorsSnuggery.Objects.Parts", "PartInfo", new object[] { string.Empty, emptyTextNodes });
		}

		public static void WriteParticles()
		{
			HTMLWriter.WriteHeader("ParticleSpawners");
			TypeWriter.WriteAll("WarriorsSnuggery.Objects.Particles", "ParticleSpawner", new[] { emptyTextNodes });

			HTMLWriter.WriteHeader("Particle");
			TypeWriter.Write(typeof(WarriorsSnuggery.Objects.Particles.ParticleType), new[] { emptyTextNodes });
		}

		public static void WriteTerrain()
		{
			TypeWriter.Write(typeof(WarriorsSnuggery.Objects.TerrainType), new object[] { (ushort)0, emptyTextNodes, true });
		}

		public static void WriteWalls()
		{
			TypeWriter.Write(typeof(WarriorsSnuggery.Objects.WallType), new object[] { (short)0, emptyTextNodes, true });
		}

		public static void WriteWeapons()
		{
			HTMLWriter.WriteHeader("WeaponType");
			TypeWriter.Write(typeof(WarriorsSnuggery.Objects.Weapons.WeaponType), new[] { emptyTextNodes });

			HTMLWriter.WriteHeader("ProjectileTypes");
			TypeWriter.WriteAll("WarriorsSnuggery.Objects.Weapons", "ProjectileType", new[] { emptyTextNodes });

			HTMLWriter.WriteHeader("Warheads");
			TypeWriter.WriteAll("WarriorsSnuggery.Objects.Weapons", "Warhead", new[] { emptyTextNodes });
		}

		public static void WriteMaps()
		{
			HTMLWriter.WriteHeader("MapType");
			TypeWriter.Write(typeof(WarriorsSnuggery.Maps.MapType), Array.Empty<object>());

			HTMLWriter.WriteHeader("NoiseMap");
			TypeWriter.Write(typeof(WarriorsSnuggery.Maps.NoiseMapInfo), new object[] { -1, emptyTextNodes });

			HTMLWriter.WriteHeader("Generators");
			TypeWriter.WriteAll("WarriorsSnuggery.Maps.Generators", "GeneratorInfo", new object[] { -1, emptyTextNodes });
		}

		public static void WriteSpells()
		{
			HTMLWriter.WriteHeader("SpellNode");
			TypeWriter.Write(typeof(WarriorsSnuggery.Spells.SpellTreeNode), new object[] { emptyTextNodes, "", true });

			HTMLWriter.WriteHeader("Spell");
			TypeWriter.Write(typeof(WarriorsSnuggery.Spells.Spell), new[] { emptyTextNodes });

			HTMLWriter.WriteHeader("Effect");
			TypeWriter.Write(typeof(WarriorsSnuggery.Spells.Effect), new[] { emptyTextNodes });
		}

		public static void WriteTrophies()
		{
			HTMLWriter.WriteHeader("Trophy");
			TypeWriter.Write(typeof(WarriorsSnuggery.Trophies.Trophy), new object[] { emptyTextNodes });
		}

		public static void WriteSounds()
		{
			HTMLWriter.WriteHeader("Sound");
			TypeWriter.Write(typeof(SoundType), new object[] { emptyTextNodes, true });
		}
	}
}
