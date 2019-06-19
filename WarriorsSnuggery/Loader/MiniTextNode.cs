/*
 * User: Andreas
 * Date: 14.05.2018
 * Time: 16:45
 */
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public class MiniTextNode
	{
		readonly string file;
		public readonly short Order;

		public MiniTextNode Parent;
		public List<MiniTextNode> Children = new List<MiniTextNode>();

		public readonly string Key;
		public readonly string Value;

		public MiniTextNode(string file, short order, string key, string value)
		{
			this.file = file;
			Order = order;
			Key = key;
			Value = value;
		}

		public T Convert<T>()
		{
			return Loader.NodeConverter.Convert<T>(file, this); // TODO
		}

		public object Convert(Type type)
		{
			return Loader.NodeConverter.Convert(file, this, type);
		}

		public MPos ToMPos()
		{
			try
			{
				var coords = Value.Split(',');

				var x = int.Parse(coords[0].Trim());
				var y = int.Parse(coords[1].Trim());
				return new MPos(x,y);
			}
			catch (Exception e)
			{
				throw new YamlInvalidFormatException(ToString(), typeof(MPos), e);
			}
		}

		public CPos ToCPos()
		{
			try
			{
				var coords = Value.Split(',');

				var x = int.Parse(coords[0].Trim());
				var y = int.Parse(coords[1].Trim());
				int z = 0;
				if (coords.Length > 2)
					z = int.Parse(coords[2].Trim());
				return new CPos(x,y,z);
			}
			catch (Exception e)
			{
				throw new YamlInvalidFormatException(ToString(), typeof(CPos), e);
			}
		}

		public Objects.ParticleSpawner ToParticleSpawner()
		{
			try
			{
				var count = 1;
				var radius = 256;

				if (Children.Exists(c => c.Key == "Count"))
					count = Children.Find(c => c.Key == "Count").ToInt();

				if (Children.Exists(c => c.Key == "Radius"))
					radius = Children.Find(c => c.Key == "Radius").ToInt();

				return new Objects.ParticleSpawner(ParticleCreator.GetType(Value), count, radius);
			}
			catch (Exception e)
			{
				throw new YamlInvalidFormatException(ToString(), typeof(Objects.ParticleSpawner), e);
			}
		}

		public TextureInfo ToTextureInfo()
		{
			try
			{
				var name = Value;
				var randomTexture = false;
				var tick = 10;
				bool searchFile = true;

				var size = Children.Find(c => c.Key == "Size").ToMPos();
				
				if (Children.Exists(c => c.Key == "AddDirectory"))
					searchFile = Children.Find(c => c.Key == "AddDirectory").ToBoolean();

				if (Children.Exists(c => c.Key == "Random"))
					randomTexture = Children.Find(c => c.Key == "Random").ToBoolean();

				if (Children.Exists(c => c.Key == "Tick"))
					tick = Children.Find(c => c.Key == "Tick").ToInt();

				return new TextureInfo(name, randomTexture ? TextureType.RANDOM : TextureType.ANIMATION, tick, size.X, size.Y, searchFile);
			}
			catch (Exception e)
			{
				throw new YamlInvalidFormatException(ToString(), typeof(TextureInfo), e);
			}
		}

		public TextureInfo ToTextureInfoAnim(out int frames, out string type)
		{
			try
			{
				frames = 1;
				type = "";
				var name = Value;
				var randomTexture = false;
				var tick = 10;
				bool searchFile = true;

				var size = Children.Find(c => c.Key == "Size").ToMPos();

				if (Children.Exists(c => c.Key == "AddDirectory"))
					searchFile = Children.Find(c => c.Key == "AddDirectory").ToBoolean();

				if (Children.Exists(c => c.Key == "Facings"))
					frames = Children.Find(c => c.Key == "Facings").ToInt();

				if (Children.Exists(c => c.Key == "Type"))
					type = Children.Find(c => c.Key == "Type").Value.ToLower();

				if (Children.Exists(c => c.Key == "Random"))
					randomTexture = Children.Find(c => c.Key == "Random").ToBoolean();

				if (Children.Exists(c => c.Key == "Tick"))
					tick = Children.Find(c => c.Key == "Tick").ToInt();

				return new TextureInfo(name, randomTexture ? TextureType.RANDOM : TextureType.ANIMATION, tick, size.X, size.Y, searchFile);
			}
			catch (Exception e)
			{
				throw new YamlInvalidFormatException(ToString(), typeof(TextureInfo), e);
			}
		}

		public bool ToBoolean()
		{
			try
			{
				var value = Value.ToLower().Trim();
				return value == "1" || value == "true" || value == "yes";
			}
			catch (Exception e)
			{
				throw new YamlInvalidFormatException(ToString(), typeof(bool), e);
			}
		}

		public string[] ToArray()
		{
			try
			{
				var split = Value.Split(',');
				for(int i = 0; i < split.Length; i++)
				{
					split[i] = split[i].Trim();
				}
				return split;
			}
			catch (Exception e)
			{
				throw new YamlInvalidFormatException(ToString(), typeof(string[]), e);
			}
		}

		public Color ToColor()
		{
			try
			{
				var split = Value.Split(',');

				var r = int.Parse(split[0].Trim());
				var g = int.Parse(split[1].Trim());
				var b = int.Parse(split[2].Trim());
				var a = 255;
				if (split.Length > 3)
					a = int.Parse(split[3]);

				return new Color(r,g,b,a);
			}
			catch (Exception e)
			{
				throw new YamlInvalidFormatException(ToString(), typeof(Color), e);
			}
		}

		public int ToInt()
		{
			if (Value == "")
				return 0;

			if (!int.TryParse(Value.Trim(), out int i))
				throw new YamlInvalidFormatException(ToString(), typeof(int));

			return i;
		}

		public float ToFloat()
		{
			if (!float.TryParse(Value.Trim(), out float f))
				throw new YamlInvalidFormatException(ToString(), typeof(float));

			return f;
		}

		public object ToEnum(Type type)
		{
			return Enum.Parse(type, Value.Trim(), true);
		}

		public override string ToString()
		{
			return string.Format("Key '{0}' | Value '{1}'", Key, Value);
		}
	}
}
