using System;

namespace WarriorsSnuggery
{
	interface IPositionable
	{
		CPos Position { get; set; }

		float Scale { get; set; }
		CPos Rotation { get; set; }
	}
}
