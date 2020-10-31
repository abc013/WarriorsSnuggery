using System;
using System.Globalization;
using System.Linq;
using WarriorsSnuggery.Objects;

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

			if (t.IsEnum)
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
			else if (t == typeof(int))
			{
				if (int.TryParse(s, out var i) || s == "")
					return i;
			}
			if (t == typeof(uint))
			{
				if (uint.TryParse(s, out var i) || s == "")
					return i;
			}
			else if (t == typeof(byte))
			{
				if (byte.TryParse(s, out var i))
					return i;
			}
			else if (t == typeof(short))
			{
				if (short.TryParse(s, out var i))
					return i;
			}
			else if (t == typeof(float))
			{
				if (float.TryParse(s, out var i))
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
				var parts = s.Split(',');
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
				var parts = s.Split(',');
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
				var parts = s.Split(',');

				for (int i = 0; i < parts.Length; i++)
					parts[i] = parts[i].Trim();

				return parts;
			}
			else if (t == typeof(bool[]))
			{
				var parts = s.Split(',');
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
				var parts = s.Split(',');
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
				var parts = s.Split(',');
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
				var parts = s.Split(',');

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
				var parts = s.Split(',');

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
				var parts = s.Split(',');

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
				var parts = s.Split(',');

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
				var parts = s.Split(',');

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
			else if (t == typeof(Objects.Conditions.Condition))
			{
				return new Objects.Conditions.Condition(node.Value);
			}
			else if (t == typeof(Objects.Particles.ParticleSpawner))
			{
				var children = node.Children;
				switch (s)
				{
					case "Point":
						return new Objects.Particles.AreaParticleSpawner(children);
					case "Line":
						return new Objects.Particles.LineParticleSpawner(children);
					case "Area":
						return new Objects.Particles.AreaParticleSpawner(children);
					case "List":
						return new Objects.Particles.ListParticleSpawner(children);
				}
			}
			else if (t == typeof(Graphics.TextureInfo))
			{
				var size = MPos.Zero;
				var name = s.Trim();
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
					return new Graphics.TextureInfo(name, randomTexture ? Graphics.TextureType.RANDOM : Graphics.TextureType.ANIMATION, tick, size.X, size.Y, searchFile);
			}
			else if (t == typeof(Objects.Weapons.WeaponType))
			{
				if (!Objects.Weapons.WeaponCreator.Types.ContainsKey(s))
					throw new MissingInfoException(s);

				return Objects.Weapons.WeaponCreator.Types[s.Trim()];
			}
			else if (t == typeof(Objects.Particles.ParticleType))
			{
				if (!Objects.Particles.ParticleCreator.Types.ContainsKey(s))
					throw new MissingInfoException(s);

				return Objects.Particles.ParticleCreator.Types[s.Trim()];
			}
			else if (t == typeof(Objects.ActorType))
			{
				// Called method handles nonexistent actor types
				return ActorCreator.Types[s.Trim()];
			}
			else if (t == typeof(Spells.Spell))
			{
				return new Spells.Spell(node.Children);
			}
			else if (t == typeof(Objects.Weapons.IProjectileType))
			{
				var children = node.Children;
				switch (s)
				{
					case "Bullet":
						return new Objects.Weapons.BulletProjectileType(children);
					case "Magic":
						return new Objects.Weapons.MagicProjectileType(children);
					case "Beam":
						return new Objects.Weapons.BeamProjectileType(children);
					case "InstantHit":
						return new Objects.Weapons.InstantHitProjectileType(children);
				}
			}
			else if (t == typeof(Objects.Weapons.IWarhead[]))
			{
				var array = new Objects.Weapons.IWarhead[node.Children.Count];
				var i = 0;
				foreach (var child in node.Children)
				{
					var children = child.Children;
					switch (child.Key)
					{
						case "Sound":
							array[i++] = new Objects.Weapons.SoundWarhead(children);
							break;
						case "Smudge":
							array[i++] = new Objects.Weapons.SmudgeWarhead(children);
							break;
						case "Damage":
							array[i++] = new Objects.Weapons.DamageWarhead(children);
							break;
						case "Particle":
							array[i++] = new Objects.Weapons.ParticleWarhead(children);
							break;
						case "Actor":
							array[i++] = new Objects.Weapons.ActorWarhead(children);
							break;
						case "Spell":
							array[i++] = new Objects.Weapons.SpellWarhead(children);
							break;
						case "Force":
							array[i++] = new Objects.Weapons.ForceWarhead(children);
							break;
					}
				}
				return array;
			}
			else if (t == typeof(Maps.ActorProbabilityInfo[]))
			{
				var convert = new Maps.ActorProbabilityInfo[node.Children.Count];

				for (int i = 0; i < node.Children.Count; i++)
					convert[i] = new Maps.ActorProbabilityInfo(node.Children[i].Children);

				return convert;
			}
			else if (t == typeof(Maps.PatrolProbabilityInfo[]))
			{
				var convert = new Maps.PatrolProbabilityInfo[node.Children.Count];

				for (int i = 0; i < node.Children.Count; i++)
					convert[i] = new Maps.PatrolProbabilityInfo(node.Children[i].Children);

				return convert;
			}
			else if (t == typeof(Objects.Particles.ParticleType[]))
			{
				var convert = new Objects.Particles.ParticleType[node.Children.Count];

				for (int i = 0; i < node.Children.Count; i++)
				{
					if (!Objects.Particles.ParticleCreator.Types.ContainsKey(s))
						throw new MissingInfoException(s);

					convert[i] = Objects.Particles.ParticleCreator.Types[s.Trim()];
				}

				return convert;
			}
			else if (t == typeof(Objects.Particles.ParticleSpawner[]))
			{
				var convert = new Objects.Particles.ParticleSpawner[node.Children.Count];

				for (int i = 0; i < node.Children.Count; i++)
					convert[i] = Convert<Objects.Particles.ParticleSpawner>(file, node.Children[i]);

				return convert;
			}

			throw new InvalidConversionException(file, node, t);
		}
	}
}
