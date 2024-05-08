public partial class TriggerInteractable : Component, Component.ITriggerListener
{
	Hand Hand { get; set; }

	public virtual void StartInteract( Hand hand )
	{
		Hand = hand;
	}

	public virtual void StopInteract( Hand hand )
	{
		if ( Hand == hand )
		{
			Hand = null;
		}
	}

	public virtual void UpdateInteract( Hand hand )
	{
	}

	protected override void OnUpdate()
	{
		if ( Hand.IsValid() )
		{
			UpdateInteract( Hand );
		}
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( other.GameObject.Root.Components.Get<Hand>( FindMode.EnabledInSelfAndDescendants ) is { } hand )
		{
			StartInteract( hand );
		}
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{
		if ( other.GameObject.Root.Components.Get<Hand>( FindMode.EnabledInSelfAndDescendants ) is { } hand )
		{
			StopInteract( hand );
		}
	}
}
