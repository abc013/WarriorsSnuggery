namespace DocWriter
{
	public enum DocumentationType
	{
		ALL,
		ACTORS,
		PARTICLES,
		WEAPONS,
		TERRAIN,
		WALLS,
		MAPS,
		SPELLS,
		TROPHIES,
		SOUNDS
	}

	static class DocumentationTypeExts
	{
		public static string GetName(this DocumentationType type)
		{
			var name = type.ToString();
			return name.Substring(0, 1) + name[1..].ToLower();
		}
	}
}
