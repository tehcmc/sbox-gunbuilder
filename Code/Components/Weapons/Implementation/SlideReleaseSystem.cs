
public partial class SlideReleaseSystem : Component
{
	[RequireComponent] PointInteractable PointInteractable { get; set; }
	[Property] public Weapon Weapon { get; set; }
	[Property] public GrabPoint InputGrabPoint { get; set; }

	WeaponMagazine Magazine => Weapon?.Magazine;

	public bool IsPulled => PointInteractable.CompletionValue.AlmostEqual( 1f );

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
		PointInteractable.CompletionValue = 1f;
	}

	public Hand Hand => InputGrabPoint?.HeldHand;

	protected override void OnUpdate()
	{
		if ( !Hand.IsValid() ) return;

		if ( Hand.GetController().ButtonA.IsPressed && PointInteractable.CompletionValue.AlmostEqual( 1f ) )
		{
			PointInteractable.CompletionValue = 0;
		}
	}
}
