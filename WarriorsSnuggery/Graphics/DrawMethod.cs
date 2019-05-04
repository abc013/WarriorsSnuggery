/*
 * User: Andreas
 * Date: 29.10.2017
 * 
 */
using OpenTK.Graphics.ES30;

namespace WarriorsSnuggery.Graphics
{
	public enum DrawMethod
	{
		LINES = PrimitiveType.Lines,
		LINELOOP = PrimitiveType.LineLoop,
		TRIANGLE = PrimitiveType.Triangles,
		TRIANGLEFAN = PrimitiveType.TriangleFan,
	}
}
