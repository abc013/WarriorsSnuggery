namespace WarriorsSnuggery
{
	interface IPositionable
	{
		CPos Position { get; set; }

		float Scale { get; set; }
		VAngle Rotation { get; set; }
	}
}
