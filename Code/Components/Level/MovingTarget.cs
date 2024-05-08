public sealed class MovingTarget : Component
{
	[Property] public GameObject Object { get; set; }
	[Property] public float Speed { get; set; }

	float dist = 0;
	[Property, Title( "Distance (meters)" )] public float Distance
	{
		get => dist;
		set
		{
			if ( dist == value ) return;
			dist = value;
		}
	}

	float MetersToInches( float meters )
	{
		return (meters * 39.3701f);
	}

	float SmoothDistance = 0;
	protected override void OnUpdate()
	{
		SmoothDistance = SmoothDistance.Approach( Distance, Time.Delta * Speed );
		var inches = MetersToInches( SmoothDistance );
		Object.Transform.LocalPosition = Transform.Rotation.Forward * inches;
	}
}
