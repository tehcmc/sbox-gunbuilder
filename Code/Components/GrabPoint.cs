

public partial class GrabPoint : Component
{
	/// <summary>
	/// The collider in question
	/// </summary>
	[Property] public Collider Collider { get; set; }

	/// <summary>
	/// The linked interactable
	/// </summary>
	[Property] public Interactable Interactable { get; set; }

	/// <summary>
	/// Is this a secondary grab point? Has to be linked to a primary grab point for it to work.
	/// </summary>
	public bool IsSecondaryGrabPoint => PrimaryGrabPoint.IsValid();

	public Hand HeldHand { get; set; }

	/// <summary>
	/// The primary grab point
	/// </summary>
	[Property] public GrabPoint PrimaryGrabPoint { get; set; }

	/// <summary>
	/// Is this grab point being held by a player?
	/// </summary>
	public bool IsBeingHeld => HeldHand.IsValid();

	public bool CanStopGrab( Interactable interactable, Hand hand )
	{
		return true;
	}

	public bool CanGrab( Interactable interactable, Hand hand )
	{
		// if ( IsSecondaryGrabPoint ) return PrimaryGrabPoint.IsBeingHeld;

		return true;
	}

	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Cyan.WithAlpha( 0.2f );
		Gizmo.Draw.Model( "models/hands/alyx_hand_right.vmdl" );
	}

	[Property] public Hand.PresetPose HoldingPose { get; set; } = Hand.PresetPose.Grip;
	internal void UpdateHandPose( Hand hand )
	{
		hand.SetPresetPose( HoldingPose );
	}
}
