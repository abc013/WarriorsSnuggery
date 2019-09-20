using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarriorsSnuggery.Graphics
{
	public class Sheet
	{
		public readonly MPos Size;

		public int TextureID;
		float[] data;

		public Sheet(int size)
		{
			Size = new MPos(size, size);
			data = new float[size * size * 4];
		}

		public void CreateTexture()
		{
			// GL
			data = null;
		}
	}
}
