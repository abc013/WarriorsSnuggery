using System;

namespace WarriorsSnuggery.Scripting
{
	[Serializable]
	public class MissingScriptException : Exception
	{
		public MissingScriptException(string script) : base($"The script '{script}' does not contain a valid class to start from. The class must inherit 'MissionScriptBase'.") { }
	}
}
