using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery.Objects.Actors
{
	public class PartSaver
	{
		readonly ActorPart part;
		readonly string internalName;
		readonly TextNodeSaver saver = new TextNodeSaver();

		public PartSaver(ActorPart part, string internalName)
		{
			this.part = part;
			this.internalName = internalName;
		}

		public void Add(string name, object value, object defaultValue)
		{
			saver.Add(name, value, defaultValue);
		}

		public void SaveUsing(TextNodeSaver parentSaver)
		{
			parentSaver.AddChildren($"{part.GetType().Name[..^4]}", internalName, saver, true);
		}
	}
}
