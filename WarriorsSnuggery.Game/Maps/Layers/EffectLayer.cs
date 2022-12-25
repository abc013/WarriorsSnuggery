using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps.Layers
{
	public class EffectLayer
	{
		public readonly List<PositionableObject> Effects = new List<PositionableObject>();
		readonly List<PositionableObject> effectsToAdd = new List<PositionableObject>();
		readonly List<PositionableObject> effectsToRemove = new List<PositionableObject>();

		public EffectLayer() { }

		public void Add(PositionableObject obj)
		{
			effectsToAdd.Add(obj);
		}

		public void Remove(PositionableObject obj)
		{
			effectsToRemove.Remove(obj);
		}

		public void Tick()
		{
			if (effectsToAdd.Count != 0)
			{
				Effects.AddRange(effectsToAdd);
				effectsToAdd.Clear();
			}

			foreach (var effect in Effects)
			{
				effect.Tick();

				if (effect.Disposed)
					effectsToRemove.Add(effect);
			}

			if (effectsToRemove.Count != 0)
			{
				foreach (var effect in effectsToRemove)
					Effects.Remove(effect);
				effectsToRemove.Clear();
			}
		}

		public void Render()
		{
			foreach (var effect in Effects)
				effect.Render();
		}
	}
}
