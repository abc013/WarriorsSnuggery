using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;
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

		[Save, DefaultValue(false)]
		public readonly bool IsPlayer;
		[Save, DefaultValue(false)]
		public readonly bool IsBot;
		public readonly bool IsPlayerSwitch;

		[Save, DefaultValue(true)]
		public bool IsAlive { get; private set; } = true;
		[Save, DefaultValue(false)]
		bool spawning;
		[Save, DefaultValue(false)]
		bool despawning;

		public ActorSector Sector;

		[Save, DefaultValue((byte)0)]
		public byte Team;
		[Save, DefaultValue(0.0f)]
		public float Angle;

		// Saved with custom method
		readonly List<ActorEffect> effects = new List<ActorEffect>();

		readonly PartManager partManager;

		readonly List<ITick> tickParts;
		readonly List<ITickInEditor> editorTickParts;
		readonly List<IRenderable> renderParts;
		readonly List<INoticeBasicChanges> basicChangesParts;
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
		// Saved with custom method
		readonly Dictionary<ActionType, int> actionTimings = new Dictionary<ActionType, int>();

		public bool CanMove
		{
			get
			{
				if (!Pushable || Motor == null)
					return false;

				if (!Mobile.CanFly && !OnGround)
					return false;

				if (EffectActive(EffectType.STUN))
					return false;

				return IsAlive;
			}
		}
		public bool Pushable => !(Mobile == null || !IsAlive);

		public bool CanAttack
		{
			get
			{
				if (Weapon == null || !Weapon.Ready || !IsAlive)
					return false;

				if (DoesAction(ActionType.MOVE) && !Weapon.AllowMoving)
					return false;

				if (EffectActive(EffectType.STUN))
					return false;

				return IsAlive;
			}
		}

		[Save]
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				TerrainPosition = Position.ToMPos();
				CurrentTerrain = World.TerrainAt(TerrainPosition);

				foreach (var part in basicChangesParts)
					part.SetPosition(value);
			}
		}

		public MPos TerrainPosition { get; private set; }
		public Terrain CurrentTerrain { get; private set; }

        public override float Scale
		{
			get => base.Scale;
			set
			{
				base.Scale = value;

				foreach (var part in basicChangesParts)
					part.SetScale(value);
			}
		}

		public override VAngle Rotation
		{
			get => base.Rotation;
			set
			{
				base.Rotation = value;

				foreach (var part in basicChangesParts)
					part.SetRotation(value);
			}
		}

		public override Color Color
		{
			get => base.Color;
			set
			{
				base.Color = value;

				foreach (var part in basicChangesParts)
					part.SetColor(value);
			}
		}

		public override TextureFlags TextureFlags
		{
			get => base.TextureFlags;
			set
			{
				base.TextureFlags = value;

				foreach (var part in basicChangesParts)
					part.SetTextureFlags(value);
			}
		}

		public Actor(World world, ActorInit init, uint overrideID) : this(world, init)
		{
			ID = overrideID;
		}

		public Actor(World world, ActorInit init)
		{
			World = world;

			Type = init.Type;
			Team = init.Team;
			IsPlayer = init.IsPlayer;
			IsBot = init.IsBot;

			ID = init.ID;
			this.init = init;

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
				partManager.Add(new PlayerPart(this, new PlayerPartInfo(new PartInitSet("PlayerPart", new List<TextNode>()))));

			tickParts = partManager.GetPartsOrDefault<ITick>();
			editorTickParts = partManager.GetPartsOrDefault<ITickInEditor>();
			renderParts = partManager.GetPartsOrDefault<IRenderable>();
			basicChangesParts = partManager.GetPartsOrDefault<INoticeBasicChanges>();
			accelerationParts = partManager.GetPartsOrDefault<INoticeAcceleration>();
			moveParts = partManager.GetPartsOrDefault<INoticeMove>();
			stopParts = partManager.GetPartsOrDefault<INoticeStop>();

			Physics = Type.Physics == null ? SimplePhysics.Empty : new SimplePhysics(this, Type.Physics.Type);

			Position = init.Position;
		}

		public void OnLoad()
		{
			foreach (var part in partManager.GetPartsOrDefault<ISaveLoadable>())
				part.OnLoad(init.MakeInitializerWith(init, (ActorPart)part));

			foreach (var effectInit in init.MakeInitializersWith(nameof(ActorEffect)))
				effects.Add(new ActorEffect(this, effectInit));

			foreach (var actionTimingInit in init.MakeInitializersWith("ActionTiming"))
			{
				var type = actionTimingInit.Convert("Action", ActionType.IDLE);
				var timing = actionTimingInit.Convert("Timing", 0);

				actionTimings.Add(type, timing);
			}

			var hoverPart = GetPartOrDefault<HoverPart>();
			if (hoverPart != null)
				Position = new CPos(Position.X, Position.Y, hoverPart.DefaultHeight);

			if (!DoesAction(ActionType.SPAWN) && WorldPart != null && WorldPart.SpawnDelay > 0)
			{
				AddAction(ActionType.SPAWN, WorldPart.SpawnDelay);
				IsAlive = false; // Set temporarily like this
				spawning = true;
			}

			init = null;
		}

		public TextNodeSaver Save()
		{
			var saver = new TextNodeSaver();
			saver.AddSaveFields(this);

			foreach (var part in partManager.GetPartsOrDefault<ISaveLoadable>())
				part.OnSave().SaveUsing(saver);

			foreach (var effect in effects)
				saver.AddChildren(nameof(ActorEffect), effect.Save());

			foreach (var (action, timing) in actionTimings)
			{
				var actionSaver = new TextNodeSaver();
				actionSaver.Add("Action", action);
				actionSaver.Add("Timing", timing);

				saver.AddChildren("ActionTiming", actionSaver);
			}

			return saver;
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

		internal void Move(CPos old)
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

		internal void StopMove()
		{
			foreach (var part in stopParts)
				part.OnStop();
		}

		public ActorEffect CastEffect(Effect effect)
		{
			if (World.Game.Editor)
				return null;

			var actorEffect = new ActorEffect(this, effect);
			effects.Add(actorEffect);

			return actorEffect;
		}

		public IEnumerable<ActorEffect> GetActiveEffects(EffectType type) => effects.Where(e => e.Effect.Type == type && !e.Sleeping);
		public bool EffectActive(EffectType type) => effects.Any(e => e.Effect.Type == type);

		public override void Render()
		{
			if (EffectActive(EffectType.INVISIBILITY))
				return;

			base.Render();

			foreach (var part in renderParts)
				part.Render();
		}

		public void RenderDebug()
		{
			ColorManager.DrawDot(Position, Color.Blue);
			if (Weapon != null)
				ColorManager.DrawCircle(Position, Weapon.Type.MaxRange / 1024f * 2, Color.Red);
		}

		public override void Tick()
		{
			processActions();

			if (!IsAlive)
			{
				if (!spawning && !despawning)
					return;

				if (spawning && !DoesAction(ActionType.SPAWN))
				{
					IsAlive = true; // Action is completed, we are alive
					spawning = false;
				}

				if (despawning && !DoesAction(ActionType.DESPAWN))
				{
					Dispose(); // Action is completed, disposing
					despawning = false;
				}
			}

			base.Tick();

			if (World.Game.Editor)
			{
				foreach (var part in editorTickParts)
					part.Tick();

				return;
			}

			foreach (var part in tickParts)
			{
				part.Tick();

				// It's possible that the actor may die because of specific traits, because of that, skip the others.
				// We still need the parts for spawning and despawning since render parts are tied to tick
				if (!IsAlive && !spawning && !despawning)
					break;
			}

			processEffects();
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

			foreach (var (action, timing) in actionTimings)
			{
				if (timing > 0)
				{
					actionTimings[action]--;
					newActions |= action;
				}
			}

			Actions = newActions;
		}

		public bool DoesAction(ActionType type)
		{
			if (type == Actions)
				return true;

			return (Actions & type) != 0;
		}

		public void AddAction(ActionType type, int duration)
		{
			if (actionTimings.ContainsKey(type))
				actionTimings[type] = duration;
			else
				actionTimings.Add(type, duration);
		}

		public void CancelAction(ActionType type)
		{
			if (actionTimings.ContainsKey(type))
				actionTimings[type] = 0;
		}

		public void PrepareAttack(Actor target)
		{
			PrepareAttack(new Target(target));
		}

		public void PrepareAttack(CPos target)
		{
			PrepareAttack(new Target(target));
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

			Weapon.OrderAttack(target);
		}

		internal void AttackWith(Target target, Weapon weapon)
		{
			World.Add(weapon);

			foreach (var part in partManager.GetPartsOrDefault<INoticeAttack>())
				part.OnAttack(target.Position, weapon);
		}

		internal void Kill(Actor killed)
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

			if (!Health.ImmuneToVampirism && attacker.Health != null && attacker.EffectActive(EffectType.VAMPIRISM))
			{
				var healthGained = (float)damage;
				foreach(var effect in attacker.GetActiveEffects(EffectType.VAMPIRISM))
					healthGained *= effect.Effect.Value;

				attacker.Health.HP += (int)healthGained;
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

		public void Killed(Actor killer)
		{
			if (World.Game.Editor)
				return;

			if (!IsAlive)
				return;

			if (killer != null)
				killer.Kill(this);

			IsAlive = false;

			foreach (var part in partManager.GetPartsOrDefault<INoticeKilled>())
				part.OnKilled(killer);

			if (WorldPart != null && WorldPart.DespawnDelay > 0)
			{
				AddAction(ActionType.DESPAWN, WorldPart.DespawnDelay);

				// Already disconnect physics
				Physics.RemoveSectors();
				despawning = true;
			}
			else
				Dispose();
		}

		public T GetPart<T>() => partManager.GetPart<T>();
		public T GetPartOrDefault<T>() => partManager.GetPartOrDefault<T>();
		public List<T> GetParts<T>() => partManager.GetParts<T>();
		public List<T> GetPartsOrDefault<T>() => partManager.GetPartsOrDefault<T>();

		public override string ToString()
		{
			return ID.ToString();
		}

		public override void Dispose()
		{
			base.Dispose();

			foreach (var part in partManager.GetPartsOrDefault<INoticeDispose>())
				part.OnDispose();
			World.ActorLayer.Remove(this);
		}
	}
}
