using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Loader
{
	public class ComplexTextNodeLoader
	{
		readonly string loaderName;

		readonly List<TextNode> nodes = new List<TextNode>();
		readonly List<TextNode> inheritanceCleanNodes = new List<TextNode>();
		readonly Stack<TextNode> inheritanceStack = new Stack<TextNode>();

		bool finished;

		public ComplexTextNodeLoader(string loaderName)
		{
			this.loaderName = loaderName;
		}

		public void Load((string directory, string file)[] data)
		{
			if (finished)
				throw new InvalidOperationException($"Unable to load file into {nameof(ComplexTextNodeLoader)} after finish procedure.");

			var rawNodes = new List<TextNode>();

			foreach (var (directory, file) in data)
			{
				Log.LoaderDebug(loaderName, $"Loading '{file}'.");
				rawNodes.AddRange(TextNodeLoader.FromFile(directory, file));
			}

			foreach (var node in rawNodes)
			{
				var existing = nodes.FirstOrDefault(n => n.Key == node.Key);
				if (existing != null)
				{
					Log.LoaderDebug(loaderName, $"Merging duplicate key entry '{node.Key}'.");

					existing.Children.AddRange(node.Children);
					continue;
				}

				nodes.Add(node);
			}
		}

		public List<TextNode> Finish()
		{
			finished = true;

			// In this procedure, children of the first node order are processed for 
			foreach (var parent in nodes)
				loadInheritNodes(parent);

			// In the following procedure, children of the first node order will be scanned for duplicate keys
			// Every node key that was mentioned with a minus before will be removed
			foreach (var parent in nodes)
			{
				var removeNodes = parent.Children.Where(n => n.Key.StartsWith('-')).ToList();

				var keysToRemove = new List<(string key, string specification)>();
				foreach (var node in removeNodes)
				{
					keysToRemove.Add((node.Key.Remove(0, 1), node.Specification));
					parent.Children.Remove(node);
				}

				foreach (var (key, specification) in keysToRemove)
				{
					var count = parent.Children.RemoveAll(n => n.Key == key && n.Specification == specification);

					if (count > 0)
						Log.LoaderDebug(loaderName, $"Removed {count} node(s) '{key}@{specification}' in parent '{parent.Key}'.");
					else
						Log.LoaderWarning(loaderName, $"No nodes with '{key}@{specification}' to remove in parent '{parent.Key}'. Are you missing something?");
				}
			}

			Log.LoaderDebug(loaderName, $"Finished loading.");

			return nodes;
		}

		List<TextNode> loadInheritNodes(TextNode parent)
		{
			if (inheritanceCleanNodes.Contains(parent))
				return parent.Children;

			inheritanceStack.Push(parent);

			var inheritNodes = parent.Children.Where(n => n.Key == "Inherits").ToList();

			foreach (var node in inheritNodes)
			{
				var inheritParent = nodes.FirstOrDefault(n => n.Key == node.Value);

				if (inheritParent == null)
				{
					Log.LoaderWarning(loaderName, $"Unable to inherit from unknown node '{node.Value}' in node '{parent.Key}'. Are you missing something?");
					continue;
				}

				if (inheritParent == parent)
				{
					Log.LoaderWarning(loaderName, $"Attempted to inherit node '{parent.Key}' from itself, aborting.");
					continue;
				}

				if (inheritanceStack.Contains(inheritParent))
					throw new InvalidNodeException($"Inheritance loop detected: Key '{inheritParent.Key}'. Loop: {string.Join(',', inheritanceStack)}");

				Log.LoaderDebug(loaderName, $"Applying: Key '{parent.Key}' inherits from '{inheritParent.Key}'.");

				parent.Children.Remove(node);
				parent.Children.AddRange(loadInheritNodes(inheritParent));
			}

			inheritanceCleanNodes.Add(parent);

			inheritanceStack.Pop();

			return parent.Children;
		}
	}
}
