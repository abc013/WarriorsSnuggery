using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Actors
{
	public interface ITickInEditor : ITick { }

	public interface INoticeAttack
	{
		void OnAttack(CPos target, Weapon weapon);
	}

	public interface INoticeKill
	{
		void OnKill(Actor killed);
	}

	public interface INoticeDamage
	{
		void OnDamage(Actor damager, int damage);
	}

	public interface INoticeKilled
	{
		void OnKilled(Actor killer);
	}

	public interface INoticeMove
	{
		void OnMove(CPos old, CPos speed);
	}

	public interface INoticeStop
	{
		void OnStop();
	}

	public interface INoticeAcceleration
	{
		void OnAccelerate(CPos acceleration);
		int OnAccelerate(float angle, int acceleration);
	}

	public interface INoticeDispose
	{
		void OnDispose();
	}

	public interface ISaveLoadable
	{
		void OnLoad(List<TextNode> nodes);
		PartSaver OnSave();
	}

	public interface IPartRenderable : IRenderable
	{
		int FacingFromAngle(float angle);

		Graphics.BatchRenderable GetRenderable(ActionType actions, int facing);

		void SetColor(Color color);
	}
}
