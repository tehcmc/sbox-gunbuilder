
public partial class SlideReleaseSystem : Component
{
	[RequireComponent] PointInteractable PointInteractable { get; set; }
	[Property] public Weapon Weapon { get; set; }
	[Property] public GrabPoint InputGrabPoint { get; set; }
	[Property] public GrabPoint SlideGrabPoint { get; set; }

	[Property] public bool SlideCatch { get; protected set; }
	WeaponMagazine Magazine => Weapon?.Magazine;

	public bool IsPulled => PointInteractable.CompletionValue.AlmostEqual( 1f );



	bool slideCaught = false;
	bool slideBack = false;
	protected override void OnStart()
	{
		PointInteractable.OnCompletionValue += OnCompletionValue;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="before"></param>
	/// <param name="after"></param>
	public void OnCompletionValue( float before, float after )
	{
		if ( after == 0 )
		{
			Weapon.TryFeedFromMagazine();
		}
	}

	public void OnBulletEjected( IEnumerable<Bullet> ejected )
	{
		//
	}

	public void TriggerEmpty()
	{
		if(Magazine is not null && !Magazine.HasAmmo)//if mag is empty hold slide. TODO add slide catch functionality to manually hold slide.
		{
			PointInteractable.CompletionValue = 1f;
		}
		else
		{
			PointInteractable.CompletionValue = 0;
		}
	}

	public Hand Hand => InputGrabPoint?.HeldHand;
	public Hand slideHand => SlideGrabPoint?.HeldHand;

	public bool slideUp = false;
	protected override void OnUpdate()
	{
		if ( !Hand.IsValid() ) return;

		if(PointInteractable.CompletionValue.AlmostEqual(1f) && !slideBack)
		{
			if(slideCaught) slideCaught = false;
			slideBack = true;
		}


		if ( Hand.GetController().ButtonB.IsPressed && SlideCatch)
		{
	
			slideCaught = true;
		}

		if (!slideCaught && IsPulled && !slideHand.IsValid() && (Magazine is null || (Magazine is not null && Magazine.HasAmmo)) )
		{
			PointInteractable.CompletionValue = 0;
			slideCaught = false;
			slideBack = false;
		}
		if (!slideHand.IsValid() && Hand.GetController().ButtonA.IsPressed && slideBack )
		{
			PointInteractable.CompletionValue = 0;
			slideCaught = false;
			slideBack = false;
		}


	}
}
