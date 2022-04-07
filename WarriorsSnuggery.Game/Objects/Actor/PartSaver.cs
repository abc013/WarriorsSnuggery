using System.Collections.Generic;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery.Objects.Actors
{
	public class PartSaver
	{
		readonly ActorPart part;
		readonly string internalName;
		readonly List<(string, object)> values = new List<(string, object)>();

		public PartSaver(ActorPart part, string internalName)
		{
			this.part = part;
			this.internalName = internalName;
		}

		public void Add(string name, object value, object defaultValue)
		{
			if (value.Equals(defaultValue))
				return;

			values.Add((name, value));
		}

		public string[] GetSave()
		{
			if (values.Count == 0)
				return new string[0];

			var save = new string[values.Count + 1];

			save[0] = $"{part.GetType().Name}@{internalName}=";
			for (int i = 0; i < values.Count; i++)
				save[i + 1] = $"\t{values[i].Item1}={values[i].Item2}";

			return save;
		}
	}
}
