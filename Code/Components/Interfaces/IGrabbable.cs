public interface IGrabbable : IValid
{
	/// <summary>
	/// This grabbable has stopped being grabbed by a hand.
	/// </summary>
	/// <param name="hand"></param>
	public void StopGrabbing( Hand hand );

	/// <summary>
	/// This grabbable thas started being grabbed by a hand.
	/// </summary>
	public void StartGrabbing( Hand hand );

	/// <summary>
	/// Can we start grabbing?
	/// </summary>
	/// <param name="interactable"></param>
	/// <param name="hand"></param>
	/// <returns></returns>
	public bool CanStartGrabbing( BaseInteractable interactable, Hand hand )
	{
		return true;
	}

	/// <summary>
	/// Can we stop grabbing this?
	/// </summary>
	/// <param name="interactable"></param>
	/// <param name=""></param>
	/// <param name="hand"></param>
	/// <returns></returns>
	public bool CanStopGrabbing( BaseInteractable interactable, Hand hand )
	{
		return true;
	}

	/// <summary>
	/// The grabbable's GameObject.
	/// </summary>
	GameObject GameObject { get; }

	/// <summary>
	/// The tags on this grabbable.
	/// </summary>
	ITagSet Tags { get; }

	/// <summary>
	/// Hand (if we have one)
	/// </summary>
	Hand Hand { get; }

	/// <summary>
	/// Is this grabbable held by something already?
	/// </summary>
	bool IsHeld { get; }

	/// <summary>
	/// The interactable
	/// </summary>
	BaseInteractable Interactable { get; }

	/// <summary>
	/// The input type.
	/// </summary>
	public GrabInputType GrabInput { get; set; }
}

/// <summary>
/// What's our grabbing input type?
/// </summary>
public enum GrabInputType
{
	Grip,
	Trigger,
	Hover
}
