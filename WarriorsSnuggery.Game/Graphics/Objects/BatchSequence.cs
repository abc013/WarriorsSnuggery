using OpenTK.Mathematics;

namespace WarriorsSnuggery.Graphics
{
	public class BatchSequence : BatchRenderable
	{
		readonly BatchObject[] objects;
		readonly int tick;
		readonly bool pauseable;
		int curTick;
		int curObj;

		public BatchSequence(TextureInfo info, bool pauseable = false, bool startRandom = false) : this(info.GetTextures(), info.Tick, pauseable, startRandom) { }

		public BatchSequence(Texture[] textures, int tick, bool pauseable = false, bool startRandom = false) : base(new Vertex[0])
		{
			this.tick = tick;
			this.pauseable = pauseable;
			curTick = tick;

			objects = new BatchObject[textures.Length];
			for (int i = 0; i < textures.Length; i++)
				objects[i] = new BatchObject(textures[i]);

			if (startRandom)
				curObj = Program.SharedRandom.Next(textures.Length);
		}

		public override void Tick()
		{
			if (!(pauseable && MasterRenderer.PauseSequences) && curTick-- <= 0)
			{
				curTick = tick;
				curObj++;
				if (curObj >= objects.Length)
					curObj = 0;
			}
		}

		public override void SetPosition(Vector position)
		{
			base.SetPosition(position);

			if (!CacheOutdated)
				return;

			foreach (var obj in objects)
				obj.SetPosition(position);
		}

		public override void SetScale(Vector3 scale)
		{
			base.SetScale(scale);

			if (!CacheOutdated)
				return;

			foreach (var obj in objects)
				obj.SetScale(scale);
		}

		public override void SetRotation(Quaternion rot3)
		{
			base.SetRotation(rot3);

			if (!CacheOutdated)
				return;

			foreach (var obj in objects)
				obj.SetRotation(rot3);
		}

		public override void SetColor(Color color)
		{
			base.SetColor(color);

			if (!CacheOutdated)
				return;

			foreach (var obj in objects)
				obj.SetColor(color);
		}

		public void SetTick(int current)
		{
			curTick = current % tick;
			curObj = (current / tick) % objects.Length;
		}

		public void Reset()
		{
			curTick = tick;
			curObj = 0;
		}

		public override void Render()
		{
			if (!Visible)
				return;

			objects[curObj].Render();
		}
	}
}
