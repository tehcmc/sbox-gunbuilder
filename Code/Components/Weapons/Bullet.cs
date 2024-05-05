
public enum BulletCaliber
{
	None,
	[Title( "5.56x45mm" )]
	FiveFiveSix,
	[Title( "9mm" )]
	Nine,
	[Title( "12ga" )]
	TwelveGauge
}

public partial record Bullet

{
	[Property] public BulletCaliber Caliber { get; set; } = BulletCaliber.FiveFiveSix;
}

public partial class BulletComponent : Component, IAmmoSource
{
	[Property] public Bullet Bullet { get; set; } = new();

	public Stack<Bullet> Bullets
	{
		get
		{
			var stack = new Stack<Bullet>();
			stack.Push( Bullet );
			return stack;
		}
	}

	public IEnumerable<Bullet> Pop( int amount = 1 )
	{
		yield return Bullet;
	}
}
