public sealed class BouncingTarget : Component
{
	[Property] public GameObject Object { get; set; }
	[Property] public float Speed { get; set; }

	bool IsGoingLeft = false;

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

	void Flip()
	{
		Distance = -Distance;
	}

	float SmoothDistance = 0;
	protected override void OnUpdate()
	{
		SmoothDistance = SmoothDistance.Approach( Distance, Time.Delta * Speed );
		var inches = MetersToInches( SmoothDistance );
		Object.Transform.LocalPosition = Transform.Rotation.Right * inches;

		if ( SmoothDistance.AlmostEqual( Distance ) )
		{
			Flip();
		}
	}
}
