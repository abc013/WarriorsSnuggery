using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;

namespace WarriorsSnuggery.Audio
{
	public class AudioSource
	{
		public AudioBuffer Buffer { get; private set; }
		public bool Used;
		public bool Loops;

		uint source;
		int ticksRemaining;

		public AudioSource()
		{
			AL.GenSource(out source);
		}

		public void Tick()
		{
			if (ticksRemaining-- <= 0)
			{
				if (Loops)
				{
					Start(Buffer);
				}
				else
				{
					Buffer = null;
					Used = false;
				}
			}
		}

		public void Start(AudioBuffer buffer)
		{
			Used = true;
			Buffer = buffer;
			ticksRemaining = buffer.Length;

			AL.BindBufferToSource(source, buffer.GetID());
			AL.SourcePlay(source);
		}

		public void Stop()
		{
			AL.SourceStop(source);

			Used = false;
			Buffer = null;
			ticksRemaining = 0;
		}

		public void Pause(bool pause)
		{
			if (pause)
				AL.SourcePause(source);
			else
				AL.SourcePlay(source);
		}

		public void Dispose()
		{
			AL.DeleteSource(ref source);
		}
	}
}
