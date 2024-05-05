using System.Text.Json.Serialization;

public partial class WeaponChamber : Component, Component.ITriggerListener
{
	/// <summary>
	/// A list of bullets that are in this chamber.
	/// </summary>
	[Property] public Stack<Bullet> Chamber { get; set; } = new();

	/// <summary>
	/// Debugging
	/// </summary>
	[Property, JsonIgnore] public int ChamberCount => Chamber.Count;

	/// <summary>
	/// How many bullets can we put in the chamber? For something like a shotgun
	/// </summary>
	[Property] public int ChamberCapacity { get; set; } = 1;

	/// <summary>
	/// Can we manually feed bullets into the chamber?
	/// </summary>
	[Property] public bool CanManuallyFeed { get; set; } = true;

	/// <summary>
	/// Should we auto-chamber from the weapon's ammo source?
	/// </summary>
	[Property] public bool AutoChamber { get; set; }

	bool CanInsert
	{
		get => Chamber.Count < ChamberCapacity;
	}

	/// <summary>
	/// Try to eject a bullet from the chamber.
	/// </summary>
	/// <returns></returns>
	public IEnumerable<Bullet> Eject()
	{
		if ( Chamber.TryPop( out var bullet ) )
		{
			Log.Info( $"Ejected {bullet} from {this}" );

			yield return bullet;
		}
	}

	/// <summary>
	/// Try to feed bullets from an ammo source
	/// </summary>
	/// <param name="src"></param>
	/// <returns>How many bullets we fed from this list</returns>
	public int Feed( IAmmoSource src )
	{
		if ( src is null ) return 0;

		int count = 0;

		while ( CanInsert )
		{
			if ( src.Pop() is { } bullets )
			{
				var bullet = bullets.First();
				Log.Info( $"Pushed {bullet} into {this}" );
				Chamber.Push( bullet );
				count++;
			}
			else
			{
				break;
			}
		}

		return count;
	}

	[Property] public PointInteractable Bolt { get; set; }

	void ITriggerListener.OnTriggerEnter( Sandbox.Collider other )
	{
		if ( other.GameObject.Root.Components.Get<BulletComponent>() is { } bulletComponent )
		{
			if ( !Bolt.IsValid() || ( Bolt.IsValid() && Bolt.CompletionValue.Equals( 1f ) ) )
			{
				if ( Feed( bulletComponent ) > 0 )
				{
					var interactable = bulletComponent.Components.Get<Interactable>();
					if ( interactable is not null )
					{
						interactable.ClearAllInteractions();
					}

					other.GameObject.Destroy();
				}
			}
		}
	}
}
