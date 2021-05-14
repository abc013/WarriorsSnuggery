using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace WarriorsSnuggery.Scripting
{
	public class MissionScriptLoader
	{
		static readonly Dictionary<string, Type> loadedAssemblies = new Dictionary<string, Type>();

		readonly string file;
		readonly Assembly assembly;
		readonly Type type;

		public MissionScriptLoader(string path, string file)
		{
			this.file = file;
			Log.Debug("Loading new mission script: " + path);

			if (loadedAssemblies.ContainsKey(file))
			{
				type = loadedAssemblies[file];
				assembly = Assembly.GetAssembly(type);

				Log.Debug("Mission script already in memory. Loaded.");
				return;
			}

			using var reader = new StreamReader(path);
			var content = reader.ReadToEnd();
			reader.Close();

			var assemblyLocation = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();

			var compilation = CSharpCompilation.Create(file)
			.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.AddReferences(
				MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
				MetadataReference.CreateFromFile(FileExplorer.MainDirectory + Path.DirectorySeparatorChar + "WarriorsSnuggery.dll"),
				MetadataReference.CreateFromFile(assemblyLocation + Path.DirectorySeparatorChar + "System.Runtime.dll"),
				MetadataReference.CreateFromFile(assemblyLocation + Path.DirectorySeparatorChar + "System.Runtime.Extensions.dll"),
				MetadataReference.CreateFromFile(assemblyLocation + Path.DirectorySeparatorChar + "System.Collections.dll"),
				MetadataReference.CreateFromFile(assemblyLocation + Path.DirectorySeparatorChar + "System.Linq.dll"),
				MetadataReference.CreateFromFile(assemblyLocation + Path.DirectorySeparatorChar + "mscorlib.dll")
			)
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(content));

			using var ms = new MemoryStream();
			var result = compilation.Emit(ms);

			if (!result.Success)
			{
				foreach (var compilerMessage in compilation.GetDiagnostics())
					Log.Exeption(compilerMessage);

				throw new Exception(string.Format("The script '{0}' could not be loaded. See console or error.log for more details.", file + ".cs"));
			}
			ms.Seek(0, SeekOrigin.Begin);

			AssemblyLoadContext context = AssemblyLoadContext.Default;
			assembly = context.LoadFromStream(ms);

			type = assembly.GetTypes().Where(t => t.BaseType == typeof(MissionScriptBase)).FirstOrDefault();

			if (type == null)
				throw new MissingScriptException(file + ".cs");

			loadedAssemblies.Add(file, type);

			Log.Debug("Mission script successfully loaded.");
		}

		public MissionScriptBase Start(Game game)
		{
			return (MissionScriptBase)Activator.CreateInstance(type, new object[] { file, game });
		}
	}
}
