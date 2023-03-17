using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WarriorsSnuggery.Audio.Sound;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery.Docs
{
	public class ChapterWriter
	{
		static readonly List<TextNode> emptyTextNodes = new List<TextNode>();

		readonly MemoryStream stream = new MemoryStream();
		readonly StreamWriter writer;

		readonly DocumentationType chapter;

		public ChapterWriter(DocumentationType chapter, int number)
		{
			this.chapter = chapter;

			writer = new StreamWriter(stream);

			writer.WriteLine(DocumentationUtils.Header($"{number}. {chapter.GetName()}", 1));
		}

		public void WriteDocumentation()
		{
			typeof(ChapterWriter).GetMethod($"write{chapter.GetName()}", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, null);
		}

#pragma warning disable IDE0051 // Unused private members, not valid because of System.Reflection
		void writeActors()
		{
			writer.WriteLine(DocumentationUtils.Header("Parts", 2));
			writer.WriteLine(TypeWriter.WriteAll("WarriorsSnuggery.Objects.Actors.Parts", "PartInfo", new object[] { new PartInitSet(string.Empty, emptyTextNodes) }));

			writer.WriteLine(DocumentationUtils.Header("SimplePhysics", 2));
			writer.WriteLine(TypeWriter.Write(typeof(Physics.SimplePhysicsType), new[] { emptyTextNodes }));

			writer.WriteLine(DocumentationUtils.Header("BotBehaviors", 2));
			writer.WriteLine(TypeWriter.WriteAll("WarriorsSnuggery.Objects.Actors.Bot", "BotBehaviorType", new[] { emptyTextNodes }));
		}

		void writeParticles()
		{
			writer.WriteLine(DocumentationUtils.Header("ParticleSpawners", 2));
			writer.WriteLine(TypeWriter.WriteAll("WarriorsSnuggery.Objects.Particles", "ParticleSpawner", new[] { emptyTextNodes }));

			writer.WriteLine(DocumentationUtils.Header("Particle", 2));
			writer.WriteLine(TypeWriter.Write(typeof(Objects.Particles.ParticleType), new[] { emptyTextNodes }));
		}

		void writeTerrain()
		{
			writer.WriteLine(TypeWriter.Write(typeof(Objects.TerrainType), new object[] { (ushort)0, emptyTextNodes, true }));
		}

		void writeWalls()
		{
			writer.WriteLine(TypeWriter.Write(typeof(Objects.WallType), new object[] { (short)0, emptyTextNodes, true }));
		}

		void writeWeapons()
		{
			writer.WriteLine(DocumentationUtils.Header("WeaponType", 2));
			writer.WriteLine(TypeWriter.Write(typeof(Objects.Weapons.WeaponType), new[] { emptyTextNodes }));

			writer.WriteLine(DocumentationUtils.Header("Projectiles", 2));
			writer.WriteLine(TypeWriter.WriteAll("WarriorsSnuggery.Objects.Weapons.Projectiles", "Projectile", new[] { emptyTextNodes }));

			writer.WriteLine(DocumentationUtils.Header("Warheads", 2));
			writer.WriteLine(TypeWriter.WriteAll("WarriorsSnuggery.Objects.Weapons.Warheads", "Warhead", new[] { emptyTextNodes }));
		}

		void writeMaps()
		{
			writer.WriteLine(DocumentationUtils.Header("MapType", 2));
			writer.WriteLine(TypeWriter.Write(typeof(Maps.MapType), Array.Empty<object>()));

			writer.WriteLine(DocumentationUtils.Header("NoiseMap", 2));
			writer.WriteLine(TypeWriter.Write(typeof(Maps.Noises.NoiseMapInfo), new object[] { -1, emptyTextNodes }));

			writer.WriteLine(DocumentationUtils.Header("Generators", 2));
			writer.WriteLine(TypeWriter.WriteAll("WarriorsSnuggery.Maps.Generators", "GeneratorInfo", new object[] { -1, emptyTextNodes }));

			writer.WriteLine(DocumentationUtils.Header("WeatherEffect", 2));
			writer.WriteLine(TypeWriter.Write(typeof(Objects.Weather.WeatherEffect), new object[] { emptyTextNodes }));
		}

		void writeSpells()
		{
			writer.WriteLine(DocumentationUtils.Header("SpellCaster", 2));
			writer.WriteLine(TypeWriter.Write(typeof(Spells.SpellCasterType), new object[] { "", emptyTextNodes }));

			writer.WriteLine(DocumentationUtils.Header("Effect", 2));
			writer.WriteLine(TypeWriter.Write(typeof(Spells.Effect), new[] { emptyTextNodes }));
		}

		void writeTrophies()
		{
			writer.WriteLine(TypeWriter.Write(typeof(Trophies.Trophy), new object[] { emptyTextNodes }));
		}

		void writeSounds()
		{
			writer.WriteLine(TypeWriter.Write(typeof(SoundType), new object[] { emptyTextNodes, true }));
		}

		void writeTextures()
		{
			writer.WriteLine(TypeWriter.Write(typeof(TextureInfo), new object[] { emptyTextNodes }));
		}
#pragma warning restore IDE0051

		public string GetResult()
		{
			writer.Flush();
			stream.Seek(0, SeekOrigin.Begin);
			return new StreamReader(stream).ReadToEnd();
		}
	}
}
