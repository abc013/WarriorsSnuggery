namespace WarriorsSnuggery.Maps
{
	public enum WaypointType
	{
		END,
		PASSAGE
	}

	public enum WaypointLocation
	{
		TOP,
		LEFT,
		BOTTOM,
		RIGHT,
		CENTER
	}

	public struct Waypoint
	{
		public readonly MPos Position;
		public readonly WaypointLocation Location;

		public readonly WaypointType Type;

		public Waypoint(MPos position, WaypointLocation location, WaypointType type)
		{
			Position = position;
			Location = location;

			Type = type;
		}
	}
}
