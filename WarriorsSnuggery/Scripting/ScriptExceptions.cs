using System;
using System.Runtime.Serialization;

namespace WarriorsSnuggery.Scripting
{

	[Serializable]
	public class MissingScriptException : Exception
	{
		public MissingScriptException() { }

		public MissingScriptException(string script) : base(string.Format(@"The script '{0}' does not contain a valid class to start from. The class must inherit 'MissionScriptBase'.", script)) { }

		protected MissingScriptException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
