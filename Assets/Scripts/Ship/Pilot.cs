using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Pilot
{
    public bool IsPlayer = false;

    [Range(0, 1)] public float Throttle = 0f;
    [Range(-1, 1)] public float Pitch = 0f;
    [Range(-1, 1)] public float Yaw = 0f;
    [Range(-1, 1)] public float Roll = 0f;

    [Header("Combat")]
    public Transform FollowTarget = null;
    public Target AttackTarget = null;
    public bool IsFiring = false;
    public float MaxTurnPower = 0.6f;
    [Range(0f, 1f)]
    public float FireChance = 0.5f;

    public Ship ownShip = null;
    public Target ownTarget = null;

    private StrafeState strafeState = StrafeState.Strafing;
    private Vector3 strafeTargetOffset = Vector3.zero;
    private Vector3 extendPosition = Vector3.zero;

    private float lastTargetCheck = 0f;
    private float thinkDelay = 0f;

    private List<Target> potentialTargets = new List<Target>();

    private Vector3 preferredAvoidOffset = Vector3.right * 200f;
    private float seed = 0f;

    public bool IsFireAllowed => Mathf.PerlinNoise(seed, Time.time / 10f) < FireChance;

    private enum StrafeState
    {
        Extending,
        Strafing
    }

    public void Init()
    {
        seed = Random.Range(0f, 1000f);
        preferredAvoidOffset = Random.onUnitSphere * 200f;
    }

    public void RunAI()
    {
        RunTargeting();

        if (AttackTarget == null)
            FindTarget();
        else
        {
            if (AttackTarget.IsCapital)
                StrafeTarget();
            else
                DogfightTarget();
        }

        if (AttackTarget == null)
        {
            if (FollowTarget != null)
            {
                TurnTowards(FollowTarget.position);
            }
            else
            {
                Throttle = 0f;
                IsFiring = false;
                Pitch = 0f;
                Yaw = 0f;
                Roll = 0f;
            }
        }
    }

    private void RunTargeting()
    {
        if (Time.time > lastTargetCheck + thinkDelay)
        {
            thinkDelay = Random.Range(1f, 2f);

            if (AttackTarget == null)
            {
                AttackTarget = FindTarget();
                if (AttackTarget != null && AttackTarget.IsCapital)
                    strafeTargetOffset = AttackTarget.GetRandomPointOnTarget();
            }
        }
    }

    private void StrafeTarget()
    {
        if (AttackTarget == null)
            return;

        if (strafeState == StrafeState.Strafing)
        {
            Throttle = .8f;

            bool shouldExtend = AttackTarget.DistanceFromAllColliders(ownTarget.Position) < 200f;
            if (shouldExtend)
            {
                extendPosition = ownTarget.Up * 1000f + AttackTarget.Position;
                strafeState = StrafeState.Extending;
                IsFiring = false;
            }
            else
            {
                var attackPoint = AttackTarget.GetLocalToWorldPoint(strafeTargetOffset);
                attackPoint = GunMaths.ComputeGunLead(
                    attackPoint,
                    AttackTarget.Velocity,
                    ownTarget.Position,
                    Vector3.zero,
                    ownShip.Specs.Weapons.MuzzleVelocity);

                TurnTowards(AttackTarget.Transform.TransformPoint(strafeTargetOffset));

                // Fire when within parameters.
                var angleToAttack = Vector3.Angle(ownTarget.Forward, attackPoint - ownTarget.Position);
                var distanceToAttack = Vector3.Distance(attackPoint, ownTarget.Position);
                IsFiring = angleToAttack < 5f && distanceToAttack < 500f;
            }
        }
        else
        {
            Throttle = 1f;
            IsFiring = false;

            var distanceToExtend = Vector3.Distance(extendPosition, ownTarget.Position);
            if (distanceToExtend < 100f)
            {
                strafeState = StrafeState.Strafing;
                strafeTargetOffset = AttackTarget.GetRandomPointOnTarget();
            }
            else
            {
                TurnTowards(extendPosition);
            }
        }
    }

    private void DogfightTarget()
    {
        if (AttackTarget == null)
            return;

        // Evasive maneuvers!
        var distance = Vector3.Distance(AttackTarget.Position, ownTarget.Position);
        if (distance < 100f)
        {
            IsFiring = false;
            TurnTowards(AttackTarget.Position + preferredAvoidOffset);

            Throttle = 0.4f;
        }
        else
        {
            // Get lead on target.
            var targetPoint = GunMaths.ComputeGunLead(
                AttackTarget.Position,
                AttackTarget.Velocity,
                ownTarget.Position,
                Vector3.zero,
                ownShip.Specs.Weapons.MuzzleVelocity);

            TurnTowards(targetPoint, Mathf.PerlinNoise(seed, Time.time / 10f) * MaxTurnPower);
            var angleToTarget = Vector3.Angle(
                ownShip.transform.forward,
                AttackTarget.Position - ownTarget.Position);

            IsFiring = angleToTarget < 5f && IsFireAllowed && distance < 300f;

            if (Vector3.Angle(AttackTarget.Forward, ownTarget.Forward) < 90)
            {
                // Adjust throttle based on distance so faster ships don't overtake slower ones.
                Throttle = Maths.Remap(50f, 250f, .33f, .8f, distance);
            }
            else
                Throttle = 0.85f;
        }
    }

    private void TurnTowards(Vector3 point, float power = 1f)
    {
        Vector3 localPosition = ownShip.transform.InverseTransformPoint(point);

        Pitch = -localPosition.y;
        Yaw = localPosition.x;

        Pitch = Mathf.Clamp(Pitch, -1f, 1f) * power;
        Yaw = Mathf.Clamp(Yaw, -1f, 1f) * power;

        // Capitals auto-level.
        if (ownShip.Specs.IsCapital)
            Roll = Mathf.Clamp(ownShip.transform.right.y * 5f, -1f, 1f);

        if (localPosition.z > 0f)
            Throttle = localPosition.z / 100f;
        else
            Throttle = 0.2f;
        Throttle = Mathf.Clamp(Throttle, 0f, 1f);
    }

    private Target FindTarget()
    {
        potentialTargets.Clear();
        foreach (var target in Target.AllTargets)
        {
            if (target == ownTarget)
                continue;
            if (target.Faction != ownTarget.Faction)
                potentialTargets.Add(target);
        }

        // Pick random one.
        if (potentialTargets.Count > 0)
            return potentialTargets[Random.Range(0, potentialTargets.Count)];
        else
            return null;
    }

    public void CapturePlayerInput()
    {
        Yaw = Input.GetAxis("Horizontal");
        Pitch = Input.GetAxis("Vertical");
        Roll = Input.GetAxis("Horizontal") * .5f;
        Throttle = Maths.Remap(-1f, 1f, 0f, 1f, Input.GetAxis("Throttle"));

        IsFiring = Input.GetButton("Fire1");
    }
}
