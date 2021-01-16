namespace DocWriter
{
	public class TableCell
	{
		public static readonly TableCell Head = new TableCell("Name", "Type", new[] { "Description" }, "Default Value");

		public readonly string Name;
		public readonly string Type;
		public readonly string[] Desc;
		public readonly string Value;

		public TableCell(string name, string type, string[] desc, string value)
		{
			Name = name;
			Type = type;
			Desc = desc;
			Value = value;
		}
	}
}
