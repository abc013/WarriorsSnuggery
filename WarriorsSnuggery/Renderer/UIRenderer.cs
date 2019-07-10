/*
 * User: Andreas
 * Date: 25.11.2017
 * 
 */
using OpenTK;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public static class UIRenderer
	{
		static Game game;

		static Matrix4 matrix;

		static readonly List<IRenderable> beforeRender = new List<IRenderable>();
		static readonly List<IRenderable> afterRender = new List<IRenderable>();

		static ImageRenderable cursor;

		public static void Reset(Game game)
		{
			if (cursor == null)
			{
				cursor = new ImageRenderable(TextureManager.Texture("cursor"));
			}
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

			game.ScreenControl.Render();

			game.RenderDebug();

			foreach (var r in afterRender)
				r.Render();

			//Graphics.ColorManager.DrawLine(CPos.Zero, MouseInput.WindowPosition, Color.Red);
			if (!Settings.EnableDebug)
			{
				cursor.SetPosition(MouseInput.WindowPosition + new CPos(240, 240, 0));
				cursor.Render();
			}
			else
			{
				Graphics.ColorManager.DrawRect(new CPos(-64, -64, 0), new CPos(64, 64, 0), Color.Cyan);
				Graphics.ColorManager.DrawRect(MouseInput.WindowPosition + new CPos(-64, -64, 0), MouseInput.WindowPosition + new CPos(64, 64, 0), Color.Blue);
			}
		}
	}
}
