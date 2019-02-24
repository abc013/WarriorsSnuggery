﻿/*
 * User: Andreas
 * Date: 04.07.2018
 * Time: 18:43
 */
using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class ColoredLine : GameObject
	{
		public ColoredLine(CPos pos, Color color, float size = 1f) : 
			base(pos, new ColoredLineRenderable(color, size))
		{ }

		public void SetColor(Color color)
		{
			Renderable.setColor(color);
		}
	}

	class ColoredLineRenderable : GraphicsObject
	{
		public ColoredLineRenderable(Color color, float size) : 
			base(new IColor(ColoredMesh.Line(size, color), DrawMethod.LINES))
		{ }
	}
}
