using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarriorsSnuggery.Loader
{
	public static class NodeConverter
	{
		public static readonly string[] trueBooleans = new []
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
			return (T) Convert(file, node, typeof(T));
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

					if (int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y) && int.TryParse(parts[1], out z))
						return new CPos(x, y, z);
				}
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
