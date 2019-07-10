namespace WarriorsSnuggery
{
	public class TableCell
	{
		public static TableCell Head = new TableCell("Name", "Type", new[] { "Description" });

		public readonly string Name;
		public readonly string Type;
		public readonly string[] Desc;

		public TableCell(string name, string type, string[] desc)
		{
			Name = name;
			Type = type;
			Desc = desc;
		}
	}
}
