using System.Collections.Immutable;

/// <summary>
/// An interactable object. This can be anything really, a box, a ball, a grenade, a gun.
/// Used in combination with grab points, which dictates positions on the interactable where you can pick it up.
/// </summary>
public partial class Interactable : Component
{
	/// <summary>
	/// Is this interactable held by something?
	/// </summary>
	public bool IsHeld => heldGrabPoints.Count( x => x.IsBeingHeld ) > 0;

	/// <summary>
	/// A shorthand property to get the primary grab point for this interactable.
	/// </summary>
	public GrabPoint PrimaryGrabPoint => heldGrabPoints.FirstOrDefault( x => x.IsPrimaryGrabPoint );

	/// <summary>
	/// A shorthand property to get the secondary grab point for this interactable.
	/// We have to be holding a primary grab point to use this grab point.
	/// </summary>
	public GrabPoint SecondaryGrabPoint => heldGrabPoints.FirstOrDefault( x => x.IsSecondaryGrabPoint );

	/// <summary>
	/// Is this interactable currently attached to an attachment point?
	/// </summary>
	[Property] public AttachmentPoint AttachmentPoint { get; set; }

	/// <summary>
	/// The interactable's Rigidbody
	/// </summary>
	[Property] public Rigidbody Rigidbody { get; set; }

	/// <summary>
	/// Which grab points (that belong to this interactable) are currently being held by some grubby player hands?
	/// </summary>

	HashSet<GrabPoint> heldGrabPoints = new();

	/// <summary>
	/// Gets you a hash set of the held grab points
	/// </summary>
	public ImmutableHashSet<GrabPoint> HeldGrabPoints => ImmutableHashSet.CreateRange( heldGrabPoints );

	/// <summary>
	/// How long has it been since we started interacting / stopped interacting with this interactable?
	/// </summary>
	TimeSince TimeSinceInteract { get; set; } = 1;

	/// <summary>
	/// An artificial delay between how long we can start a new/stop a current interaction
	/// </summary>
	const float InteractDelay = 0.4f;

	/// <summary>
	/// Can we start interacting with this object?
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	/// <returns></returns>
	protected virtual bool CanInteract( GrabPoint grabPoint, Hand hand )
	{
		// Artificial delay.
		if ( TimeSinceInteract < InteractDelay ) return false;

		// Are we close enough to this grab point to grab it?
		// We could end up having stuff where we force grab items, but this system shouldn't be responsible for doing that.
		if ( grabPoint.Transform.Position.Distance( hand.Transform.Position ) > 8f ) return false;

		// already being held by someone's hands
		if ( grabPoint.IsBeingHeld ) return false;

		// Is this really necessary?
		if ( hand.IsHolding() ) return false;

		// Final call, grab point, what do you think?
		return grabPoint.CanStartGrabbing( this, hand );
	}

	public void Attach( Attachable attachable, AttachmentPoint attachmentPoint )
	{
		// Clear all interactions since we don't want the player to hold the item anymore.
		ClearAllInteractions();

		// Make sure we know which attachment point we're on now.
		AttachmentPoint = attachmentPoint;

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
	protected virtual bool CanStopInteract( GrabPoint grabPoint, Hand hand )
	{
		if ( TimeSinceInteract < InteractDelay ) return false;

		return grabPoint.CanStopGrabbing( this, hand );
	}

	/// <summary>
	/// Called when we stop interacting with this object
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	/// <returns></returns>
	protected virtual void OnStopInteract( GrabPoint grabPoint, Hand hand )
	{
		TimeSinceInteract = 0;
	}

	/// <summary>
	/// Called when we start interactring with this opbject
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	protected virtual void OnInteract( GrabPoint grabPoint, Hand hand )
	{
		TimeSinceInteract = 0;
	}

	/// <summary>
	/// Called every update while we're holding this interactable.
	/// </summary>
	protected virtual void OnHeldUpdate()
	{
		//
	}

	/// <summary>
	/// Called when a player's hand interacts with a grab point that's on this gameobject.
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	public bool Interact( GrabPoint grabPoint, Hand hand )
	{
		if ( !CanInteract( grabPoint, hand ) ) return false;

		if ( AttachmentPoint.IsValid() )
		{
			AttachmentPoint.TryDetach();
			AttachmentPoint = null;
		}

		OnInteract( grabPoint, hand );

		// Is this really necessary?
		if ( GameObject.Parent.IsValid() )
		{
			GameObject.SetParent( null, true );
		}

		hand?.AttachModelToGrabPoint( grabPoint.GameObject );

		grabPoint.HeldHand = hand;
		Rigidbody.MotionEnabled = false;
		heldGrabPoints.Add( grabPoint );

		return true;
	}

	/// <summary>
	/// Called when a player's hand interacts with a grab point that's on this gameobject.
	/// </summary>
	/// <param name="grabPoint"></param>
	public bool StopInteract( GrabPoint grabPoint )
	{
		var hand = grabPoint.HeldHand;

		if ( !CanStopInteract( grabPoint, hand ) ) return false;

		Log.Info( $"{this.GameObject} stopping interaction {grabPoint}" );

		hand?.DetachModelFromGrabPoint();

		OnStopInteract( grabPoint, hand );

		grabPoint.HeldHand = null;
		heldGrabPoints.Remove( grabPoint );
		
		// If we're not holding this interactable anymore, turn its motion back on.
		if ( heldGrabPoints.Count <= 0 )
		{
			Rigidbody.MotionEnabled = true;
		}

		return true;
	}

	/// <summary>
	/// Called every update while holding this interactable.
	/// </summary>
	protected void HeldUpdate()
	{
		OnHeldUpdate();
	}

	protected void PositionInteractable()
	{
		var primaryGrabPoint = heldGrabPoints.First();

		var velocity = Rigidbody.Velocity;

		var holdPos = primaryGrabPoint.HeldHand.GetHoldPosition( primaryGrabPoint );
		var grabPointPos = PrimaryGrabPoint.Transform.Position;
		var diff = (holdPos - grabPointPos);

		Vector3.SmoothDamp( Rigidbody.Transform.Position, holdPos + diff, ref velocity, CalcVelocityWeight(), Time.Delta );
		Rigidbody.Velocity = velocity;

		var secondaryGrabPoint = heldGrabPoints.FirstOrDefault( x => x.IsSecondaryGrabPoint );

		Rotation targetRotation = primaryGrabPoint.HeldHand.GetHoldRotation( primaryGrabPoint );

		// Are we holding from a secondary hold point as well?
		if ( heldGrabPoints.FirstOrDefault( x => x.IsSecondaryGrabPoint ) is { } secondaryGrabPopint )
		{
			var direction = (secondaryGrabPoint.HeldHand.Transform.Position - primaryGrabPoint.HeldHand.Transform.Position).Normal;
			targetRotation = Rotation.LookAt( direction, Vector3.Up );
		}

		var angularVelocity = Rigidbody.AngularVelocity;
		Rotation.SmoothDamp( Rigidbody.Transform.Rotation, targetRotation, ref angularVelocity, CalcAngularVelocityWeight(), Time.Delta );
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
