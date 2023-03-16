using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;

namespace WarriorsSnuggery.Objects.Weather
{
	public class WeatherManager : ITick
	{
		internal static MPos cameraMaxBounds;
		internal static int cameraLeft, cameraRight;
		internal static int cameraTop, cameraBottom;

		readonly WeatherEffectController[] controllers;

		public WeatherManager(World world, MapType type)
		{
			controllers = new WeatherEffectController[type.WeatherEffects.Length];

			for (int i = 0; i < controllers.Length; i++)
				controllers[i] = new WeatherEffectController(world, type.WeatherEffects[i]);

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

			foreach (var controller in controllers)
				controller.Tick();
		}

		public void Render()
        {
			foreach (var controller in controllers)
				controller.Render();
		}
	}
}
