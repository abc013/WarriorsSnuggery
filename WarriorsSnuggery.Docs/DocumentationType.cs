namespace WarriorsSnuggery.Docs
{
	public enum DocumentationType
	{
		ACTORS,
		PARTICLES,
		WEAPONS,
		TERRAIN,
		WALLS,
		MAPS,
		SPELLS,
		TROPHIES,
		SOUNDS,
		TEXTURES
	}

	static class DocumentationTypeExts
	{
		public static string GetName(this DocumentationType type)
		{
			var name = type.ToString();
			return name[..1] + name[1..].ToLower();
		}
	}
}
