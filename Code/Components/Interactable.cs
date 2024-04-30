using System.Collections.Immutable;
using System.Numerics;

public partial class Interactable : Component
{
	/// <summary>
	/// Is this interactable held by something?
	/// </summary>
	public bool IsHeld => heldGrabPoints.Count( x => x.IsBeingHeld ) > 0;

	[Property] public Rigidbody Rigidbody { get; set; }

	HashSet<GrabPoint> heldGrabPoints = new();

	/// <summary>
	/// Gets you a hash set of the held grab points
	/// </summary>
	public ImmutableHashSet<GrabPoint> HeldGrabPoints => ImmutableHashSet.CreateRange( heldGrabPoints );

	/// <summary>
	/// Can we start interacting with this object?
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	/// <returns></returns>
	protected virtual bool CanInteract( GrabPoint grabPoint, Hand hand )
	{
		if ( hand.IsHolding() ) return false;

		return grabPoint.CanGrab( this, hand );
	}

	/// <summary>
	/// Can we stop interacting with this object? Normally called when releasing the grip.
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	/// <returns></returns>
	protected virtual bool CanStopInteract( GrabPoint grabPoint, Hand hand )
	{
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
		//
	}

	/// <summary>
	/// Called when we start interactring with this opbject
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	protected virtual void OnInteract( GrabPoint grabPoint, Hand hand )
	{
		//
	}

	protected virtual void OnHeldUpdate()
	{

	}

	/// <summary>
	/// Called when a player's hand interacts with a grab point that's on this gameobject.
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	public bool Interact( GrabPoint grabPoint, Hand hand )
	{
		if ( !CanInteract( grabPoint, hand ) ) return false;

		Log.Info( $"started grabbing {this.GameObject} at {grabPoint.GameObject} with {hand.GameObject}" );

		OnInteract( grabPoint, hand );

		grabPoint.HeldHand = hand;
		Rigidbody.MotionEnabled = false;
		heldGrabPoints.Add( grabPoint );

		return true;
	}


	/// <summary>
	/// Called when a player's hand interacts with a grab point that's on this gameobject.
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	public bool StopInteract( GrabPoint grabPoint, Hand hand )
	{
		if ( !CanStopInteract( grabPoint, hand ) ) return false;

		Log.Info( $"stopped grabbing {this.GameObject} at {grabPoint.GameObject} with {hand.GameObject}" );

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

	protected override void OnUpdate()
	{
		if ( IsHeld )
		{
		}
	}

	protected void PositionInteractable()
	{
		var primaryGrabPoint = heldGrabPoints.First();

		var velocity = Rigidbody.Velocity;
		Vector3.SmoothDamp( Rigidbody.Transform.Position, primaryGrabPoint.HeldHand.Transform.Position, ref velocity, 0.075f, Time.Delta );
		Rigidbody.Velocity = velocity;

		var secondaryGrabPoint = heldGrabPoints.FirstOrDefault( x => x.IsSecondaryGrabPoint );

		Rotation targetRotation = primaryGrabPoint.HeldHand.Transform.Rotation;

		// Are we holding from a secondary hold point as well?
		if ( heldGrabPoints.FirstOrDefault( x => x.IsSecondaryGrabPoint ) is { } secondaryGrabPopint )
		{
			var direction = (secondaryGrabPoint.HeldHand.Transform.Position - primaryGrabPoint.HeldHand.Transform.Position).Normal;
			targetRotation = Rotation.LookAt( direction, Vector3.Up );
		}

		if ( secondaryGrabPoint.IsValid() )
		{
			Gizmo.Draw.Color = Color.Red;
			Gizmo.Draw.Line( secondaryGrabPoint.Transform.Position, primaryGrabPoint.Transform.Position );
			Gizmo.Draw.LineSphere( secondaryGrabPoint.Transform.Position, 2 );
			Gizmo.Draw.LineSphere( primaryGrabPoint.Transform.Position, 2 );
		}

		var angularVelocity = Rigidbody.AngularVelocity;
		Rotation.SmoothDamp( Rigidbody.Transform.Rotation, targetRotation, ref angularVelocity, 0.075f, Time.Delta );
		Rigidbody.AngularVelocity = angularVelocity;
	}

	protected override void OnFixedUpdate()
	{
		if ( IsHeld )
		{
			PositionInteractable();
			HeldUpdate();
		}
	}
}
