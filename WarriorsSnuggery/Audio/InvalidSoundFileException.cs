using System;
using System.Runtime.Serialization;

namespace WarriorsSnuggery.Audio
{
	[Serializable]
	public class InvalidSoundFileException : Exception
	{
		public InvalidSoundFileException() { }

		public InvalidSoundFileException(string text)
		  : base(text)
		{

		}

		public InvalidSoundFileException(string text, Exception innerException)
			: base(text, innerException)
		{

		}

		protected InvalidSoundFileException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}
}
