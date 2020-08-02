using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery.Objects.Actors
{
	public class PartSaver
	{
		readonly ActorPart part;
		readonly bool mustSave;
		readonly List<(string, object)> values = new List<(string, object)>();

		public PartSaver(ActorPart part, bool mustSave = false)
		{
			this.part = part;
			this.mustSave = mustSave;
		}

		public void Add(string name, object value, object defaultValue)
		{
			if (value == defaultValue)
				return;

			values.Add((name, value));
		}

		public string[] GetSave()
		{
			if (!values.Any() && !mustSave)
				return new string[0];

			var save = new string[values.Count + 1];

			save[0] = part.GetType().Name + "=";
			for (int i = 0; i < values.Count; i++)
				save[i + 1] = "\t" + values[i].Item1 + "=" + values[i].Item2;

			return save;
		}
	}
}
