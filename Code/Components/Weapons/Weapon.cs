using Sandbox;

public partial class Weapon : Interactable
{
	/// <summary>
	/// Where do the bullets come from?
	/// </summary>
	[Property, Group( "Setup" )] public GameObject MuzzleGameObject { get; set; }

	/// <summary>
	/// Rounds per minute that this weapon can fire.
	/// </summary>
	[Property, Group( "Stats" )] public float RPM { get; set; }

	/// <summary>
	/// How far can this bullet travel?
	/// </summary>
	[Property, Group( "Stats" )] public float MaxRange { get; set; } = 100000;

	/// <summary>
	/// How big (radius) is this bullet?
	/// </summary>
	[Property, Group( "Stats" )] public float BulletSize { get; set; } = 4;

	/// <summary>
	/// What sound should we play when shooting the gun?
	/// </summary>
	[Property, Group( "Sounds" )] public SoundEvent ShootSound { get; set; }

	/// <summary>
	/// What sound should we play when trying to dry fire?
	/// </summary>
	[Property, Group( "Sounds" )] public SoundEvent DryFireSound { get; set; }

	/// <summary>
	/// The current weapon magazine
	/// </summary>
	public WeaponMagazine Magazine { get; private set; }

	/// <summary>
	/// Optional: Uses slide release system if we have one.
	/// </summary>
	[Property] public SlideReleaseSystem SlideReleaseSystem { get; set; }

	/// <summary>
	/// How long has it been since we've shot this gun?
	/// </summary>
	[Sync] public TimeSince TimeSinceShoot { get; private set; }

	/// <summary>
	/// When we're trying to dry fire, we want to delay playing any sound
	/// </summary>
	[Sync] public TimeUntil TimeUntilNextDryFire { get; private set; }

	/// <summary>
	/// Called when an attachable is added to this weapon. (From <see cref="Interactable"/>)
	/// </summary>
	/// <param name="attachable"></param>
	/// <param name="attachmentPoint"></param>
	protected override void OnAttachableAdded( Attachable attachable, AttachmentPoint attachmentPoint )
	{
		// When an attachable is added to this weapon (an attachment point on it), check to see if it's a magazine
		// and specify to the weapon that this is its ammo reserve.
		if ( attachable.Components.Get<WeaponMagazine>() is { } magazine )
		{
			Magazine = magazine;
		}
	}

	/// <summary>
	/// Called when an attachble is removed from this weapon. From (<see cref="Interactable"/>)
	/// </summary>
	/// <param name="attachable"></param>
	/// <param name="attachmentPoint"></param>
	protected override void OnAttachableRemoved( Attachable attachable, AttachmentPoint attachmentPoint )
	{
		// If the magazine gets detached, we don't have an ammo reserve anymore.
		if ( attachable.Components.Get<WeaponMagazine>() is { } magazine )
		{
			Magazine = null;
		}
	}

	private Vector2 CurrentRecoilAmount { get; set; }

	protected override Rotation GetHoldRotation()
	{
		var original = base.GetHoldRotation();

		original = original * Rotation.From( -CurrentRecoilAmount.y * Time.Delta, CurrentRecoilAmount.x * Time.Delta, 0 );

		return original;
	}

	/// <summary>
	/// If there's a magazine, we'll try to detach it from its attachment point.
	/// </summary>
	protected void TryDetachMagazine()
	{
		Magazine?.Attachable?.Detach();
	}

	/// <summary>
	/// A conversion from RPM (rounds per minute) to a fire rate in seconds.
	/// </summary>
	/// <returns></returns>
	protected float RPMToSeconds()
	{
		return 60 / RPM;
	}

	/// <summary>
	/// The weapon's forward direction, originating normally from the muzzle, straight forward.
	/// </summary>
	protected virtual Ray WeaponRay => new Ray( MuzzleGameObject.Transform.Position, MuzzleGameObject.Transform.Rotation.Forward );

	/// <summary>
	/// Produces a trace that we can use for weapon shooting.
	/// This is designed in a way where we can override it for a shotgun, and supply multiple shots for pellets.
	/// </summary>
	/// <returns></returns>
	protected virtual IEnumerable<SceneTraceResult> GetShootTrace()
	{
		var tr = Scene.Trace.Ray( WeaponRay, MaxRange )
			.Size( BulletSize )
			.WithoutTags( "trigger" )
			.UseHitboxes()
			.Run();

		yield return tr;
	}

	/// <summary>
	/// Can we shoot this gun?
	/// </summary>
	/// <returns></returns>
	public bool CanShoot()
	{
		if ( SlideReleaseSystem?.IsPulled ?? false )
		{
			return false;
		}

		// Delay checks
		if ( TimeSinceShoot < RPMToSeconds() )
		{
			return false;
		}

		return Magazine?.TakeBullet() ?? false;
	}

	/// <summary>
	/// Does the current magazine have any ammo in it?
	/// </summary>
	/// <returns></returns>
	public bool HasAmmo()
	{
		return ( Magazine?.BulletCount ?? 0 ) > 0;
	}

	protected override void OnUpdate()
	{
		CurrentRecoilAmount = CurrentRecoilAmount.LerpTo( 0, Time.Delta * CalcRecoilDecay() );

		if ( PrimaryGrabPoint?.HeldHand is { } hand )
		{
			if ( hand.IsTriggerDown() )
			{
				if ( !HasAmmo() )
				{
					TryDryShoot();
					return;
				}

				TryShoot();
			}

			if ( hand.GetController().ButtonB.IsPressed )
			{
				TryDetachMagazine();
			}
		}
	}

	[Broadcast]
	private void TryDryShoot()
	{
		if ( !TimeUntilNextDryFire ) return;

		Sound.Play( DryFireSound, Transform.Position );
		TimeUntilNextDryFire = 0.5f;
	}

	[Property, Group( "Recoil" )] public RangedFloat VerticalRecoil { get; set; } = new( 300, 600 );
	[Property, Group( "Recoil" )] public RangedFloat HorizontalRecoil { get; set; } = new( -200, 200 );

	private Vector2 CalcRecoil()
	{
		return new Vector2()
		{
			y = Game.Random.Int( VerticalRecoil.x.CeilToInt(), VerticalRecoil.y.CeilToInt() ),
			x = Game.Random.Int( HorizontalRecoil.x.CeilToInt(), HorizontalRecoil.y.CeilToInt() )
		};
	}

	private float CalcRecoilDecay()
	{
		return 3f;
	}

	[Broadcast]
	public void TryShoot()
	{
		if ( !CanShoot() ) return;

		TimeSinceShoot = 0;

		CurrentRecoilAmount += CalcRecoil();

		// did we just run out of ammo?
		if ( !HasAmmo() )
		{
			// trigger the slide
			SlideReleaseSystem?.TriggerEmpty();
		}

		int count = 0;
		foreach ( var tr in GetShootTrace() )
		{
			// TODO: don't do random forces, wtf
			Rigidbody.ApplyForceAt( MuzzleGameObject.Transform.Position, GameObject.Transform.Rotation.Up * 25000f );
			Rigidbody.ApplyForceAt( MuzzleGameObject.Transform.Position, GameObject.Transform.Rotation.Left * 35000f );

			// TODO: hurt players and stuff

			if ( tr.Hit )
				CreateImpactEffects( tr.GameObject, tr.Surface, tr.EndPosition, tr.Normal );

			DoShootEffects();
			DoTracer( tr.StartPosition, tr.EndPosition, tr.Distance, count );
			count++;
		}
	}
}
