using Sandbox;
using System.Text.Json.Serialization;

public interface IAmmoSource
{
	Stack<Bullet> Bullets { get; }
}

public sealed class WeaponMagazine : Component, IAmmoSource
{
	/// <summary>
	/// The interactable that belongs to this weapon magazine. It'll always exist.
	/// </summary>
	[RequireComponent] public Interactable Interactable { get; set; }

	/// <summary>
	/// The attachable that belongs to this weapon magazine. It'll also always exist.
	/// </summary>
	[RequireComponent] public Attachable Attachable { get; set; }

	/// <summary>
	/// The model renderer for this mag.
	/// </summary>
	[Property] public SkinnedModelRenderer Renderer { get; set; }

	/// <summary>
	/// A list of the weapon's bullets.
	/// </summary>
	[Property, MakeDirty] public Stack<Bullet> Bullets { get; set; }

	/// <summary>
	/// Debugging
	/// </summary>
	[Property, JsonIgnore] public int ChamberCount => Bullets.Count;

	/// <summary>
	/// What's the ammo capacity for this gun?
	/// </summary>
	[Property] public int BulletCapacity { get; set; } = 30;

	/// <summary>
	/// Mainly for development - but negates needing ammo in the magazine to fire a gun.
	/// </summary>
	[Property] public bool IsInfiniteAmmo { get; set; } = false;

	[Property] public int AmmoBodygroup { get; set; } = 0;

	/// <summary>
	/// Does this magazine have any ammo?
	/// </summary>
	public bool HasAmmo
	{
		get => Bullets.Count > 0;
	}

	protected override void OnStart()
	{
		// Push the bullet capacity into the gun
		for ( int i = 0; i < BulletCapacity; i++ )
		{
			Push( new Bullet() );
		}
	}

	/// <summary>
	/// Adds a bullet to the top of the pile.
	/// </summary>
	/// <param name="bullets"></param>
	public void Push( params Bullet[] bullets )
	{
		foreach ( var bullet in bullets )
		{
			Bullets.Push( bullet );
		}
	}

	public IEnumerable<Bullet> Pop( int amount = 1 )
	{
		var popped = new List<Bullet>();

		for ( int i = 0; i < amount; i++ )
		{
			popped.Add( Bullets.Pop() );
		}
		return popped;
	}

	protected override void OnDirty()
	{
		Renderer?.SetBodyGroup( AmmoBodygroup, Bullets.Count > 0 ? 0 : 1 );
	}
}
