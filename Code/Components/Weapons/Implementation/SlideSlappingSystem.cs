public partial class SlideSlappingSystem : Component, Component.ITriggerListener
{
	[Property] public PointInteractable PointInteractable { get; set; }
	[Property] public Collider Slapper { get; set; }
	[Property] public GameObject Hand { get; set; }

	public bool IsPulled => PointInteractable.CompletionValue.AlmostEqual( 1f );

	protected override void OnUpdate()
	{
		if ( !Hand.IsValid() ) return;

		if ( IsPulled && IsCorrectDirectionAndSpeed() )
		{
			PointInteractable.CompletionValue = 0f;
			Hand = null;
			return;
		}

		if ( Hand.Transform.Position.Distance( Transform.Position ) > 64f )
		{
			Hand = null;
		}
	}

	protected bool IsCorrectDirectionAndSpeed()
	{
		var velocity = Hand.Components.Get<Hand>( FindMode.EnabledInSelfAndDescendants )?.Velocity ?? 0f;
		Vector3 relativeVelocity = Transform.Rotation.Inverse * velocity;
		var relativeDir = relativeVelocity.Normal;
		var howForward = relativeDir.Dot( Transform.Rotation.Forward );

		if ( velocity.Length < 1.2f ) return false;
		if ( howForward > 0.25f && howForward < 0.75f ) return true;
		
		return false;
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( other.Tags.Has( "hands" ) )
		{
			Hand = other.GameObject.Root;
		}
	}
}
