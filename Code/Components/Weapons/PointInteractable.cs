
/// <summary>
/// A weapon part. This can be held by a player, and translated on a locked axis specified by the component.
/// </summary>
public partial class PointInteractable : BaseInteractable
{
	[Property] public GameObject TargetGameObject { get; set; }

	[Property] public GameObject StartPoint { get; set; }
	[Property] public GameObject EndPoint { get; set; }

	protected override void DrawGizmos()
	{
		if ( StartPoint.IsValid() && EndPoint.IsValid() )
		{
			Gizmo.Transform = new();
			Gizmo.Draw.Color = Color.Red;
			Gizmo.Draw.Line( StartPoint.Transform.Position, EndPoint.Transform.Position );
			Gizmo.Draw.LineSphere( StartPoint.Transform.Position, 0.2f );
			Gizmo.Draw.LineSphere( EndPoint.Transform.Position, 0.2f );
		}

		if ( TargetGameObject.IsValid() )
		{
			Gizmo.Transform = new();
			Gizmo.Draw.Color = Color.Blue;
			Gizmo.Draw.LineSphere( TargetGameObject.Transform.Position, 0.5f );
		}
	}
}
