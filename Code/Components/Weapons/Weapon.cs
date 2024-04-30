using Sandbox;

public partial class Weapon : Component
{
	[RequireComponent] Interactable Interactable { get; set; }
	[RequireComponent] Rigidbody Rigidbody { get; set; }
	[RequireComponent] Collider MainCollider { get; set; }

	/// <summary>
	/// Where do the bullets come from?
	/// </summary>
	[Property] public GameObject MuzzleGameObject { get; set; }

	/// <summary>
	/// Rounds per minute
	/// </summary>
	[Property] public float RPM { get; set; }
	[Property] public float MaxRange { get; set; } = 100000;
	[Property] public float BulletSize { get; set; } = 4;

	[Property] public SoundEvent ShootSound { get; set; }

	/// <summary>
	/// How long has it been since we've shot this gun?
	/// </summary>
	public TimeSince TimeSinceShoot { get; private set; }

	protected float RPMToSeconds()
	{
		return 60 / RPM;
	}

	protected virtual Ray WeaponRay => new Ray( MuzzleGameObject.Transform.Position, MuzzleGameObject.Transform.Rotation.Forward );

	protected virtual IEnumerable<SceneTraceResult> GetShootTrace()
	{
		var tr = Scene.Trace.Ray( WeaponRay, MaxRange )
			.Size( BulletSize )
			.WithoutTags( "trigger" )
			.UseHitboxes()
			.Run();

		yield return tr;
	}

	public bool CanShoot()
	{
		// Delay checks
		if ( TimeSinceShoot < RPMToSeconds() )
		{
			return false;
		}

		return true;
	}

	private LegacyParticleSystem CreateParticleSystem( string particle, Vector3 pos, Rotation rot, List<ParticleControlPoint> cps = null, float decay = 5f )
	{
		var gameObject = Scene.CreateObject();
		gameObject.Transform.Position = pos;
		gameObject.Transform.Rotation = rot;

		var p = gameObject.Components.Create<LegacyParticleSystem>();
		p.Particles = ParticleSystem.Load( particle );
		p.ControlPoints = cps ?? new()
		{
			new() { Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = pos }
		};

		// Clear off in a suitable amount of time.
		// TODO: destroy async util
		// gameObject.DestroyAsync( decay );

		return p;
	}

	private void CreateImpactEffects( GameObject hitObject, Surface surface, Vector3 pos, Vector3 normal )
	{
		var decalPath = Game.Random.FromList( surface.ImpactEffects.BulletDecal, "decals/bullethole.decal" );
		if ( ResourceLibrary.TryGet<DecalDefinition>( decalPath, out var decalResource ) )
		{
			CreateParticleSystem( Game.Random.FromList( surface.ImpactEffects.Bullet ), pos, Rotation.LookAt( -normal ) );

			var decal = Game.Random.FromList( decalResource.Decals );

			var gameObject = Scene.CreateObject();
			gameObject.Transform.Position = pos;
			gameObject.Transform.Rotation = Rotation.LookAt( -normal );

			// Random rotation
			gameObject.Transform.Rotation *= Rotation.FromAxis( Vector3.Forward, decal.Rotation.GetValue() );

			var decalRenderer = gameObject.Components.Create<DecalRenderer>();
			decalRenderer.Material = decal.Material;
			decalRenderer.Size = new( decal.Width.GetValue(), decal.Height.GetValue(), decal.Depth.GetValue() );

			// Creates a destruction component to destroy the gameobject after a while
			// TODO: destroy async util
			//gameObject.DestroyAsync( 3f );
		}

		if ( !string.IsNullOrEmpty( surface.Sounds.Bullet ) )
		{
			// TODO: play bullet impact sound
			// hitObject.PlaySound( surface.Sounds.Bullet );
		}
	}

	/// <summary>
	/// Do shoot effects
	/// </summary>
	protected void DoShootEffects()
	{
		if ( ShootSound is not null )
		{
			if ( Sound.Play( ShootSound, MuzzleGameObject.Transform.Position ) is SoundHandle snd )
			{
				snd.ListenLocal = !IsProxy;
			}
		}
	}

	protected void DoTracer( Vector3 startPosition, Vector3 endPosition, float distance, int count )
	{
		var effectPath = "particles/gameplay/guns/trail/trail_smoke.vpcf";

		// For when we have bullet penetration implemented.
		if ( count > 0 )
		{
			effectPath = "particles/gameplay/guns/trail/rico_trail_smoke.vpcf";

			// Project backward
			Vector3 dir = (startPosition - endPosition).Normal;
			var tr = Scene.Trace.Ray( endPosition, startPosition + (dir * 50f) )
				.Radius( 1f )
				.WithoutTags( "weapon" )
				.Run();

			if ( tr.Hit )
			{
				CreateImpactEffects( tr.GameObject, tr.Surface, tr.StartPosition, dir );
			}
		}

		var origin = count == 0 ? MuzzleGameObject.Transform.Position : startPosition;

		// What in tarnation is this 
		CreateParticleSystem( effectPath, startPosition, Rotation.Identity, new()
		{
			new() { StringCP = "0", Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = origin },
			new() { StringCP = "1", Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = endPosition },
			new() { StringCP = "2", Value = ParticleControlPoint.ControlPointValueInput.Float, FloatValue = distance }
		}, 3f );
	}

	protected override void OnUpdate()
	{
		if ( Interactable?.PrimaryGrabPoint?.HeldHand is { } hand )
		{
			if ( hand.IsTriggerDown() )
			{
				Shoot();
			}
		}
	}

	public void Shoot()
	{
		if ( !CanShoot() ) return;

		TimeSinceShoot = 0;

		int count = 0;
		foreach ( var tr in GetShootTrace() )
		{
			// TODO: hurt players and stuff

			Rigidbody.ApplyForceAt( MuzzleGameObject.Transform.Position, GameObject.Transform.Rotation.Up * 25000f );
			Rigidbody.ApplyForceAt( MuzzleGameObject.Transform.Position, GameObject.Transform.Rotation.Left * 35000f );

			if ( tr.Hit )
			{
				CreateImpactEffects( tr.GameObject, tr.Surface, tr.EndPosition, tr.Normal );
			}

			DoShootEffects();
			DoTracer( tr.StartPosition, tr.EndPosition, tr.Distance, count );
			count++;
		}
	}
}
