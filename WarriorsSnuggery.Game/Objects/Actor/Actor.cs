using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Actors.Parts;
using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Actors
{
	public sealed class Actor : PhysicsObject
	{
		public const byte PlayerTeam = 2;
		public const byte NeutralTeam = 0;

		public readonly World World;

		public readonly bool IsPlayer;
		public readonly bool IsBot;
		public readonly bool IsPlayerSwitch;
		public readonly uint ID;

		public bool IsAlive = true;

		public ActorSector Sector;

		// Used when changing player for bot targeting.
		public Actor FollowupActor;

		[Save]
		public byte Team;
		[Save]
		public float Angle;

		public readonly List<ActorPart> Parts = new List<ActorPart>();
		public readonly List<ActorEffect> Effects = new List<ActorEffect>();
		public readonly PartManager PartManager;

		readonly List<ITick> tickParts;
		readonly List<ITickInEditor> editorTickParts;
		readonly List<IRenderable> renderParts;
		readonly List<INoticeAcceleration> accelerationParts;
		readonly List<INoticeMove> moveParts;
		readonly List<INoticeStop> stopParts;

		public readonly MobilityPart Mobility;
		public readonly HealthPart Health;
		public readonly RevealsShroudPart RevealsShroud;
		public readonly WeaponPart Weapon;
		public readonly WorldPart WorldPart;
		public readonly BotPart Bot;

		[Save]
		public readonly ActorType Type;
		ActorInit init;

		[Save]
		public ActionType Actions { get; private set; } = ActionType.IDLE;
		// TODO: save
		readonly List<ActorAction> actions = new List<ActorAction>();

		bool allowAttackMove => Weapon == null || Weapon.AllowMoving;

		public MPos TerrainPosition;
		public Terrain CurrentTerrain;

		bool visible;

		CPos Velocity
		{
			get => Mobility == null ? CPos.Zero : Mobility.Velocity;
			set
			{
				if (Mobility != null)
					Mobility.Velocity = value;
			}
		}

		public Actor(World world, ActorInit init, uint overrideID) : this(world, init)
		{
			ID = overrideID;
		}

		public Actor(World world, ActorInit init) : base(init.Position, null)
		{
			World = world;

			Type = init.Type;
			Team = init.Team;
			IsPlayer = init.IsPlayer;
			IsBot = init.IsBot;

			Height = init.Height;

			ID = init.ID;
			this.init = init;

			TerrainPosition = init.Position.ToMPos();
			CurrentTerrain = world.TerrainAt(TerrainPosition);

			// Parts
			foreach (var partinfo in init.Type.PartInfos)
			{
				var part = partinfo.Create(this);
				Parts.Add(part);

				// Cache some important parts
				if (part is MobilityPart mobility)
					Mobility = mobility;
				else if (part is HealthPart health)
					Health = health;
				else if (part is RevealsShroudPart revealsShroud)
					RevealsShroud = revealsShroud;
				else if (part is WeaponPart weapon)
					Weapon = weapon;
				else if (part is WorldPart worldPart)
					WorldPart = worldPart;
				else if (part is PlayerSwitchPart)
					IsPlayerSwitch = true;
			}

			if (IsPlayer)
				Parts.Add(new PlayerPart(this));

			if (IsBot)
			{
				var behavior = WorldPart == null ? Objects.Bot.BotBehaviorType.TYPICAL : WorldPart.BotBehavior;
				Bot = new BotPart(this, behavior);
				Parts.Add(Bot);
			}

			PartManager = new PartManager();
			foreach (var part in Parts)
				PartManager.Add(part);

			tickParts = PartManager.GetOrDefault<ITick>();
			editorTickParts = PartManager.GetOrDefault<ITickInEditor>();
			renderParts = PartManager.GetOrDefault<IRenderable>();
			accelerationParts = PartManager.GetOrDefault<INoticeAcceleration>();
			moveParts = PartManager.GetOrDefault<INoticeMove>();
			stopParts = PartManager.GetOrDefault<INoticeStop>();

			Physics = getPhysics(init.Type);

			if (Health != null && init.Health >= 0f)
				Health.RelativeHP = init.Health;

			if (WorldPart != null)
				Height = WorldPart.DefaultHeight;
		}

		SimplePhysics getPhysics(ActorType type)
		{
			if (type.Physics == null)
				return SimplePhysics.Empty;

			return new SimplePhysics(this, type.Physics.Type);
		}

		public void OnLoad()
		{
			foreach (var part in PartManager.GetOrDefault<ISaveLoadable>())
				part.OnLoad(init.Nodes);

			var effects = init.Nodes.Where(n => n.Key == nameof(ActorEffect));
			foreach (var effect in effects)
				Effects.Add(new ActorEffect(this, effect.Children));

			init = null;
		}

		public List<string> Save()
		{
			var list = WorldSaver.GetSaveFields(this);

			foreach (var part in PartManager.GetOrDefault<ISaveLoadable>())
				list.AddRange(part.OnSave().GetSave());

			foreach (var effect in Effects)
				list.AddRange(effect.Save());

			return list;
		}

		public void Push(float angle, int power)
		{
			if (Mobility == null || World.Game.Editor)
				return;

			var acceleration = Mobility.Accelerate(angle, power);
			foreach (var part in accelerationParts)
				part.OnAccelerate(angle, acceleration);
		}

		public void AccelerateSelf(float angle)
		{
			if (World.Game.Editor)
				return;

			if (!canMove())
				return;

			Mobility.AccelerateSelf(angle);
		}

		public void Lift(int power)
		{
			if (Mobility == null || World.Game.Editor)
				return;

			var acceleration = Mobility.AccelerateHeight(power);
			foreach (var part in accelerationParts)
				part.OnAccelerate(new CPos(0, 0, acceleration));
		}

		public void AccelerateHeightSelf(bool up)
		{
			if (World.Game.Editor)
				return;

			if (!Mobility.CanFly || !canMove())
				return;

			var acceleration = Mobility.AccelerateHeightSelf(up);
			foreach (var part in accelerationParts)
				part.OnAccelerate(new CPos(0, 0, acceleration));
		}

		bool canMove()
		{
			if (Mobility == null || !IsAlive || Height > 0 && !Mobility.CanFly)
				return false;

			if (!ActionPossible(ActionType.PREPARE_MOVE) && !ActionPossible(ActionType.MOVE))
				return false;

			if (Effects.Any(e => e.Active && e.Effect.Type == Spells.EffectType.STUN))
				return false;

			return true;
		}

		public void Move(CPos old)
		{
			foreach (var part in moveParts)
				part.OnMove(old, Velocity);

			foreach (var effect in Effects)
				effect.OnMove(old, Velocity);
		}

		public void StopMove()
		{
			foreach (var part in stopParts)
				part.OnStop();
		}

		public void CastSpell(Spells.Spell spell)
		{
			if (World.Game.Editor)
				return;

			if (spell.Sound != null)
			{
				var sound = new Sound(spell.Sound);
				sound.Play(Position, false);
			}

			for (int i = 0; i < spell.Effects.Length; i++)
				Effects.Add(new ActorEffect(this, spell, i));
		}

		public override bool CheckVisibility()
		{
			if (Disposed)
				return false;

			if (WorldPart != null)
				visible = VisibilitySolver.IsVisible(GraphicPosition + WorldPart.VisibilityBoxOffset, WorldPart.VisibilityBox);
			else
				visible = VisibilitySolver.IsVisible(GraphicPosition, new MPos(512, 512));

			return visible;
		}

		public override void Render()
		{
			if (!visible)
				return;

			if (Effects.Any(e => e.Active && e.Effect.Type == Spells.EffectType.INVISIBILITY))
				return;

			base.Render();

			foreach (var part in renderParts)
				part.Render();
		}

		public void RenderDebug()
		{
			Graphics.ColorManager.DrawDot(Position, Color.Blue);
			if (Weapon != null)
				Graphics.ColorManager.DrawCircle(Position, Weapon.Type.MaxRange / 1024f * 2, Color.Red);
		}

		public override void Tick()
		{
			base.Tick();

			if (!IsAlive)
				return;

			if (World.Game.Editor)
			{
				foreach (var part in editorTickParts)
					part.Tick();

				return;
			}

			foreach (var part in tickParts)
				part.Tick();

			processEffects();

			processActions();
		}

		void processEffects()
		{
			var effectsToRemove = new List<ActorEffect>();
			foreach (var effect in Effects)
			{
				effect.Tick();
				if (!effect.Active)
					effectsToRemove.Add(effect);
			}

			foreach (var effect in effectsToRemove)
				Effects.Remove(effect);
		}

		void processActions()
		{
			var newActions = ActionType.IDLE;

			if (actions.Count != 0)
			{
				var toAdd = new List<ActorAction>();
				var toRemove = new List<ActorAction>();

				foreach (var action in actions)
				{
					if (action.IsOverOrCanceled(Actions, allowAttackMove))
					{
						toRemove.Add(action);

						if (action.ActionOver && action.Following != null)
						{
							toAdd.Add(action.Following);

							newActions |= action.Following.Type;
						}
					}
					else
					{
						newActions |= action.Type;
					}
				}

				actions.RemoveAll(a => toRemove.Contains(a));
				actions.AddRange(toAdd);
			}

			Actions = newActions;
		}

		public bool ActionPossible(ActionType type)
		{
			if (Actions == ActionType.IDLE || allowAttackMove)
				return true;

			if (Actions == type)
				return true;

			if (DoesAction(ActionType.END_ATTACK | ActionType.END_MOVE))
				return false;

			if (type == ActionType.END_ATTACK && DoesAction(ActionType.ATTACK))
				return true;

			if (type == ActionType.END_MOVE && DoesAction(ActionType.MOVE))
				return true;

			if ((type == ActionType.ATTACK || type == ActionType.PREPARE_MOVE || type == ActionType.MOVE) && DoesAction(ActionType.PREPARE_ATTACK))
				return true;

			if ((type == ActionType.MOVE || type == ActionType.PREPARE_ATTACK || type == ActionType.ATTACK) && DoesAction(ActionType.PREPARE_MOVE))
				return true;

			//c ATTACK: ENDATTACK
			//c ENDATTACK: -
			//c STARTATTACK: STARTMOVE, MOVE
			//c MOVE: ENDMOVE
			//c ENDMOVE: -
			//c STARTMOVE: STARTATTACK, ATTACK
			//c IDLE: ALL

			return false;
		}

		public bool DoesAction(ActionType type)
		{
			if (type == ActionType.IDLE && Actions == ActionType.IDLE)
				return true;

			return (Actions & type) != 0;
		}

		public bool AddAction(ActionType type, int duration, ActorAction following = null, bool interruptsOthers = false)
		{
			if (!ActionPossible(type))
				return false;

			var action = new ActorAction(type, duration, following);

			if (interruptsOthers)
				actions.Clear();

			actions.Add(action);

			return true;
		}

		public override void SetColor(Color color)
		{
			foreach (var part in Parts.Where(p => p is RenderablePart))
				((RenderablePart)part).SetColor(color);
		}

		public void PrepareAttack(Actor target)
		{
			PrepareAttack(new Target(target));
		}

		public void PrepareAttack(CPos target, int height)
		{
			PrepareAttack(new Target(target, height));
		}

		public void PrepareAttack(Target target)
		{
			if (World.Game.Editor || !World.Map.Type.AllowWeapons)
				return;

			if (!canAttack())
				return;

			Angle = (Position - target.Position).FlatAngle;

			Weapon.OnAttack(target);
		}

		bool canAttack()
		{
			if (Weapon == null || !Weapon.ReloadDone || !IsAlive)
				return false;

			if (!ActionPossible(ActionType.PREPARE_ATTACK) && !ActionPossible(ActionType.ATTACK))
				return false;

			if (Effects.Any(e => e.Active && e.Effect.Type == Spells.EffectType.STUN))
				return false;

			return true;
		}

		public bool AttackWith(Target target, Weapon weapon)
		{
			if (!World.Map.Type.AllowWeapons)
				return false;

			World.Add(weapon);

			foreach (var part in PartManager.GetOrDefault<INoticeAttack>())
				part.OnAttack(target.Position, weapon);

			return true;
		}

		public void Kill(Actor killed)
		{
			foreach (var part in PartManager.GetOrDefault<INoticeKill>())
				part.OnKill(killed);
		}

		public void Damage(Actor attacker, int damage)
		{
			if (World.Game.Editor)
				return;

			if (Health == null || Health.HP <= 0)
				return;

			if (Effects.Any(e => e.Active && e.Effect.Type == Spells.EffectType.SHIELD))
				return;

			Health.HP -= damage;

			foreach (var part in PartManager.GetOrDefault<INoticeDamage>())
				part.OnDamage(attacker, damage);

			if (Health.HP <= 0)
				Killed(attacker);
		}

		public void Damage(int damage)
		{
			Damage(null, damage);
		}

		public void Killed(Actor killer)
		{
			if (World.Game.Editor)
				return;

			if (killer != null)
				killer.Kill(this);

			IsAlive = false;

			foreach (var part in PartManager.GetOrDefault<INoticeKilled>())
				part.OnKilled(killer);

			Dispose();
		}

		public override void Dispose()
		{
			base.Dispose();

			foreach (var part in PartManager.GetOrDefault<INoticeDispose>())
				part.OnDispose();
			World.ActorLayer.Remove(this);
		}
	}
}
