﻿using System;
using System.Linq;

namespace WarriorsSnuggery.Loader
{
	public static class NodeConverter
	{
		public static readonly string[] trueBooleans = new[]
		{
			"1",
			"true",
			"yes"
		};
		public static readonly string[] falseBooleans =
		{
			"0",
			"false",
			"no"
		};

		public static T Convert<T>(string file, MiniTextNode node)
		{
			return (T)Convert(file, node, typeof(T));
		}

		public static object Convert(string file, MiniTextNode node, Type t)
		{
			var s = node.Value;

			if (t == typeof(int))
			{
				var i = 0;

				if (int.TryParse(s, out i) || s == "")
					return i;
			}
			else if (t == typeof(byte))
			{
				var i = (byte)0;

				if (byte.TryParse(s, out i))
					return i;
			}
			else if (t == typeof(float))
			{
				var i = 0f;

				if (float.TryParse(s, out i))
					return i;
			}
			else if (t == typeof(bool))
			{
				var v = s.ToLower().Trim();

				if (trueBooleans.Contains(v))
					return true;
				else if (falseBooleans.Contains(v))
					return false;
			}
			else if (t == typeof(string))
			{
				return s.Trim();
			}
			else if (t == typeof(int[]))
			{
				var parts = s.Split(',').ToArray();
				var res = new int[parts.Length];

				for (int i = 0; i < parts.Length; i++)
				{
					var part = parts[i].Trim();
					var convert = 0;

					if (int.TryParse(part, out convert))
					{
						res[i] = convert;
					}
					else
					{
						throw new InvalidConversionException(file, node, t);
					}
				}

				return res;
			}
			else if (t == typeof(float[]))
			{
				var parts = s.Split(',').ToArray();
				var res = new float[parts.Length];

				for (int i = 0; i < parts.Length; i++)
				{
					var part = parts[i].Trim();
					var convert = 0.0f;

					if (float.TryParse(part, out convert))
					{
						res[i] = convert;
					}
					else
					{
						throw new InvalidConversionException(file, node, t);
					}
				}

				return res;
			}
			else if (t == typeof(string[]))
			{
				var parts = s.Split(',').ToArray();

				for (int i = 0; i < parts.Length; i++)
				{
					parts[i] = parts[i].Trim();
				}

				return parts;
			}
			else if (t == typeof(MPos))
			{
				var parts = s.Split(',');

				if (parts.Length == 2)
				{
					var x = 0;
					var y = 0;

					if (int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y))
						return new MPos(x, y);
				}
			}
			else if (t == typeof(CPos))
			{
				var parts = s.Split(',');

				if (parts.Length == 3)
				{
					var x = 0;
					var y = 0;
					var z = 0;

					if (int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y) && int.TryParse(parts[2], out z))
						return new CPos(x, y, z);
				}
			}
			else if (t == typeof(Color))
			{
				var parts = s.Split(',');

				if (parts.Length >= 3 && parts.Length <= 4)
				{
					var r = 0;
					var g = 0;
					var b = 0;
					var a = 255;

					if (int.TryParse(parts[0], out r) && int.TryParse(parts[1], out g) && int.TryParse(parts[2], out b))
					{
						if (parts.Length == 4 && int.TryParse(parts[3], out a) || parts.Length == 3)
							return new Color(r, g, b, a);
					}
				}
			}
			else if (t == typeof(Objects.ParticleSpawner))
			{
				var name = s.Trim();
				var count = 1;
				var radius = 256;

				var children = node.Children;
				foreach (var child in children)
				{
					if (child.Key == "Count")
					{
						count = child.Convert<int>();
					}
					else if (child.Key == "Radius")
					{
						radius = child.Convert<int>();
					}
					else
					{
						throw new UnexpectedConversionChild(file, node, t, child.Key);
					}
				}

				return new Objects.ParticleSpawner(ParticleCreator.GetType(name), count, radius);
			}
			else if (t == typeof(Graphics.TextureInfo))
			{
				var size = MPos.Zero;
				var name = s.Trim();
				var randomTexture = false;
				var tick = 10;
				bool searchFile = true;

				var children = node.Children;
				foreach (var child in children)
				{
					if (child.Key == "AddDirectory")
					{
						searchFile = child.Convert<bool>();
					}
					else if (child.Key == "Random")
					{
						randomTexture = child.Convert<bool>();
					}
					else if (child.Key == "Tick")
					{
						tick = child.Convert<int>();
					}
					else if (child.Key == "Size")
					{
						size = child.Convert<MPos>();
					}
					else
					{
						throw new UnexpectedConversionChild(file, node, t, child.Key);
					}
				}

				if (size != MPos.Zero)
				{
					return new Graphics.TextureInfo(name, randomTexture ? Graphics.TextureType.RANDOM : Graphics.TextureType.ANIMATION, tick, size.X, size.Y, searchFile);
				}
			}
			else if (t == typeof(Objects.WeaponType))
			{
				// Called method handles nonexistant weapon types
				return WeaponCreator.GetType(s.Trim());
			}
			else if (t.IsEnum)
			{
				object @enum;
				try
				{
					@enum = Enum.Parse(t, s.Trim(), true);
				}
				catch (Exception e)
				{
					throw new InvalidEnumConversionException(file, node, t, e);
				}
				return @enum;
			}

			throw new InvalidConversionException(file, node, t);
		}
	}
}