using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps.Layers;
using WarriorsSnuggery.Objects.Actors.Parts;
using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Physics;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Actors
{
	public sealed class Actor : PhysicsObject
	{
		public const byte PlayerTeam = 2;
		public const byte NeutralTeam = 0;

		public readonly World World;

		public readonly uint ID;

		public readonly bool IsPlayer;
		public readonly bool IsBot;
		public readonly bool IsPlayerSwitch;

		public bool IsAlive { get; private set; } = true;
		public bool IsDisposed => Disposed;

		public ActorSector Sector;

		// Used when changing player for bot targeting.
		public Actor FollowupActor;

		[Save, DefaultValue((byte)0)]
		public byte Team;
		[Save, DefaultValue(0.0f)]
		public float Angle;

		readonly List<ActorEffect> sleepingEffects = new List<ActorEffect>();
		readonly List<ActorEffect> effects = new List<ActorEffect>();

		readonly PartManager partManager;

		readonly List<ITick> tickParts;
		readonly List<ITickInEditor> editorTickParts;
		readonly List<IRenderable> renderParts;
		readonly List<INoticeAcceleration> accelerationParts;
		readonly List<INoticeMove> moveParts;
		readonly List<INoticeStop> stopParts;

		public readonly MobilePart Mobile;
		public readonly MotorPart Motor;
		public readonly HealthPart Health;
		public readonly RevealsShroudPart RevealsShroud;
		public readonly WeaponPart Weapon;
		public readonly WorldPart WorldPart;
		public readonly BotPart Bot;

		[Save, DefaultValue(null)]
		public readonly string ScriptTag;

		[Save]
		public readonly ActorType Type;
		ActorInit init;

		[Save, DefaultValue(ActionType.IDLE)]
		public ActionType Actions { get; private set; } = ActionType.IDLE;
		// TODO: save
		readonly List<ActorAction> actions = new List<ActorAction>();

		bool allowAttackMove => Weapon == null || Weapon.AllowMoving;

		public MPos TerrainPosition => Position.ToMPos();
		public Terrain CurrentTerrain;

		public bool CanMove
		{
			get
			{
				if (!Pushable || Motor == null)
					return false;

				if (!Mobile.CanFly && Height > 0)
					return false;

				if (!ActionPossible(ActionType.PREPARE_MOVE) && !ActionPossible(ActionType.MOVE))
					return false;

				if (EffectActive(EffectType.STUN))
					return false;

				return true;
			}
		}
		public bool Pushable => !(Mobile == null || !IsAlive);

		public bool CanAttack
		{
			get
			{
				if (Weapon == null || !Weapon.ReloadDone || !IsAlive)
					return false;

				if (!ActionPossible(ActionType.PREPARE_ATTACK) && !ActionPossible(ActionType.ATTACK))
					return false;

				if (EffectActive(EffectType.STUN))
					return false;

				return true;
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

			CurrentTerrain = world.TerrainAt(TerrainPosition);

			// Parts
			partManager = new PartManager();
			foreach (var partinfo in init.Type.PartInfos)
			{
				if (partinfo is BotPartInfo && !IsBot)
					continue;

				var part = partinfo.Create(this);
				partManager.Add(part);

				// Cache some important parts
				if (part is MobilePart mobile)
					Mobile = mobile;
				else if (part is MotorPart motor)
					Motor = motor;
				else if (part is HealthPart health)
					Health = health;
				else if (part is RevealsShroudPart revealsShroud)
					RevealsShroud = revealsShroud;
				else if (part is WeaponPart weapon)
					Weapon = weapon;
				else if (part is WorldPart worldPart)
					WorldPart = worldPart;
				else if (part is BotPart botPart)
					Bot = botPart;
				else if (part is PlayerSwitchPart)
					IsPlayerSwitch = true;
			}

			if (IsBot && Bot == null)
				IsBot = false; // BotPart is not there, thus there's no bot

			if (IsPlayer)
				partManager.Add(new PlayerPart(this));

			tickParts = partManager.GetPartsOrDefault<ITick>();
			editorTickParts = partManager.GetPartsOrDefault<ITickInEditor>();
			renderParts = partManager.GetPartsOrDefault<IRenderable>();
			accelerationParts = partManager.GetPartsOrDefault<INoticeAcceleration>();
			moveParts = partManager.GetPartsOrDefault<INoticeMove>();
			stopParts = partManager.GetPartsOrDefault<INoticeStop>();

			Physics = Type.Physics == null ? SimplePhysics.Empty : new SimplePhysics(this, Type.Physics.Type);

			if (Health != null && init.Health >= 0f)
				Health.RelativeHP = init.Health;

			var hoverPart = GetPartOrDefault<HoverPart>();
			if (hoverPart != null)
				Height = hoverPart.DefaultHeight;
		}

		public void OnLoad()
		{
			var partLoader = new PartLoader(init);
			foreach (var part in partManager.GetPartsOrDefault<ISaveLoadable>())
				part.OnLoad(partLoader);

			var effectData = init.Nodes.Where(n => n.Key == nameof(ActorEffect));
			foreach (var effect in effectData)
				effects.Add(new ActorEffect(this, effect.Children));

			init = null;
		}

		public List<string> Save()
		{
			var list = SaveAttribute.GetFields(this);

			foreach (var part in partManager.GetPartsOrDefault<ISaveLoadable>())
				list.AddRange(part.OnSave().GetSave());

			foreach (var effect in effects)
				list.AddRange(effect.Save());

			return list;
		}

		public void Push(float angle, int power)
		{
			if (World.Game.Editor || !Pushable)
				return;

			var acceleration = Mobile.Accelerate(angle, power);
			foreach (var part in accelerationParts)
				part.OnAccelerate(angle, acceleration);
		}

		public void AccelerateSelf(float angle)
		{
			if (World.Game.Editor || !CanMove)
				return;

			Motor.AccelerateSelf(angle);
		}

		public void Lift(int power)
		{
			if (World.Game.Editor || !Pushable)
				return;

			var acceleration = Mobile.AccelerateHeight(power);
			foreach (var part in accelerationParts)
				part.OnAccelerate(new CPos(0, 0, acceleration));
		}

		public void AccelerateHeightSelf(bool up)
		{
			if (World.Game.Editor || !Mobile.CanFly || !CanMove)
				return;

			Motor.AccelerateHeightSelf(up);
		}

		public void Move(CPos old)
		{
			foreach (var part in moveParts)
				part.OnMove(old, Mobile.Velocity);

			foreach (var effect in effects)
			{
				if (effect.Sleeping && effect.Effect.Activation == EffectActivationType.ON_MOVE)
					effect.Sleeping = false;

				effect.OnMove(old, Mobile.Velocity);
			}
		}

		public void StopMove()
		{
			foreach (var part in stopParts)
				part.OnStop();
		}

		public List<ActorEffect> CastSpell(Spell spell)
		{
			var list = new List<ActorEffect>();

			if (World.Game.Editor)
				return list;

			if (spell.Sound != null)
			{
				var sound = new Sound(spell.Sound);
				sound.Play(Position, false);
			}

			for (int i = 0; i < spell.Effects.Length; i++)
			{
				var effect = new ActorEffect(this, spell, i);
				effects.Add(effect);
				list.Add(effect);
			}

			return list;
		}

		public IEnumerable<ActorEffect> GetActiveEffects(EffectType type) => effects.Where(e => e.Effect.Type == type && !e.Sleeping);
		public bool EffectActive(EffectType type) => effects.Any(e => e.Effect.Type == type);

		public override bool CheckVisibility()
		{
			if (Disposed)
				return false;

			if (WorldPart != null)
				Visible = CameraVisibility.IsVisible(GraphicPosition + WorldPart.VisibilityBoxOffset, WorldPart.VisibilityBox);
			else
				Visible = CameraVisibility.IsVisible(GraphicPosition, new MPos(512, 512));

			return Visible;
		}

		public override void Render()
		{
			if (!Visible)
				return;

			if (EffectActive(EffectType.INVISIBILITY))
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
			if (!IsAlive)
				return;

			base.Tick();

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
			foreach (var effect in effects)
				effect.Tick();

			effects.RemoveAll(e => !e.Active && !e.Sleeping);
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

						if (action.ActionOver && action.Following != null && action.Following.CurrentTick != 0)
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
			foreach (var part in partManager.GetPartsOrDefault<IPartRenderable>())
				part.SetColor(color);
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

			if (!CanAttack)
				return;

			Angle = (Position - target.Position).FlatAngle;

			foreach (var effect in effects)
			{
				if (effect.Sleeping && effect.Effect.Activation == EffectActivationType.ON_ATTACK)
					effect.Sleeping = false;
			}

			Weapon.OnAttack(target);
		}

		public bool AttackWith(Target target, Weapon weapon)
		{
			if (!World.Map.Type.AllowWeapons)
				return false;

			World.Add(weapon);

			foreach (var part in partManager.GetPartsOrDefault<INoticeAttack>())
				part.OnAttack(target.Position, weapon);

			return true;
		}

		public void Kill(Actor killed)
		{
			foreach (var part in partManager.GetPartsOrDefault<INoticeKill>())
				part.OnKill(killed);
		}

		public void Damage(Actor attacker, int damage)
		{
			if (World.Game.Editor)
				return;

			if (Health == null || Health.HP <= 0)
				return;

			foreach (var effect in effects)
			{
				if (effect.Sleeping && effect.Effect.Activation == EffectActivationType.ON_DAMAGE)
					effect.Sleeping = false;
			}

			if (EffectActive(EffectType.SHIELD))
			{
				World.AddText(Position, 50, ActionText.ActionTextType.SCALE, new Color(0.4f, 0.4f, 1f).ToString() + "shielded");
				return;
			}

			if (WorldPart != null && WorldPart.ShowDamage)
				World.AddText(Position, 50, ActionText.ActionTextType.SCALE, new Color(1f, 0.4f, 0).ToString() + damage);

			Health.HP -= damage;

			foreach (var part in partManager.GetPartsOrDefault<INoticeDamage>())
				part.OnDamage(attacker, damage);

			if (Health.HP <= 0)
				Killed(attacker);
		}

		public void Damage(int damage)
		{
			Damage(null, damage);
		}

		public void Killed(Actor killer, bool dispose = true)
		{
			if (World.Game.Editor)
				return;

			if (killer != null)
				killer.Kill(this);

			IsAlive = false;

			foreach (var part in partManager.GetPartsOrDefault<INoticeKilled>())
				part.OnKilled(killer);

			if (dispose)
				Dispose();
		}

		public T GetPart<T>() => partManager.GetPart<T>();
		public T GetPartOrDefault<T>() => partManager.GetPartOrDefault<T>();
		public List<T> GetParts<T>() => partManager.GetParts<T>();
		public List<T> GetPartsOrDefault<T>() => partManager.GetPartsOrDefault<T>();

		public override void Dispose()
		{
			base.Dispose();

			foreach (var part in partManager.GetPartsOrDefault<INoticeDispose>())
				part.OnDispose();
			World.ActorLayer.Remove(this);
		}
	}
}
