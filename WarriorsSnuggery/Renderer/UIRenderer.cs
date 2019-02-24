using System;
using System.Collections.Generic;
using OpenTK;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public static class UIRenderer
	{
		static Game game;

		static Matrix4 matrix;

		static readonly List<IRenderable> beforeRender = new List<IRenderable>();
		static readonly List<IRenderable> afterRender = new List<IRenderable>();

		public static void Reset(Game game)
		{

			UIRenderer.game = game;
			Update();
			ClearRenderLists();
		}

		public static void Update()
		{
			matrix = Matrix4.CreateScale(1 / Camera.DefaultZoom * 2 / WindowInfo.Ratio, 1 / Camera.DefaultZoom * 2, 1f);
		}

		public static void ClearRenderLists()
		{
			beforeRender.Clear();
			afterRender.Clear();
		}

		public static void RenderAfter(IRenderable renderable)
		{
			afterRender.Add(renderable);
		}

		public static void RenderBefore(IRenderable renderable)
		{
			beforeRender.Add(renderable);
		}

		public static void RemoveRenderAfter(IRenderable renderable)
		{
			afterRender.Remove(renderable);
		}

		public static void RemoveRenderBefore(IRenderable renderable)
		{
			beforeRender.Remove(renderable);
		}

		public static void Render()
		{
			MasterRenderer.Uniform(MasterRenderer.TextureShader, ref matrix, Color.White);
			MasterRenderer.Uniform(MasterRenderer.ColorShader, ref matrix, Color.White);
			MasterRenderer.Uniform(MasterRenderer.FontShader, ref matrix, Color.White);
			MasterRenderer.Uniform(MasterRenderer.ShadowShader, ref matrix, Color.White);

			foreach (var r in beforeRender)
				r.Render();

			if (game.CurrentScreen != null) game.CurrentScreen.Render();
			if (!Game.Paused)
			{
				foreach (GameObject o in game.UI)
					o.Render();
			}
			game.RenderDebug();

			foreach (var r in afterRender)
				r.Render();
		}
	}
}
