
public partial class SlideReleaseSystem : Component
{
	[RequireComponent] PointInteractable PointInteractable { get; set; }
	[Property] public Weapon Weapon { get; set; }
	[Property] public GrabPoint InputGrabPoint { get; set; }
	[Property] public GrabPoint SlideGrabPoint { get; set; }

	[Property] public bool SlideCatch { get; protected set; }
	WeaponMagazine Magazine => Weapon?.Magazine;
	WeaponChamber Chamber => Weapon?.Chamber;
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

		if ( after == 1 )
		{
			Weapon.TryEjectFromChamber();
		}
	}

	public void OnBulletEjected( IEnumerable<Bullet> ejected )
	{
		//
	}

	public void TriggerEmpty()
	{
		if(LockSlide()) //if mag is empty hold slide. TODO add slide catch functionality to manually hold slide.
		{
			PointInteractable.CompletionValue = 0.9999f;
		}
		else
		{
			PointInteractable.CompletionValue = 0;
		}
	}

	public bool LockSlide()
	{
		if ( (Magazine is not null && !Magazine.HasAmmo) && (Chamber is not null && Chamber.Chamber.Count == 0) ) return true;

		return false;
	}

	public void ResetSlide()
	{
		PointInteractable.CompletionValue = 0;
		slideCaught = false;
		slideBack = false;
		if ( Chamber is not null && Chamber.ChamberCount != 0 && Magazine is not null )
		{
			Chamber.Feed(Magazine);
		}
	}

	public Hand Hand => InputGrabPoint?.HeldHand;
	public Hand SlideHand => SlideGrabPoint?.HeldHand;


	protected override void OnUpdate()
	{
		if ( !Hand.IsValid() ) return;

		

		if(PointInteractable.CompletionValue.AlmostEqual(1f) || PointInteractable.CompletionValue ==1f && !slideBack)
		{
			if(slideCaught) slideCaught = false;
			slideBack = true;

			if(Chamber is not null && Chamber.ChamberCount > 0)
			{
				Chamber.Eject();
			}
		}

		if (IsPulled && !SlideHand.IsValid() && !LockSlide() )
		{
			ResetSlide();
		}

		if (LockSlide() && Hand.GetController().ButtonA.IsPressed && slideBack )
		{
			ResetSlide();
		}


	}
}
