﻿using WarriorsSnuggery.Objects.Weapons;

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
		void OnLoad(PartLoader loader);
		PartSaver OnSave();
	}

	public interface INoticeBasicChanges
	{
		void SetPosition(CPos pos);

		void SetScale(float scale);

		void SetRotation(VAngle rotation);

		void SetColor(Color color);

		void SetTextureFlags(Graphics.TextureFlags flags);
	}
}
