using System;
using System.Globalization;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Maps.Generators;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Bot;
using WarriorsSnuggery.Objects.Conditions;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Objects.Weapons.Projectiles;
using WarriorsSnuggery.Objects.Weapons.Warheads;
using WarriorsSnuggery.Physics;
using WarriorsSnuggery.Spells;

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
			var value = node.Value;

			if (t.IsEnum)
			{
				object @enum;
				try
				{
					@enum = Enum.Parse(t, value.Trim(), true);
				}
				catch (Exception e)
				{
					throw new InvalidEnumConversionException(file, node, t, e);
				}
				return @enum;
			}
			else if (t == typeof(int))
			{
				if (int.TryParse(value, out var i))
					return i;
				else
					throw new InvalidConversionException(file, node, t);
			}
			if (t == typeof(uint))
			{
				if (uint.TryParse(value, out var i))
					return i;
				else
					throw new InvalidConversionException(file, node, t);
			}
			else if (t == typeof(byte))
			{
				if (byte.TryParse(value, out var i))
					return i;
				else
					throw new InvalidConversionException(file, node, t);
			}
			else if (t == typeof(short))
			{
				if (short.TryParse(value, out var i))
					return i;
				else
					throw new InvalidConversionException(file, node, t);
			}
			else if (t == typeof(float))
			{
				if (float.TryParse(value, out var i))
					return i;
				else
					throw new InvalidConversionException(file, node, t);
			}
			else if (t == typeof(bool))
			{
				var v = value.ToLower().Trim();

				if (trueBooleans.Contains(v))
					return true;
				else if (falseBooleans.Contains(v))
					return false;
				else
					throw new InvalidConversionException(file, node, t);
			}
			else if (t == typeof(string))
			{
				return value.Trim();
			}
			else if (t.IsArray && t.GetElementType().IsEnum)
			{
				var parts = value.Split(',');

				for (int i = 0; i < parts.Length; i++)
					parts[i] = parts[i].Trim();

				var elementType = t.GetElementType();
				var enums = Array.CreateInstance(elementType, parts.Length);
				try
				{
					for (int i = 0; i < parts.Length; i++)
						enums.SetValue(Enum.Parse(elementType, parts[i], true), i);
				}
				catch (Exception e)
				{
					throw new InvalidEnumConversionException(file, node, t, e);
				}
				return enums;
			}
			else if (t == typeof(int[]))
			{
				var parts = value.Split(',');
				var res = new int[parts.Length];

				for (int i = 0; i < parts.Length; i++)
				{
					var part = parts[i].Trim();
					int convert;

					if (int.TryParse(part, out convert))
						res[i] = convert;
					else
						throw new InvalidConversionException(file, node, t);
				}

				return res;
			}
			else if (t == typeof(float[]))
			{
				var parts = value.Split(',');
				var res = new float[parts.Length];

				for (int i = 0; i < parts.Length; i++)
				{
					var part = parts[i].Trim();
					float convert;

					if (float.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out convert))
						res[i] = convert;
					else
						throw new InvalidConversionException(file, node, t);
				}

				return res;
			}
			else if (t == typeof(string[]))
			{
				var parts = value.Split(',');

				for (int i = 0; i < parts.Length; i++)
					parts[i] = parts[i].Trim();

				return parts;
			}
			else if (t == typeof(bool[]))
			{
				var parts = value.Split(',');
				var res = new bool[parts.Length];

				for (int i = 0; i < parts.Length; i++)
				{
					var part = parts[i].Trim();

					if (trueBooleans.Contains(part))
						res[i] = true;
					else if (falseBooleans.Contains(part))
						res[i] = false;
					else
						throw new InvalidConversionException(file, node, t);
				}

				return res;
			}
			else if (t == typeof(ushort[]))
			{
				var parts = value.Split(',');
				var res = new ushort[parts.Length];

				for (int i = 0; i < parts.Length; i++)
				{
					var part = parts[i].Trim();
					ushort convert;

					if (ushort.TryParse(part, out convert))
						res[i] = convert;
					else
						throw new InvalidConversionException(file, node, t);
				}

				return res;
			}
			else if (t == typeof(short[]))
			{
				var parts = value.Split(',');
				var res = new short[parts.Length];

				for (int i = 0; i < parts.Length; i++)
				{
					var part = parts[i].Trim();
					short convert;

					if (short.TryParse(part, out convert))
						res[i] = convert;
					else
						throw new InvalidConversionException(file, node, t);
				}

				return res;
			}
			else if (t == typeof(MPos))
			{
				var parts = value.Split(',');

				if (parts.Length == 2)
				{
					int x;
					int y;

					if (int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y))
						return new MPos(x, y);
				}
			}
			else if (t == typeof(CPos))
			{
				var parts = value.Split(',');

				if (parts.Length == 3)
				{
					int x;
					int y;
					int z;

					if (int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y) && int.TryParse(parts[2], out z))
						return new CPos(x, y, z);
				}
			}
			else if (t == typeof(Color))
			{
				var parts = value.Split(',');

				if (parts.Length >= 3 && parts.Length <= 4)
				{
					int r;
					int b;
					int g;
					var a = 255;

					if (int.TryParse(parts[0], out r) && int.TryParse(parts[1], out g) && int.TryParse(parts[2], out b))
					{
						if (parts.Length == 4 && int.TryParse(parts[3], out a) || parts.Length == 3)
							return new Color(r, g, b, a);
					}
				}
			}
			else if (t == typeof(Vector))
			{
				var parts = value.Split(',');

				if (parts.Length == 3)
				{
					float x;
					float y;
					float z;

					if (float.TryParse(parts[0], out x) && float.TryParse(parts[1], out y) && float.TryParse(parts[2], out z))
						return new Vector(x, y, z);
				}
			}
			else if (t == typeof(VAngle))
			{
				var parts = value.Split(',');

				if (parts.Length == 3)
				{
					float x;
					float y;
					float z;

					if (float.TryParse(parts[0], out x) && float.TryParse(parts[1], out y) && float.TryParse(parts[2], out z))
						return new VAngle(x, y, z);
				}
			}
			else if (t == typeof(SoundType))
			{
				return new SoundType(node.Children);
			}
			else if (t == typeof(Condition))
			{
				return new Condition(node.Value);
			}
			else if (t == typeof(ParticleSpawner))
			{
				var type = Type.GetType("WarriorsSnuggery.Objects.Particles." + value.Trim() + "ParticleSpawner", false, true);

				if (type == null || type.IsInterface)
					throw new InvalidConversionException(file, node, t);

				return Activator.CreateInstance(type, new[] { node.Children });
			}
			else if (t == typeof(TextureInfo))
			{
				var size = MPos.Zero;
				var name = value.Trim();
				var randomTexture = false;
				var tick = 20;
				bool searchFile = true;

				var children = node.Children;
				foreach (var child in children)
				{
					switch (child.Key)
					{
						case "AddDirectory":
							searchFile = child.Convert<bool>();
							break;
						case "Random":
							randomTexture = child.Convert<bool>();
							break;
						case "Tick":
							tick = child.Convert<int>();
							break;
						case "Size":
							size = child.Convert<MPos>();
							break;
						default:
							throw new UnexpectedConversionChild(file, node, t, child.Key);
					}
				}

				if (size != MPos.Zero)
					return new TextureInfo(name, randomTexture ? TextureType.RANDOM : TextureType.ANIMATION, tick, size.X, size.Y, searchFile);
			}
			else if (t == typeof(WeaponType))
			{
				if (!WeaponCreator.Types.ContainsKey(value))
					throw new MissingInfoException(value);

				return WeaponCreator.Types[value.Trim()];
			}
			else if (t == typeof(ParticleType))
			{
				if (!ParticleCreator.Types.ContainsKey(value))
					throw new MissingInfoException(value);

				return ParticleCreator.Types[value.Trim()];
			}
			else if (t == typeof(ActorType))
			{
				if (!ActorCreator.Types.ContainsKey(value))
					throw new MissingInfoException(value);

				return ActorCreator.Types[value.Trim()];
			}
			else if (t == typeof(SimplePhysicsType))
			{
				return new SimplePhysicsType(node.Children);
			}
			else if (t == typeof(Spell))
			{
				if (!SpellCreator.Types.ContainsKey(value))
					throw new MissingInfoException(value);

				return SpellCreator.Types[value.Trim()];
			}
			else if (t == typeof(Effect))
			{
				return new Effect(node.Children);
			}
			else if (t == typeof(IProjectile))
			{
				var type = Type.GetType("WarriorsSnuggery.Objects.Weapons.Projectiles." + value.Trim() + "Projectile", false, true);

				if (type == null || type.IsInterface)
					throw new InvalidConversionException(file, node, t);

				return Activator.CreateInstance(type, new[] { node.Children });
			}
			else if (t == typeof(IWarhead[]))
			{
				var array = new IWarhead[node.Children.Count];
				var i = 0;
				foreach (var child in node.Children)
				{
					var type = Type.GetType("WarriorsSnuggery.Objects.Weapons.Warheads." + child.Key.Trim() + "Warhead", false, true);

					if (type == null || type.IsInterface)
						throw new InvalidConversionException(file, child, t);

					array[i++] = (IWarhead)Activator.CreateInstance(type, new[] { child.Children });
				}
				return array;
			}
			else if (t == typeof(ActorProbabilityInfo[]))
			{
				var convert = new ActorProbabilityInfo[node.Children.Count];

				for (int i = 0; i < node.Children.Count; i++)
					convert[i] = new ActorProbabilityInfo(node.Children[i].Children);

				return convert;
			}
			else if (t == typeof(PatrolProbabilityInfo[]))
			{
				var convert = new PatrolProbabilityInfo[node.Children.Count];

				for (int i = 0; i < node.Children.Count; i++)
					convert[i] = new PatrolProbabilityInfo(node.Children[i].Children);

				return convert;
			}
			else if (t == typeof(ParticleType[]))
			{
				var convert = new ParticleType[node.Children.Count];

				for (int i = 0; i < node.Children.Count; i++)
				{
					if (!ParticleCreator.Types.ContainsKey(value))
						throw new MissingInfoException(value);

					convert[i] = ParticleCreator.Types[value.Trim()];
				}

				return convert;
			}
			else if (t == typeof(ParticleSpawner[]))
			{
				var convert = new ParticleSpawner[node.Children.Count];

				for (int i = 0; i < node.Children.Count; i++)
					convert[i] = Convert<ParticleSpawner>(file, node.Children[i]);

				return convert;
			}

			throw new InvalidConversionException(file, node, t);
		}
	}
}
