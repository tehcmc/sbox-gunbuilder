
public partial class SlideReleaseSystem : Component
{
	[RequireComponent] PointInteractable PointInteractable { get; set; }

	[Property] public GrabPoint InputGrabPoint { get; set; }

	public Hand Hand => InputGrabPoint?.HeldHand;

	protected override void OnUpdate()
	{
		if ( !Hand.IsValid() ) return;

		// Reset slide with button
		if ( Hand.GetController().ButtonA.IsPressed && PointInteractable.CompletionValue.AlmostEqual( 1f ) )
		{
			PointInteractable.CompletionValue = 0;
		}
	}
}
