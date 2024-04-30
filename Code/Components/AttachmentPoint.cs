using Sandbox;

public sealed class AttachmentPoint : Component, Component.ExecuteInEditor
{
	[Property] public Model GuideModel { get; set; }
	[Property] public TagSet AcceptedTags { get; set; } = new TagSet();

	protected override void DrawGizmos()
	{
		if ( GuideModel is not null )
		{
			Gizmo.Draw.Color = Color.Orange.WithAlpha( 0.4f );
			Gizmo.Draw.Model( GuideModel );
		}
	}

	protected override void OnUpdate()
	{
	}
}
