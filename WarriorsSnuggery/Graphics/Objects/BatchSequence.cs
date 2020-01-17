﻿namespace WarriorsSnuggery.Graphics
{
	public class BatchSequence : BatchRenderable
	{
		readonly BatchObject[] objects;
		readonly int tick;
		readonly bool pauseable;
		int curTick;
		int curObj;

		// Create empty GraphicsObject
		public BatchSequence(ITexture[] textures, int tick, bool pauseable = false) : base(new Vertex[0], Color.White)
		{
			this.tick = tick;
			this.pauseable = pauseable;
			curTick = tick;

			objects = new BatchObject[textures.Length];
			for (int i = 0; i < textures.Length; i++)
				objects[i] = new BatchObject(textures[i], Color.White);
		}

		public override void PushToBatchRenderer()
		{
			if (!(pauseable && MasterRenderer.PauseSequences) && curTick-- <= 0)
			{
				curTick = tick;
				curObj++;
				if (curObj >= objects.Length)
					curObj = 0;
			}

			if (!Visible)
				return;

			var renderable = objects[curObj];

			renderable.SetPosition(Position);
			renderable.SetScale(Scale);
			renderable.SetRotation(Rotation);
			renderable.SetColor(Color);
			renderable.PushToBatchRenderer();
		}
	}
}