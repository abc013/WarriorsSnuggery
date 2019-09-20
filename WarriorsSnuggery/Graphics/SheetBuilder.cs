using System;
using System.Runtime.InteropServices;

namespace WarriorsSnuggery.Graphics
{
	public static class SheetBuilder
	{
		static Sheet currentSheet;
		static int currentHeight;

		static int rowHeight;
		static int rowSpaceLeft;

		public static ITexture WriteTexture(float[] data, TextureInfo info)
		{
			var id = currentSheet.TextureID;
			var position = writeTexture(data, new MPos(info.Width, info.Height));

			return new ITexture(info.File, position.X, position.Y, info.Width, info.Height, id);
		}

		static MPos writeTexture(float[] data, int dataOffset, MPos size)
		{
			if (rowSpaceLeft < size.X)
				newRow();

			if (size.Y > rowHeight)
				rowHeight = size.Y;

			var location = new MPos(currentSheet.Size.X - rowSpaceLeft, currentHeight);

			rowSpaceLeft -= size.X;

			Array.Copy(data, dataOffset, currentSheet.Data, (currentHeight * currentSheet.Size.X + currentSheet.Size.X - rowSpaceLeft) * 4, data.Length);

			return location;
		}

		static void newRow()
		{
			rowSpaceLeft = currentSheet.Size.X;
			currentHeight += rowHeight;
			rowHeight = 0;
		}

		public static void UseSheet(Sheet sheet)
		{
			Clear();
			currentSheet = sheet;
			rowSpaceLeft = sheet.Size.X;
		}

		public static void Clear()
		{
			currentSheet = null;
			rowHeight = 0;
			rowSpaceLeft = 0;
		}
	}
}
