using System.Diagnostics;

public partial class LaserPointerComponent : Component, Component.ExecuteInEditor
{
	[Property] public Sandbox.ParticleSystem LineParticle { get; set; }
	[Property] public Sandbox.ParticleSystem DotParticle { get; set; }
	[Property] public Color LaserColor { get; set; }

	LegacyParticleSystem LineParticleSystem { get; set; }
	LegacyParticleSystem DotParticleSystem { get; set; }

	[Property, MakeDirty] public bool IsEnabled { get; set; } = true;

	[Property] public GrabPoint GrabPoint { get; set; }

	protected override void OnDirty()
	{
		SetEnabled( IsEnabled );
	}

	private List<ParticleControlPoint> LineCPs
	{
		get
		{
			return new()
			{
				new() { StringCP = "0", Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = Transform.Position },
				new() { StringCP = "1", Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = GetTraceEnd() },
				new() { StringCP = "2", Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = new( LaserColor.r, LaserColor.g, LaserColor.b )  },
			};
		}
	}

	private List<ParticleControlPoint> DotCPs
	{
		get
		{
			return new()
			{
				new() { StringCP = "0", Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = GetTraceEnd() },
				new() { StringCP = "2", Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = new( LaserColor.r, LaserColor.g, LaserColor.b )  },
			};
		}
	}

	const float Dist = 100000;

	private Vector3 GetTraceEnd()
	{
		var tr = Scene.Trace.Ray( Transform.Position, Transform.Position + ( Transform.Rotation.Forward * Dist ) ).Run();
		return tr.EndPosition;
	}

	public void SetEnabled( bool enabled )
	{
		if ( enabled )
		{
			LineParticleSystem?.Destroy();
			DotParticleSystem?.Destroy();

			if ( LineParticle is not null )
			{
				LineParticleSystem = Components.Create<LegacyParticleSystem>();
				LineParticleSystem.Flags = ComponentFlags.NotSaved | ComponentFlags.Hidden;
				LineParticleSystem.Particles = LineParticle;
				LineParticleSystem.ControlPoints = LineCPs;
			}

			if ( DotParticle is not null )
			{
				DotParticleSystem = Components.Create<LegacyParticleSystem>();
				DotParticleSystem.Flags = ComponentFlags.NotSaved | ComponentFlags.Hidden;
				DotParticleSystem.Particles = DotParticle;
				DotParticleSystem.ControlPoints = DotCPs;
			}
		}
		else
		{
			LineParticleSystem?.Destroy();
			DotParticleSystem?.Destroy();
		}
	}

	protected override void OnEnabled()
	{
		SetEnabled( IsEnabled );

		GrabPoint.OnGrabStartEvent += OnGrabStart;
		GrabPoint.OnGrabEndEvent += OnGrabEnd;
	}

	void OnGrabStart()
	{ 
		//
	}

	void OnGrabEnd()
	{
		//
	}

	protected override void OnDisabled()
	{
		SetEnabled( false );
	}

	TimeSince TimeSinceToggled { get; set; } = 1;

	protected override void OnUpdate()
	{
		if ( LineParticleSystem.IsValid() )
			LineParticleSystem.ControlPoints = LineCPs;

		if ( DotParticleSystem.IsValid() )
			DotParticleSystem.ControlPoints = DotCPs;

		if ( GrabPoint.IsValid() && GrabPoint.HeldHand.IsValid() )
		{
			var controller = GrabPoint.HeldHand.GetController();
			if ( controller.JoystickPress.IsPressed && TimeSinceToggled > 0.5f )
			{
				TimeSinceToggled = 0;

				IsEnabled ^= true;
				SetEnabled( IsEnabled );
			}
		}
	}
}
