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
				assembly = loadedAssemblies[file];
				type = assembly.GetTypes().Where(t => t.BaseType == typeof(MissionScriptBase)).FirstOrDefault();

				Log.Debug("Mission script already in memory. Loaded.");
				return;
			}

			Stream stream;
			if (File.Exists(cachePath) && File.GetLastWriteTimeUtc(cachePath) > File.GetLastWriteTimeUtc(path))
			{
				stream = File.OpenRead(cachePath);

				Log.Debug("Script assembly compilation cached and not outdated. Loaded.");
			}
			else
				stream = compile();

			var timer = Timer.Start();

			AssemblyLoadContext context = AssemblyLoadContext.Default;
			assembly = context.LoadFromStream(stream);

			stream.Dispose();

			timer.StopAndWrite("Loading script assembly: " + file);

			type = assembly.GetTypes().Where(t => t.BaseType == typeof(MissionScriptBase)).FirstOrDefault();

			if (type == null)
				throw new MissingScriptException(file + ".cs");

			loadedAssemblies.Add(file, assembly);

			Log.Debug("Mission script successfully loaded.");
		}

		Stream compile()
		{
			var timer = Timer.Start();

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

			ms.Seek(0, SeekOrigin.Begin);

			timer.StopAndWrite("Compiling script assembly: " + file);
			Log.Debug("Script assembly compilation not cached or cache outdated. (Re-)Compiled.");

			return ms;
		}

		public MissionScriptBase Start(Game game)
		{
			return (MissionScriptBase)Activator.CreateInstance(type, new object[] { file, game });
		}
	}
}
