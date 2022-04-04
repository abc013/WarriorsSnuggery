using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Graphics
{
	public static class SheetBuilder
	{
		static Sheet currentSheet;
		static readonly List<SheetFragment> currentFragments = new List<SheetFragment>();

		public static void UseSheet(Sheet sheet)
		{
			Clear();
			currentSheet = sheet;
			currentFragments.Add(new SheetFragment(MPos.Zero, sheet.Bounds));
		}

		public static Texture WriteTexture(float[] data, string filepath, int width, int height)
		{
			var bounds = new MPos(width, height);
			var fragment = findLowest(bounds);

			if (fragment == null)
				throw new OverflowException($"Sheet (ID: {currentSheet.TextureID}) cannot contain texture (File: {filepath}, Size: {bounds}). Try increasing the sheet size.");

			var position = useFragment(fragment, data, bounds);

			return new Texture(position.X, position.Y, width, height, currentSheet.TextureID);
		}

		static SheetFragment findLowest(MPos bounds)
		{
			var fragments = currentFragments.Where(f => f.CanContain(bounds));

			SheetFragment frag = null;
			var fragValue = int.MaxValue;
			foreach (var f in fragments)
			{
				var currentValue = f.Bounds.X * f.Bounds.Y;
				if (currentValue < fragValue)
				{
					frag = f;
					fragValue = currentValue;
				}
			}

			return frag;
		}

		public static bool HasSpaceLeft(int width, int height)
		{
			return currentFragments.Find(f => f.CanContain(new MPos(width, height))) != null;
		}

		static MPos useFragment(SheetFragment fragment, float[] data, MPos bounds)
		{
			currentFragments.Remove(fragment);
			currentFragments.AddRange(fragment.Split(bounds));

			var position = fragment.Position;

			currentSheet.WriteData(data, position.X, position.Y, bounds.X, bounds.Y);

			return position;
		}

		public static void Clear()
		{
			currentSheet = null;
			var ordered = currentFragments.OrderBy(f => f.Bounds.X * f.Bounds.Y);
			currentFragments.Clear();
		}

		class SheetFragment
		{
			public readonly MPos Position;
			public readonly MPos Bounds;

			public SheetFragment(MPos position, MPos bounds)
			{
				Position = position;
				Bounds = bounds;
			}

			public bool CanContain(MPos textureBounds)
			{
				return textureBounds.X <= Bounds.X && textureBounds.Y <= Bounds.Y;
			}

			public List<SheetFragment> Split(MPos textureBounds)
			{
				var list = new List<SheetFragment>();

				if (textureBounds.X > Bounds.X || textureBounds.Y > Bounds.Y)
					throw new ArgumentOutOfRangeException($"Tried to fit texture (size: {textureBounds.X}, {textureBounds.Y}) into fragment of size {Bounds.X}, {Bounds.Y}.");

				if (textureBounds == Bounds)
					return list;

				var xDiff = Bounds.X - textureBounds.X;
				var yDiff = Bounds.Y - textureBounds.Y;

				if (xDiff == 0)
				{
					list.Add(new SheetFragment(Position + new MPos(0, textureBounds.Y), new MPos(Bounds.X, yDiff)));

					return list;
				}

				if (yDiff == 0)
				{
					list.Add(new SheetFragment(Position + new MPos(textureBounds.X, 0), new MPos(xDiff, Bounds.Y)));

					return list;
				}

				if (xDiff > yDiff)
				{
					list.Add(new SheetFragment(Position + new MPos(textureBounds.X, 0), new MPos(xDiff, Bounds.Y)));
					list.Add(new SheetFragment(Position + new MPos(0, textureBounds.Y), new MPos(textureBounds.X, yDiff)));
				}
				else
				{
					list.Add(new SheetFragment(Position + new MPos(textureBounds.X, 0), new MPos(xDiff, textureBounds.Y)));
					list.Add(new SheetFragment(Position + new MPos(0, textureBounds.Y), new MPos(Bounds.X, yDiff)));
				}

				return list;
			}
		}
	}
}
