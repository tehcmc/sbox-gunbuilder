
public partial class GrabPoint : Component
{
	[Property] public Collider Collider { get; set; }

	protected override void OnUpdate()
	{
		Gizmo.Draw.Color = Color.Green.WithAlpha( 0.2f );
		Gizmo.Draw.LineSphere( Transform.Position, 2 );
	}
}
