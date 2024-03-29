﻿using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects
{
	public static class WallCache
	{
		public static readonly Dictionary<short, WallType> Types = new Dictionary<short, WallType>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (var node in nodes)
			{
				var id = short.Parse(node.Key);
				Types.Add(id, new WallType(id, node.Children));
			}
		}

		public static Wall Create(WPos position, World world, short ID)
		{
			if (!Types.ContainsKey(ID))
				throw new MissingFieldException($"Wall with ID '{ID}' does not exist.");

			return new Wall(position, world, Types[ID]);
		}
	}
}
