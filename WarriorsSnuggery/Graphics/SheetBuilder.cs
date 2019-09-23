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

		public static bool IsSpaceLeft(int width, int height)
		{
			if (rowSpaceLeft - width >= 0 && currentHeight + height < currentSheet.Size.Y)
				return true;

			return currentHeight + height + rowHeight < currentSheet.Size.Y;
		}

		public static ITexture WriteTexture(float[] data, TextureInfo info)
		{
			var id = currentSheet.TextureID;
			var position = writeTexture(data, new MPos(info.Width, info.Height));

			return new ITexture(info.File, position.X, position.Y, info.Width, info.Height, id);
		}

		static MPos writeTexture(float[] data, MPos size)
		{
			if (rowSpaceLeft < size.X)
				newRow();

			if (size.Y > rowHeight)
				rowHeight = size.Y;

			var location = new MPos(currentSheet.Size.X - rowSpaceLeft, currentHeight);

			for (int y = 0; y < size.Y; y++)
			{
				Array.Copy(data, y * size.X * 4, currentSheet.Data, ((currentHeight + y) * currentSheet.Size.X + currentSheet.Size.X - rowSpaceLeft) * 4, size.X * 4);
			}

			rowSpaceLeft -= size.X;

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
			currentHeight = 0;
			rowHeight = 0;
			rowSpaceLeft = 0;
		}
	}
}
