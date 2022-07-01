using System;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Maps.Generators;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Actors.Bot;
using WarriorsSnuggery.Objects.Conditions;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Objects.Weapons.Projectiles;
using WarriorsSnuggery.Objects.Weapons.Warheads;
using WarriorsSnuggery.Physics;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Loader
{
	public static class TextNodeConverter
	{
		static readonly string[] trueBooleans = new[]
		{
			"1",
			"true",
			"yes"
		};
		static readonly string[] falseBooleans =
		{
			"0",
			"false",
			"no"
		};

		public static T Convert<T>(TextNode node)
		{
			return (T)Convert(node, typeof(T));
		}

		public static object Convert(TextNode node, Type t)
		{
			return convert(node.Value, node, t);
		}

		static object convert(string value, TextNode node, Type t)
		{
			if (t.IsEnum)
			{
				if (Enum.TryParse(t, value, true, out var @enum))
					return @enum;
			}
			else if (t == typeof(int))
			{
				if (int.TryParse(value, out var i))
					return i;
			}
			if (t == typeof(uint))
			{
				if (uint.TryParse(value, out var i))
					return i;
			}
			else if (t == typeof(byte))
			{
				if (byte.TryParse(value, out var i))
					return i;
			}
			else if (t == typeof(short))
			{
				if (short.TryParse(value, out var i))
					return i;
			}
			else if (t == typeof(ushort))
			{
				if (ushort.TryParse(value, out var i))
					return i;
			}
			else if (t == typeof(float))
			{
				if (float.TryParse(value, out var i))
					return i;
			}
			else if (t == typeof(bool))
			{
				var v = value.ToLower();

				if (trueBooleans.Contains(v))
					return true;
				else if (falseBooleans.Contains(v))
					return false;
			}
			else if (t == typeof(string))
			{
				return value;
			}
			else if (t == typeof(PackageFile))
			{
				return new PackageFile(value);
			}
			else if (t == typeof(MPos))
			{
				var parts = value.Split(',');

				if (parts.Length == 2)
				{
					if (int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
						return new MPos(x, y);
				}
			}
			else if (t == typeof(CPos))
			{
				var parts = value.Split(',');

				if (parts.Length == 3)
				{
					if (int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y) && int.TryParse(parts[2], out int z))
						return new CPos(x, y, z);
				}
			}
			else if (t == typeof(Color))
			{
				var parts = value.Split(',');

				if (parts.Length >= 3 && parts.Length <= 4)
				{
					if (int.TryParse(parts[0], out int r) && int.TryParse(parts[1], out int g) && int.TryParse(parts[2], out int b))
					{
						var a = 255;
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
					if (float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y) && float.TryParse(parts[2], out float z))
						return new Vector(x, y, z);
				}
			}
			else if (t == typeof(VAngle))
			{
				var parts = value.Split(',');

				if (parts.Length == 3)
				{
					if (float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y) && float.TryParse(parts[2], out float z))
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
				var type = Type.GetType($"WarriorsSnuggery.Objects.Particles.{value}ParticleSpawner", false, true);

				if (type != null && !type.IsInterface)
					return Activator.CreateInstance(type, new[] { node.Children });
			}
			else if (t == typeof(TextureInfo))
			{
				return TextureInfo.Create(node);
			}
			else if (t == typeof(WeaponType))
			{
				return WeaponCache.Types[value];
			}
			else if (t == typeof(ParticleType))
			{
				return ParticleCache.Types[value];
			}
			else if (t == typeof(ActorType))
			{
				return ActorCache.Types[value];
			}
			else if (t == typeof(SimplePhysicsType))
			{
				return new SimplePhysicsType(node.Children);
			}
			else if (t == typeof(Effect))
			{
				if (!EffectCache.Types.ContainsKey(value))
					throw new MissingInfoException(value);

				return EffectCache.Types[value];
			}
			else if (t == typeof(BotBehaviorType))
			{
				var type = Type.GetType($"WarriorsSnuggery.Objects.Actors.Bot.{value}BotBehaviorType", false, true);

				if (type != null && !type.IsInterface)
					return Activator.CreateInstance(type, new[] { node.Children });
			}
			else if (t == typeof(IProjectile))
			{
				var type = Type.GetType($"WarriorsSnuggery.Objects.Weapons.Projectiles.{value}Projectile", false, true);

				if (type != null && !type.IsInterface)
					return Activator.CreateInstance(type, new[] { node.Children });
			}
			else if (t == typeof(IWarhead[]))
			{
				var array = new IWarhead[node.Children.Count];
				var i = 0;
				foreach (var child in node.Children)
				{
					var type = Type.GetType($"WarriorsSnuggery.Objects.Weapons.Warheads.{child.Key}Warhead", false, true);

					if (type == null || type.IsInterface)
						throw new InvalidConversionException(child, t);

					array[i++] = (IWarhead)Activator.CreateInstance(type, new[] { child.Children });
				}
				return array;
			}
			else if (t == typeof(IMapGeneratorInfo[]))
			{
				var array = new IMapGeneratorInfo[node.Children.Count];
				var i = 0;
				foreach (var child in node.Children)
				{
					var type = Type.GetType($"WarriorsSnuggery.Maps.Generators.{child.Key}Info", true, true);

					if (type == null || type.IsInterface)
						throw new InvalidConversionException(child, t);

					array[i++] = (IMapGeneratorInfo)Activator.CreateInstance(type, new object[] { child.Convert<int>(), child.Children });
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
					if (!ParticleCache.Types.ContainsKey(value))
						throw new MissingInfoException(value);

					convert[i] = ParticleCache.Types[value];
				}

				return convert;
			}
			else if (t == typeof(ParticleSpawner[]))
			{
				var convert = new ParticleSpawner[node.Children.Count];

				for (int i = 0; i < node.Children.Count; i++)
					convert[i] = Convert<ParticleSpawner>(node.Children[i]);

				return convert;
			}
			else if (t == typeof(Effect))
			{
				if (!EffectCache.Types.ContainsKey(value))
					throw new MissingInfoException(value);

				return EffectCache.Types[value];
			}
			else if (t.IsArray)
			{
				var parts = value.Split(',');
				var elementType = t.GetElementType();

				var array = Array.CreateInstance(elementType, parts.Length);

				for (int i = 0; i < parts.Length; i++)
					array.SetValue(convert(parts[i].Trim(), node, elementType), i);

				return array;
			}

			throw new InvalidConversionException(node, t);
		}
	}
}
