public sealed class TrashRemoverComponent : Component, Component.ICollisionListener
{
	[Property] public SoundEvent TrashSound { get; set; }

	void PlaySound()
	{
		if ( TrashSound is not null )
		{
			Sound.Play( TrashSound, Transform.Position );
		}
	}

	void ICollisionListener.OnCollisionStart( Sandbox.Collision other )
	{
		if ( other.Other.GameObject.Root.Components.Get<Interactable>() is { } interactable )
		{
			// Clear all interactions
			interactable.ClearAllInteractions();
			
			// Delete the object
			interactable.GameObject.Destroy();

			PlaySound();
		}
	}
}
