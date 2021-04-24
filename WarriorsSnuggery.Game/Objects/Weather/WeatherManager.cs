using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;

namespace WarriorsSnuggery.Objects.Weather
{
	public class WeatherManager : ITick
	{
		internal static MPos cameraMaxBounds;
		internal static int cameraLeft, cameraRight;
		internal static int cameraTop, cameraBottom;

		public readonly WeatherEffectController[] Controllers;

		public WeatherManager(World world, MapType type)
		{
			Controllers = new WeatherEffectController[type.WeatherEffects.Length];

			for (int i = 0; i < Controllers.Length; i++)
				Controllers[i] = new WeatherEffectController(world, type.WeatherEffects[i]);

			cameraMaxBounds = new MPos((int)(1024 * Camera.MaxZoom * WindowInfo.Ratio), (int)(1024 * Camera.MaxZoom));
		}

		public void Tick()
		{
			if (!Settings.EnableWeatherEffects)
				return;

			cameraLeft = Camera.LookAt.X - cameraMaxBounds.X / 2;
			cameraRight = Camera.LookAt.X + cameraMaxBounds.X / 2;

			cameraTop = Camera.LookAt.Y - cameraMaxBounds.Y / 2;
			cameraBottom = Camera.LookAt.Y + cameraMaxBounds.Y / 2;

			for (int i = 0; i < Controllers.Length; i++)
				Controllers[i].Tick();
		}
	}
}
