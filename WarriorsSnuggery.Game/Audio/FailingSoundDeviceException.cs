using System;
using System.Runtime.Serialization;

namespace WarriorsSnuggery.Audio
{
	[Serializable]
	public class FailingSoundDeviceException : Exception
	{
		public FailingSoundDeviceException() { }

		public FailingSoundDeviceException(string text)
		  : base(text)
		{

		}

		public FailingSoundDeviceException(string text, Exception innerException)
			: base(text, innerException)
		{

		}

		protected FailingSoundDeviceException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}
}
