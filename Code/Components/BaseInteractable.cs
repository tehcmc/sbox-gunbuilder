
using System.Collections.Immutable;

/// <summary>
/// The base class for an interactable. This has common functionality for interacting/holding continuous interaction with something.
/// </summary>
public partial class BaseInteractable : Component
{
	/// <summary>
	/// How long has it been since we started interacting / stopped interacting with this interactable?
	/// </summary>
	protected TimeSince TimeSinceInteract { get; set; } = 1;

	/// <summary>
	/// Which grab points (that belong to this interactable) are currently being held by some grubby player hands?
	/// </summary>

	protected HashSet<GrabPoint> heldGrabPoints = new();

	protected IEnumerable<GrabPoint> AllGrabPoints => Components.GetAll<GrabPoint>( FindMode.EnabledInSelfAndDescendants );

	/// <summary>
	/// Gets you a hash set of the held grab points
	/// </summary>
	public ImmutableHashSet<GrabPoint> HeldGrabPoints => ImmutableHashSet.CreateRange( heldGrabPoints );

	/// <summary>
	/// Is this interactable held by something?
	/// </summary>
	public bool IsHeld => heldGrabPoints.Count( x => x.IsBeingHeld ) > 0;

	/// <summary>
	/// A shorthand property to get the primary grab point for this interactable.
	/// </summary>
	public GrabPoint PrimaryGrabPoint => AllGrabPoints.FirstOrDefault( x => x.IsPrimaryGrabPoint );

	/// <summary>
	/// An artificial delay between how long we can start a new/stop a current interaction
	/// </summary>
	protected const float InteractDelay = 0.4f;

	protected virtual bool CanInteract( GrabPoint grabPoint, Hand hand )
	{
		// Artificial delay.
		if ( TimeSinceInteract < InteractDelay ) return false;

		// already being held by someone's hands
		if ( grabPoint.IsBeingHeld ) return false;

		// Is this really necessary?
		if ( hand.IsHolding() ) return false;

		// Final call, grab point, what do you think?
		return grabPoint.CanStartGrabbing( this, hand );
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
	/// Called when a player's hand interacts with a grab point that's on this gameobject.
	/// </summary>
	/// <param name="grabPoint"></param>
	/// <param name="hand"></param>
	public bool Interact( GrabPoint grabPoint, Hand hand )
	{
		if ( !CanInteract( grabPoint, hand ) ) return false;

		hand?.AttachModelToGrabPoint( grabPoint.GameObject );
		grabPoint.HeldHand = hand;
		heldGrabPoints.Add( grabPoint );

		grabPoint.OnStartGrabbing();
		OnInteract( grabPoint, hand );

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

		Log.Info( $"> Stop interacting with {grabPoint}" );

		hand?.DetachModelFromGrabPoint();

		heldGrabPoints.Remove( grabPoint );

		OnStopInteract( grabPoint, hand );
		grabPoint.OnStopGrabbing();
		grabPoint.HeldHand = null;

		return true;
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

	protected override void OnFixedUpdate()
	{
		if ( IsHeld )
		{
			HeldUpdate();
		}
	}

	/// <summary>
	/// Called every update while we're holding this interactable.
	/// </summary>
	protected virtual void OnHeldUpdate()
	{
		//
	}

	/// <summary>
	/// Called every update while holding this interactable.
	/// </summary>
	protected void HeldUpdate()
	{
		OnHeldUpdate();
	}

}
