using System.Collections.Generic;
using WarriorsSnuggery.Maps.Noises;

namespace WarriorsSnuggery.Maps
{
	public class MapDebugInformation
	{
		public readonly Dictionary<int, NoiseMap> NoiseMaps;
		public readonly List<Waypoint> Waypoints;
		public readonly List<MPos> PatrolLocations;

		public MapDebugInformation(Dictionary<int, NoiseMap> noiseMaps, List<Waypoint> waypoints, List<MPos> patrolLocations)
		{
			NoiseMaps = noiseMaps;
			Waypoints = waypoints;
			PatrolLocations = patrolLocations;
		}
	}
}