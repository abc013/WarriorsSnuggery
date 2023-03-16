using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Scripting
{
	public class MissionScriptLoader
	{
		static readonly Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();

		readonly PackageFile packageFile;
		readonly string filePath;

		string cachePath => filePath + ".cache";

		readonly Assembly assembly;
		readonly Type type;

		public MissionScriptLoader(PackageFile packageFile)
		{
			this.packageFile = packageFile;
			filePath = FileExplorer.FindIn(packageFile.Package.ScriptsDirectory, packageFile.File, ".cs");

			Log.Debug("Loading new mission script: " + filePath);

			if (loadedAssemblies.ContainsKey(filePath))
			{
				if (!Program.ReloadScripts)
				{
					assembly = loadedAssemblies[filePath];
					type = assembly.GetTypes().Where(t => t.BaseType == typeof(MissionScriptBase)).FirstOrDefault();

					Log.Debug("Mission script already in memory. Loaded.");
					return;
				}

				loadedAssemblies.Remove(filePath);

				Log.Debug("Mission script already in memory, but reload enabled. Reloading.");
			}

			if (File.Exists(cachePath) && File.GetLastWriteTimeUtc(cachePath) > File.GetLastWriteTimeUtc(filePath))
				Log.Debug("Script assembly compilation cached and not outdated. Loaded.");
			else
				compileAndCache();

			var data = File.ReadAllBytes(cachePath);

			var timer = Timer.StartNew();

			assembly = Assembly.Load(data);

			timer.StopAndWrite("Loading script assembly: " + filePath);

			type = assembly.GetTypes().Where(t => t.BaseType == typeof(MissionScriptBase)).FirstOrDefault();

			if (type == null)
				throw new MissingScriptException(packageFile.ToString());

			loadedAssemblies.Add(filePath, assembly);

			Log.Debug("Mission script successfully loaded.");
		}

		void compileAndCache()
		{
			var timer = Timer.StartNew();

			using var reader = new StreamReader(filePath);
			var content = reader.ReadToEnd();
			reader.Close();

			var assemblyLocation = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory() + FileExplorer.Separator;

			var compilation = CSharpCompilation.Create(packageFile.ToString())
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

				throw new Exception($"The script '{packageFile + ".cs"}' could not be loaded. See console or error.log for more details.");
			}
			ms.Seek(0, SeekOrigin.Begin);

			// Cache compilation
			using var fileStream = File.Create(cachePath);
			ms.WriteTo(fileStream);

			timer.StopAndWrite("Compiling script assembly: " + packageFile);
			Log.Debug("Script assembly compilation not cached or cache outdated. (Re-)Compiled.");
		}

		public MissionScriptBase Start(Game game)
		{
			return (MissionScriptBase)Activator.CreateInstance(type, new object[] { packageFile, game });
		}
	}
}
