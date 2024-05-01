using System.Collections.Immutable;

public partial class Interactable : Component
{
	/// <summary>
	/// Is this interactable held by something?
	/// </summary>
	public bool IsHeld => heldGrabPoints.Count( x => x.IsBeingHeld ) > 0;

	public GrabPoint PrimaryGrabPoint => heldGrabPoints.FirstOrDefault();
	public GrabPoint SecondaryGrabPoint => heldGrabPoints.FirstOrDefault( x => x.IsSecondaryGrabPoint );

	/// <summary>
	/// Is this interactable currently attached to an attachment point?
	/// </summary>
	[Property] public AttachmentPoint AttachmentPoint { get; set; }

	[Property] public Rigidbody Rigidbody { get; set; }

	HashSet<GrabPoint> heldGrabPoints = new();

	/// <summary>
	/// Gets you a hash set of the held grab points
	/// </summary>
	public ImmutableHashSet<GrabPoint> HeldGrabPoints => ImmutableHashSet.CreateRange( heldGrabPoints );

	TimeSince TimeSinceInteract = 1;
	const float InteractDelay = 0.4f;

	/// <summary>
	/// Can we start interacting with this object?
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	/// <returns></returns>
	protected virtual bool CanInteract( GrabPoint grabPoint, Hand hand )
	{
		if ( TimeSinceInteract < InteractDelay ) return false;

		if ( grabPoint.Transform.Position.Distance( hand.Transform.Position ) > 8f ) return false;

		// already being held by someone's hands
		if ( grabPoint.IsBeingHeld ) return false;

		// ??
		if ( hand.IsHolding() ) return false;

		return grabPoint.CanGrab( this, hand );
	}

	public void AttachableAdded( Attachable attachable, AttachmentPoint attachmentPoint )
	{
		OnAttachableAdded( attachable, attachmentPoint );
	}

	public void AttachableRemoved( Attachable attachable, AttachmentPoint attachmentPoint )
	{
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

		return grabPoint.CanStopGrab( this, hand );
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
			AttachmentPoint.Detach();
			AttachmentPoint = null;
		}

		OnInteract( grabPoint, hand );

		if ( GameObject.Parent.IsValid() )
		{
			GameObject.SetParent( null, true );
		}

		hand.AttachModelTo( grabPoint.GameObject );

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

		hand?.ResetAttachment();

		OnStopInteract( grabPoint, hand );

		grabPoint.HeldHand = null;
		heldGrabPoints.Remove( grabPoint );
		
		if ( heldGrabPoints.Count <= 0 )
		{
			Rigidbody.MotionEnabled = true;
		}

		return true;
	}

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

		Vector3.SmoothDamp( Rigidbody.Transform.Position, holdPos + diff, ref velocity, GetTimeToTranslate(), Time.Delta );
		Rigidbody.Velocity = velocity;

		var secondaryGrabPoint = heldGrabPoints.FirstOrDefault( x => x.IsSecondaryGrabPoint );

		Rotation targetRotation = primaryGrabPoint.HeldHand.GetHoldRotation( primaryGrabPoint );

		// Are we holding from a secondary hold point as well?
		if ( heldGrabPoints.FirstOrDefault( x => x.IsSecondaryGrabPoint ) is { } secondaryGrabPopint )
		{
			var direction = (secondaryGrabPoint.HeldHand.Transform.Position - primaryGrabPoint.HeldHand.Transform.Position).Normal;
			targetRotation = Rotation.LookAt( direction, Vector3.Up );
		}

		//if ( secondaryGrabPoint.IsValid() )
		//{
		//	Gizmo.Draw.Color = Color.Red;
		//	Gizmo.Draw.Line( secondaryGrabPoint.Transform.Position, primaryGrabPoint.Transform.Position );
		//	Gizmo.Draw.LineSphere( secondaryGrabPoint.Transform.Position, 2 );
		//	Gizmo.Draw.LineSphere( primaryGrabPoint.Transform.Position, 2 );
		//}

		var angularVelocity = Rigidbody.AngularVelocity;
		Rotation.SmoothDamp( Rigidbody.Transform.Rotation, targetRotation, ref angularVelocity, GetTimeToRotate(), Time.Delta );
		Rigidbody.AngularVelocity = angularVelocity;
	}

	float GetTimeToTranslate()
	{
		return 0.025f;
	}

	float GetTimeToRotate()
	{
		return 0.01f;
	}

	protected override void OnFixedUpdate()
	{
		if ( IsHeld )
		{
			PositionInteractable();
			HeldUpdate();
		}
	}

	internal void ClearAll()
	{
		var copy = new HashSet<GrabPoint>( heldGrabPoints );
		foreach ( var point in copy )
		{
			StopInteract( point );
		}
	}
}
