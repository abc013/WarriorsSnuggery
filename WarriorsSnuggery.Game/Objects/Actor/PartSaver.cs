using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery.Objects.Actors
{
	public class PartSaver : TextNodeSaver
	{
		readonly ActorPart part;

		public PartSaver(ActorPart part)
		{
			this.part = part;
		}

		public void SaveUsing(TextNodeSaver parentSaver)
		{
			parentSaver.AddChildren($"{part.GetType().Name[..^4]}", part.Specification, this, true);
		}
	}
}
