
public partial class SlideReleaseSystem : Component
{
	[RequireComponent] PointInteractable PointInteractable { get; set; }
	[Property] public GrabPoint InputGrabPoint { get; set; }

	public bool IsPulled => PointInteractable.CompletionValue.AlmostEqual( 1f );

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
