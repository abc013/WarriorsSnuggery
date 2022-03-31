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
		static readonly Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();

		readonly string file;
		readonly string path;
		string cachePath => path + ".cache";

		readonly Assembly assembly;
		readonly Type type;

		public MissionScriptLoader(string path, string file)
		{
			this.file = file;
			this.path = path;

			Log.Debug("Loading new mission script: " + path);

			if (loadedAssemblies.ContainsKey(file))
			{
				if (!Program.ReloadScripts)
				{
					assembly = loadedAssemblies[file];
					type = assembly.GetTypes().Where(t => t.BaseType == typeof(MissionScriptBase)).FirstOrDefault();

					Log.Debug("Mission script already in memory. Loaded.");
					return;
				}

				loadedAssemblies.Remove(file);

				Log.Debug("Mission script already in memory, but reload enabled. Reloading.");
			}

			if (File.Exists(cachePath) && File.GetLastWriteTimeUtc(cachePath) > File.GetLastWriteTimeUtc(path))
				Log.Debug("Script assembly compilation cached and not outdated. Loaded.");
			else
				compileAndCache();

			var data = File.ReadAllBytes(cachePath);

			var timer = Timer.Start();

			assembly = Assembly.Load(data);

			timer.StopAndWrite("Loading script assembly: " + file);

			type = assembly.GetTypes().Where(t => t.BaseType == typeof(MissionScriptBase)).FirstOrDefault();

			if (type == null)
				throw new MissingScriptException(file + ".cs");

			loadedAssemblies.Add(file, assembly);

			Log.Debug("Mission script successfully loaded.");
		}

		void compileAndCache()
		{
			var timer = Timer.Start();

			using var reader = new StreamReader(path);
			var content = reader.ReadToEnd();
			reader.Close();

			var assemblyLocation = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory() + FileExplorer.Separator;

			var compilation = CSharpCompilation.Create(file)
			.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.AddReferences(
				MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
				MetadataReference.CreateFromFile(FileExplorer.MainDirectory + "WarriorsSnuggery.dll"),
				MetadataReference.CreateFromFile(FileExplorer.MainDirectory + "OpenTK.Windowing.GraphicsLibraryFramework.dll"),
				MetadataReference.CreateFromFile(assemblyLocation + "System.Runtime.dll"),
				MetadataReference.CreateFromFile(assemblyLocation + "System.Runtime.Extensions.dll"),
				MetadataReference.CreateFromFile(assemblyLocation + "System.Collections.dll"),
				MetadataReference.CreateFromFile(assemblyLocation + "System.Linq.dll"),
				MetadataReference.CreateFromFile(assemblyLocation + "mscorlib.dll")
			)
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(content));

			var ms = new MemoryStream();
			var result = compilation.Emit(ms);

			if (!result.Success)
			{
				foreach (var compilerMessage in compilation.GetDiagnostics())
					Log.Exeption(compilerMessage);

				throw new Exception($"The script '{file + ".cs"}' could not be loaded. See console or error.log for more details.");
			}
			ms.Seek(0, SeekOrigin.Begin);

			// Cache compilation
			using var fileStream = File.Create(cachePath);
			ms.WriteTo(fileStream);

			timer.StopAndWrite("Compiling script assembly: " + file);
			Log.Debug("Script assembly compilation not cached or cache outdated. (Re-)Compiled.");
		}

		public MissionScriptBase Start(Game game)
		{
			return (MissionScriptBase)Activator.CreateInstance(type, new object[] { file, game });
		}
	}
}
