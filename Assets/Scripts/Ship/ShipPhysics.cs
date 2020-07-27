using UnityEngine;

public class ShipPhysics
{
    private Rigidbody rigidbody;

    private float smoothThrottle = 0f;
    private bool isDestroyed = false;

    private const float Multiplier = 100f;

    public Vector3 Velocity => rigidbody.velocity;

    public ShipPhysics(Rigidbody rigidbody, ref ShipSpecs specs)
    {
        this.rigidbody = rigidbody;

        if (!specs.IsCapital)
            this.rigidbody.inertiaTensor = Vector3.one * 1000f;

        this.rigidbody.mass = specs.Physics.Mass;
        this.rigidbody.drag = specs.Physics.Drag;
        this.rigidbody.angularDrag = specs.Physics.AngularDrag;
    }

    public void Update(Pilot pilot, ref ShipSpecs specs)
    {
        if (isDestroyed)
            return;

        rigidbody.AddRelativeTorque(
            pilot.Pitch * specs.Engine.Torque * Multiplier,
            pilot.Yaw * specs.Engine.Torque * Multiplier,
            -pilot.Roll * specs.Engine.Torque * Multiplier);

        // Ramp up/down speed for smoother acceleration.
        smoothThrottle = SmoothDamp.Move(
            smoothThrottle,
            pilot.Throttle,
            specs.Engine.Accel,
            Time.fixedDeltaTime);

        rigidbody.AddRelativeForce(
            Vector3.forward * specs.Engine.Thrust * Multiplier * smoothThrottle,
            ForceMode.Force);
    }

    /// <summary>
    /// Puts the ship in an uncontrollable spin.
    /// </summary>
    public void NotifyShipDestroyed()
    {
        isDestroyed = true;
        rigidbody.drag = 0f;
        rigidbody.angularDrag = 0f;
        rigidbody.angularVelocity = Random.insideUnitSphere * 5f;
    }
}
