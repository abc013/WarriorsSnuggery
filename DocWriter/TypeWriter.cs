using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WarriorsSnuggery
{
	public static class TypeWriter
	{
		static Assembly assembly;

		public static void Initialize()
		{
			assembly = Assembly.Load("WarriorsSnuggery");
		}

		public static void Write(StreamWriter writer, string typeName, object[] args)
		{
			var type = assembly.GetType(typeName);

			var attrib = type.GetCustomAttribute(typeof(DescAttribute));
			var description = attrib == null ? null : ((DescAttribute)attrib).Desc;
			if (description != null)
				HTMLWriter.WriteDescription(writer, description);

			var obj = Activator.CreateInstance(type, args);
			var variables = type.GetFields().Where(f => f.IsInitOnly && f.GetCustomAttribute(typeof(DescAttribute)) != null);
			var cells = new List<TableCell>();

			foreach (var variable in variables)
			{
				var varname = variable.Name;
				var vartype = getNameOfType(variable.FieldType.Name);
				var vardesc = getDescription(variable);
				var value = variable.GetValue(obj);

				cells.Add(new TableCell(varname, vartype, vardesc, value == null ? "Not given" : value.ToString()));
			}
			HTMLWriter.WriteTable(writer, cells, true);
		}

		static string[] getDescription(FieldInfo variable)
		{
			var desc = ((DescAttribute)variable.GetCustomAttribute(typeof(DescAttribute))).Desc;

			var type = variable.FieldType.IsArray ? variable.FieldType.GetElementType() : variable.FieldType;
			if (!type.IsEnum)
				return desc;

			var enumNames = Enum.GetNames(type);
			var newDesc = "Available options: " + string.Join(", ", enumNames);

			var array = new string[desc.Length + 1];
			for (int i = 0; i < desc.Length; i++)
				array[i] = desc[i];
			array[^1] = newDesc;

			return array;
		}

		public static void WriteAll(StreamWriter writer, string @namespace, string endsWith, object[] args)
		{
			var infos = assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface && t.Namespace == @namespace && t.Name.EndsWith(endsWith));

			bool first = true;
			foreach (var info in infos)
			{
				var attrib = info.GetCustomAttribute(typeof(DescAttribute));
				var name = info.Name.Replace(endsWith, "");

				HTMLWriter.WriteHeader(writer, name, 3);

				Write(writer, info.FullName, args);

				Console.Write((first ? "" : ", ") + name);
				first = false;
			}
			Console.WriteLine();
		}

		static string getNameOfType(string name)
		{
			name = name.Replace("Single", "Float");
			name = name.Replace("Int32", "Integer");

			return name;
		}
	}
}
