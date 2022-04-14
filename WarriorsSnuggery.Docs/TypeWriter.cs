using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WarriorsSnuggery.Docs
{
	public static class TypeWriter
	{
		static Assembly assembly;

		public static void Initialize()
		{
			assembly = Assembly.Load("WarriorsSnuggery");
			Settings.IgnoreRequiredAttribute = true;
		}

		public static string Write(Type type, object[] args)
		{
			var builder = new StringBuilder();

			var attrib = type.GetCustomAttribute<DescAttribute>();
			var description = attrib?.Desc;
			if (description != null)
				builder.AppendLine(DocumentationUtils.Description(description));

			var obj = Activator.CreateInstance(type, args);
			var variables = type.GetFields().Where(f => f.IsInitOnly && (f.GetCustomAttribute<DescAttribute>() != null || f.GetCustomAttribute<RequireAttribute>() != null));
			var cells = new List<TableCell>();

			foreach (var variable in variables)
			{
				var varname = variable.Name;
				var vartype = getNameOfType(variable.FieldType.Name);
				var vardesc = getDescription(variable);
				var value = getValue(variable, obj);

				cells.Add(new TableCell(varname, vartype, vardesc, value));
			}

			if (cells.Count > 0)
				builder.AppendLine(DocumentationUtils.Table(cells, true));

			return builder.ToString();
		}

		static string[] getDescription(FieldInfo variable)
		{
			var desc = variable.GetCustomAttribute<DescAttribute>()?.Desc;

			var type = variable.FieldType.IsArray ? variable.FieldType.GetElementType() : variable.FieldType;

			string enumDesc = null;
			if (type.IsEnum)
				enumDesc = "Available options: " + string.Join(", ", Enum.GetNames(type));

			string requiredDesc = null;
			if (variable.GetCustomAttribute<RequireAttribute>() != null)
				requiredDesc = "<i style='color: #d22'>This field must be declared.</i>";

			var array = new string[(desc != null ? desc.Length : 0) + (enumDesc != null ? 1 : 0) + (requiredDesc != null ? 1 : 0)];

			if (desc != null)
			{
				for (int i = 0; i < desc.Length; i++)
					array[i] = desc[i];
			}

			if (enumDesc != null)
				array[^2] = enumDesc;

			if (requiredDesc != null)
				array[^1] = requiredDesc;

			return array;
		}

		public static string WriteAll(string @namespace, string endsWith, object[] args)
		{
			var builder = new StringBuilder();

			var infos = assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface && t.Namespace == @namespace && t.Name.EndsWith(endsWith));

			bool first = true;
			foreach (var info in infos)
			{
				var attrib = info.GetCustomAttribute(typeof(DescAttribute));
				var name = info.Name.Replace(endsWith, "");

				builder.AppendLine(DocumentationUtils.Header(name, 3));
				builder.AppendLine(Write(info, args));

				Console.Write((first ? "" : ", ") + name);
				first = false;
			}
			Console.WriteLine();

			return builder.ToString();
		}

		static string getNameOfType(string name)
		{
			name = name.Replace("Single", "Float");
			name = name.Replace("Int32", "Integer");
			name = name.Replace("UInt16", "Positive Integer");

			return name;
		}

		static string getValue(FieldInfo info, object obj)
		{
			var value = info.GetValue(obj);

			if (value == null)
				return "Not given";

			if (info.FieldType.IsArray)
			{
				var result = string.Empty;

				var list = value as IEnumerable;

				foreach(var listObj in list)
					result += $"{listObj}, ";

				return string.IsNullOrEmpty(result) ? "Not given" : result.Remove(result.Length - 2);
			}
			else if (info.FieldType == typeof(Color))
			{
				var color = (Color)value;
				return $"{color.R}, {color.G}, {color.B}, {color.A}";
			}

			return value.ToString();
		}
	}
}
