using System;
using System.Runtime.Serialization;

namespace WarriorsSnuggery.Scripting
{
	[Serializable]
	public class MissingScriptException : Exception
	{
		public MissingScriptException() { }

		public MissingScriptException(string script) : base($"The script '{script}' does not contain a valid class to start from. The class must inherit 'MissionScriptBase'.") { }

		protected MissingScriptException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
