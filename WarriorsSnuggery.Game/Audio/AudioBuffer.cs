using OpenTK.Audio.OpenAL;
using System;
using System.Runtime.InteropServices;

namespace WarriorsSnuggery.Audio
{
	public abstract class AudioBuffer
	{
		public abstract int BufferID { get; }

		protected void LoadData(byte[] data, ALFormat format, int sampleRate)
		{
			unsafe
			{
				GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
				IntPtr pointer = pinnedArray.AddrOfPinnedObject();

				AL.BufferData(BufferID, format, pointer, data.Length, sampleRate);

				pinnedArray.Free();
			}
		}

		public abstract void Dispose();
	}
}
