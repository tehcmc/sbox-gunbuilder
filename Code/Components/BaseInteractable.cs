
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

	protected HashSet<IGrabbable> heldGrabbables = new();

	public virtual IEnumerable<IGrabbable> AllGrabPoints => Components.GetAll<GrabPoint>( FindMode.EnabledInSelfAndDescendants );

	/// <summary>
	/// Gets you a hash set of the held grab points
	/// </summary>
	public virtual ImmutableHashSet<IGrabbable> HeldGrabPoints => ImmutableHashSet.CreateRange( heldGrabbables );

	/// <summary>
	/// Is this interactable held by something?
	/// </summary>
	public virtual bool IsHeld => heldGrabbables.Count( x => x.IsHeld ) > 0;

	/// <summary>
	/// A shorthand property to get the primary grab point for this interactable.
	/// </summary>
	public virtual IGrabbable PrimaryGrabPoint => AllGrabPoints.FirstOrDefault( x => !x.Tags.Has( "secondary" ) );

	/// <summary>
	/// An artificial delay between how long we can start a new/stop a current interaction
	/// </summary>
	protected const float InteractDelay = 0.4f;

	public void SetGrabPointsEnabled( bool enabled )
	{
		foreach ( var grabPoint in AllGrabPoints )
		{
			grabPoint.GameObject.Enabled = enabled;
		}
	}

	protected virtual bool CanInteract( IGrabbable grabbable, Hand hand )
	{
		// Artificial delay.
		if ( TimeSinceInteract < InteractDelay ) return false;

		// already being held by someone's hands
		if ( grabbable.IsHeld ) return false;

		// Is this really necessary?
		if ( hand.IsHolding() ) return false;

		// Final call, grab point, what do you think?
		return grabbable.CanStartGrabbing( this, hand );
	}

	/// <summary>
	/// Can we stop interacting with this object? Normally called when releasing the grip.
	/// </summary>
	/// <param name="grabbable"></param>
	/// <param name="hand"></param>
	/// <returns></returns>
	protected virtual bool CanStopInteract( IGrabbable grabbable, Hand hand )
	{
		if ( TimeSinceInteract < InteractDelay ) return false;
		return grabbable.CanStopGrabbing( this, hand );
	}

	/// <summary>
	/// Called when a player's hand interacts with a grab point that's on this gameobject.
	/// </summary>
	/// <param name="grabbable"></param>
	/// <param name="hand"></param>
	public bool Interact( IGrabbable grabbable, Hand hand )
	{
		if ( !CanInteract( grabbable, hand ) ) return false;

		grabbable.StartGrabbing( hand );

		hand?.AttachModelToGrabPoint( grabbable.GameObject );

		heldGrabbables.Add( grabbable );

		OnInteract( grabbable, hand );

		return true;
	}

	/// <summary>
	/// Called when a player's hand interacts with a grab point that's on this gameobject.
	/// </summary>
	/// <param name="grabbable"></param>
	public bool StopInteract( IGrabbable grabbable )
	{
		var hand = grabbable.Hand;

		if ( !CanStopInteract( grabbable, hand ) ) return false;

		Log.Info( $"> Stop interacting with {grabbable}" );

		grabbable.StopGrabbing( hand );

		hand?.DetachModelFromGrabPoint();

		heldGrabbables.Remove( grabbable );

		OnStopInteract( grabbable, hand );

		return true;
	}


	/// <summary>
	/// Called when we stop interacting with this object
	/// </summary>
	/// <param name="grabbable"></param>
	/// <param name="hand"></param>
	/// <returns></returns>
	protected virtual void OnStopInteract( IGrabbable grabbable, Hand hand )
	{
		TimeSinceInteract = 0;
	}

	/// <summary>
	/// Called when we start interactring with this opbject
	/// </summary>
	/// <param name="grabbable"></param>
	/// <param name="hand"></param>
	protected virtual void OnInteract( IGrabbable grabbable, Hand hand )
	{
		TimeSinceInteract = 0;
	}
}
