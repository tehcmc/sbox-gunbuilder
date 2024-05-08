using Sandbox;

public sealed class FlipUpTarget : Component
{
	[Property] public GameObject Target { get; set; }
	[Property] public bool Triggered { get; set; } = false;
	[Property] public float Speed { get; set; } = 10;

	float SmoothPitch = 180;
	protected override void OnUpdate()
	{
		SmoothPitch = SmoothPitch.Approach( Triggered ? 90 : 180, Time.Delta * Speed );
		Target.Transform.LocalRotation = Rotation.From( SmoothPitch, 0, 0 );
	}
}
