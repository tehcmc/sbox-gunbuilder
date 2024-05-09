public partial class GrabPoint
{
	GameObject IGrabbable.GameObject => GameObject;
	ITagSet IGrabbable.Tags => Tags;
	Hand IGrabbable.Hand => HeldHand;
	bool IGrabbable.IsHeld => HeldHand.IsValid();

	void IGrabbable.StartGrabbing( Hand hand )
	{
		HeldHand = hand;
		OnStartGrabbing();
	}

	void IGrabbable.StopGrabbing( Hand hand )
	{
		HeldHand = null;
		OnStopGrabbing();
	}

	bool IGrabbable.CanStartGrabbing( BaseInteractable interactable, Hand hand )
	{
		return true;
	}

	bool IGrabbable.CanStopGrabbing( BaseInteractable interactable, Hand hand )
	{
		return true;
	}
}
