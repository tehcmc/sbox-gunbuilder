using System.Collections.Immutable;

/// <summary>
/// An interactable object. This can be anything really, a box, a ball, a grenade, a gun.
/// Used in combination with grab points, which dictates positions on the interactable where you can pick it up.
/// </summary>
public partial class Interactable : BaseInteractable
{
	/// <summary>
	/// A shorthand property to get the secondary grab point for this interactable.
	/// We have to be holding a primary grab point to use this grab point.
	/// </summary>
	public GrabPoint SecondaryGrabPoint => heldGrabPoints.FirstOrDefault( x => x.IsSecondaryGrabPoint );

	/// <summary>
	/// The interactable's Rigidbody
	/// </summary>
	[Property] public Rigidbody Rigidbody { get; set; }

	/// <summary>
	/// Can we start interacting with this object?
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	/// <returns></returns>
	protected override bool CanInteract( GrabPoint grabPoint, Hand hand )
	{
		// Are we close enough to this grab point to grab it?
		// We could end up having stuff where we force grab items, but this system shouldn't be responsible for doing that.
		if ( grabPoint.Transform.Position.Distance( hand.Transform.Position ) > 8f ) return false;

		return base.CanInteract( grabPoint, hand );
	}

	public void Attach( Attachable attachable, AttachmentPoint attachmentPoint )
	{
		attachable.OnAttach( attachmentPoint );

		OnAttachableAdded( attachable, attachmentPoint );
	}

	/// <summary>
	/// Detaches an attachable from a specific attachemnt point on this interactable.
	/// </summary>
	/// <param name="attachable"></param>
	/// <param name="attachmentPoint"></param>
	public void Detach( Attachable attachable, AttachmentPoint attachmentPoint )
	{
		attachable.OnDetach( attachmentPoint );

		OnAttachableRemoved( attachable, attachmentPoint );
	}

	protected virtual void OnAttachableRemoved( Attachable attachable, AttachmentPoint attachmentPoint )
	{
		//
	}

	protected virtual void OnAttachableAdded( Attachable attachable, AttachmentPoint attachmentPoint )
	{
		//
	}

	/// <summary>
	/// Can we stop interacting with this object? Normally called when releasing the grip.
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	/// <returns></returns>
	protected override bool CanStopInteract( GrabPoint grabPoint, Hand hand )
	{
		return base.CanStopInteract( grabPoint, hand );
	}

	/// <summary>
	/// Called when we stop interacting with this object
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	/// <returns></returns>
	protected override void OnStopInteract( GrabPoint grabPoint, Hand hand )
	{
		base.OnStopInteract( grabPoint, hand );

		// If we're not holding this interactable anymore, turn its motion back on.
		if ( heldGrabPoints.Count <= 0 )
		{
			Rigidbody.MotionEnabled = true;
		}
	}

	/// <summary>
	/// Called when we start interactring with this opbject
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	protected override void OnInteract( GrabPoint grabPoint, Hand hand )
	{
		base.OnInteract( grabPoint, hand );

		Rigidbody.MotionEnabled = false;
	}

	protected override void OnHeldUpdate()
	{
		base.OnHeldUpdate();

		// Position the interactable how we want it.
		PositionInteractable();
	}

	/// <summary>
	/// Calculates the hold rotation for this interactable.
	/// Can be overriden in child classes.
	/// </summary>
	/// <returns></returns>
	protected virtual Rotation GetHoldRotation()
	{
		var secondaryGrabPoint = heldGrabPoints.FirstOrDefault( x => x.IsSecondaryGrabPoint );
		var targetRotation = PrimaryGrabPoint.HeldHand.GetHoldRotation( PrimaryGrabPoint );

		// Are we holding from a secondary hold point as well?
		if ( SecondaryGrabPoint.IsValid() )
		{
			var direction = (SecondaryGrabPoint.HeldHand.Transform.Position - PrimaryGrabPoint.HeldHand.Transform.Position).Normal;
			// TODO: Take into account the real rotation of the secondary grab point, so you can tilt the interactable from there.
			targetRotation = Rotation.LookAt( direction, Vector3.Up );
		}

		return targetRotation;
	}

	protected void PositionInteractable()
	{
		var velocity = Rigidbody.Velocity;
		var holdPos = PrimaryGrabPoint.HeldHand.GetHoldPosition( PrimaryGrabPoint );
		var grabPointPos = PrimaryGrabPoint.Transform.Position;

		Vector3.SmoothDamp( Rigidbody.Transform.Position, holdPos + ( holdPos - grabPointPos ), ref velocity, CalcVelocityWeight(), Time.Delta );
		Rigidbody.Velocity = velocity;

		var angularVelocity = Rigidbody.AngularVelocity;
		Rotation.SmoothDamp( Rigidbody.Transform.Rotation, GetHoldRotation(), ref angularVelocity, CalcAngularVelocityWeight(), Time.Delta );
		Rigidbody.AngularVelocity = angularVelocity;
	}

	/// <summary>
	/// Calculate the velocity weight.
	/// </summary>
	/// <returns></returns>
	float CalcVelocityWeight()
	{
		// A higher weight means it'll take longer for this weapon to traverse to your hand.
		// This is in seconds.
		return 0.20f;
	}

	/// <summary>
	/// Calculates the angular velocity weight.
	/// </summary>
	/// <returns></returns>
	float CalcAngularVelocityWeight()
	{
		return 0.15f;
	}

	protected override void OnFixedUpdate()
	{
		if ( IsHeld )
		{
			PositionInteractable();
			HeldUpdate();
		}
	}

	/// <summary>
	/// Clears all interactions. This'll make the player release this weapon from their hands.
	/// </summary>
	internal void ClearAllInteractions()
	{
		var copy = new HashSet<GrabPoint>( heldGrabPoints );
		foreach ( var point in copy )
		{
			StopInteract( point );
		}
	}
}
