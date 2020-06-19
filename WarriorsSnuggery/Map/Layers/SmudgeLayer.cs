using System.Collections.Generic;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery
{
	public sealed class SmudgeLayer : ITickRenderDisposable
	{
		readonly List<Smudge> smudgeList = new List<Smudge>();

		public SmudgeLayer() { }

		public void Add(Smudge smudge)
		{
			smudgeList.Add(smudge);

			if (smudgeList.Count > 128)
			{
				for (int i = 0; i < smudgeList.Count; i++)
				{
					if (!smudgeList[i].IsDissolving)
					{
						smudgeList[i].BeginDissolve();
						break;
					}
				}
			}
		}

		public void Render()
		{
			foreach (var smudge in smudgeList)
				smudge.Render();
		}

		public void Tick()
		{
			for (int i = 0; i < smudgeList.Count; i++)
			{
				var smudge = smudgeList[i];
				smudge.Tick();
			}

			smudgeList.RemoveAll(s => s.Disposed);
		}

		public void Dispose()
		{
			foreach (var smudge in smudgeList)
				smudge.Disposed = true;
			smudgeList.Clear();
		}
	}
}
