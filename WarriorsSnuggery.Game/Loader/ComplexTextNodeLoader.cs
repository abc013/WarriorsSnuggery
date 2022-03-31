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

		public void Load(string directory, string file)
		{
			if (finished)
				throw new InvalidOperationException($"Unable to load file into {nameof(ComplexTextNodeLoader)} after finish procedure.");

			Log.LoaderDebug(loaderName, $"Loading '{file}'.");
			processNewNodes(TextNodeLoader.FromFile(directory, file));
		}

		void processNewNodes(List<TextNode> newNodes)
		{
			foreach (var node in newNodes)
			{
				if (node.Key.StartsWith('-'))
				{
					var existing = nodes.FirstOrDefault(n => n.Key == node.Key[1..]);

					if (existing != null)
					{
						Log.LoaderDebug(loaderName, $"Removing key entry '{node.Key}'.");
						nodes.Remove(existing);
						continue;
					}

					Log.LoaderWarning(loaderName, $"[{node.Origin}] Unable to remove key entry '{node.Key}'. Are you missing something?");
					continue;
				}

				if (node.Key.StartsWith('+'))
				{
					var existing = nodes.FirstOrDefault(n => n.Key == node.Key[1..]);

					if (existing != null)
					{
						Log.LoaderDebug(loaderName, $"Merging duplicate key entry '{node.Key}'.");
						combine(existing, node, false);
						continue;
					}

					Log.LoaderWarning(loaderName, $"[{node.Origin}] Unable to merge new key entry '{node.Key}'. Are you missing something?");
					continue;
				}

				// Check for duplicate keys
				if (nodes.Exists(n => node.Key == n.Key))
					throw new DuplicateNodeEntryException(node);

				checkDuplicates(node);
				nodes.Add(node);
			}
		}

		public List<TextNode> Finish()
		{
			finished = true;

			foreach (var parent in nodes)
				loadInheritNodes(parent);

			Log.LoaderDebug(loaderName, $"Finished loading.");

			return nodes;
		}

		void checkDuplicates(TextNode parent)
		{
			foreach (var node in parent.Children)
			{
				// Check for duplicate nodes
				var n = parent.Children.FirstOrDefault(n => n != node && node.Key == n.Key && node.Specification == n.Specification);
				if (n != null)
					throw new DuplicateNodeEntryException(n);

				checkDuplicates(node);
			}
		}

		void loadInheritNodes(TextNode parent)
		{
			// In this procedure, children of the first node order are processed for inheritance
			if (inheritanceCleanNodes.Contains(parent))
				return;

			inheritanceStack.Push(parent);

			var inheritNodes = parent.Children.Where(n => n.Key == "Inherits").ToList();
			foreach (var node in inheritNodes)
			{
				var inheritParent = nodes.FirstOrDefault(n => n.Key == node.Value);

				if (inheritParent == null)
				{
					Log.LoaderWarning(loaderName, $"[{node.Origin}] Unable to inherit from unknown node '{node.Value}' in node '{parent.Key}'. Are you missing something?");
					continue;
				}

				if (inheritParent == parent)
				{
					Log.LoaderWarning(loaderName, $"[{node.Origin}] Attempted to inherit node '{parent.Key}' from itself, aborting.");
					continue;
				}

				if (inheritanceStack.Contains(inheritParent))
					throw new InvalidNodeException($"[{node.Origin}] Inheritance loop detected: Key '{inheritParent.Key}'. Loop: {string.Join(',', inheritanceStack)}");

				Log.LoaderDebug(loaderName, $"Inheriting: '{inheritParent.Key}'->'{parent.Key}'.");

				parent.Children.Remove(node);

				loadInheritNodes(inheritParent);
				inheritAndCombine(parent, inheritParent, true);
			}

			inheritanceCleanNodes.Add(parent);

			inheritanceStack.Pop();
		}

		void combine(TextNode existing, TextNode @new, bool final)
		{
			var existingList = existing.Children;
			var newList = @new.Children;

			combine(existingList, newList, final);
		}

		void inheritAndCombine(TextNode existing, TextNode inheritanceFrom, bool final)
		{
			var newList = existing.Children.ToList();

			existing.Children.Clear();
			existing.Children.AddRange(inheritanceFrom.Children);

			combine(existing.Children, newList, final);
		}

		void combine(List<TextNode> existingList, List<TextNode> newList, bool final)
		{
			foreach (var node in newList)
			{
				if (node.Key.StartsWith('-'))
				{
					var key = node.Key[1..];
					var specification = node.Specification;

					var count = existingList.RemoveAll(n => n.Key == key && n.Specification == specification);

					if (count > 0)
						Log.LoaderDebug(loaderName, $"Removed '{node.ToIdentifierString()[1..]}' in parent '{node.Parent}'.");
					else if (final)
						Log.LoaderWarning(loaderName, $"[{node.Origin}] No nodes '{node.ToIdentifierString()[1..]}' to remove in parent '{node.Parent}'. Are you missing something?");

					continue;
				}

				if (node.Key.StartsWith('+'))
				{
					var key = node.Key[1..];
					var specification = node.Specification;

					var existingNode = existingList.FirstOrDefault(n => n.Key == key && n.Specification == specification);

					if (existingNode != null)
					{
						Log.LoaderDebug(loaderName, $"Merging entries '{node.ToIdentifierString()[1..]}'.");
						existingList.Remove(existingNode);

						// override the value
						var overrideNode = new TextNode(node.Origin, node.Order, key, specification, node.Value);
						overrideNode.Children.AddRange(existingNode.Children);
						overrideNode.Parent = node.Parent;
						existingList.Add(overrideNode);

						combine(overrideNode, node, final);
						continue;
					}

					Log.LoaderWarning(loaderName, $"[{node.Origin}] No nodes '{node.ToIdentifierString()[1..]}' to merge. Are you missing something?");
					continue;
				}

				// Check for duplicate nodes
				if (existingList.Exists(n => node.Key == n.Key && node.Specification == n.Specification))
					throw new DuplicateNodeEntryException(node);

				existingList.Add(node);
			}
		}
	}
}
