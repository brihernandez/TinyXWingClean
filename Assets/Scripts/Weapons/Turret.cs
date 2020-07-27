using UnityEngine;
using System.Collections.Generic;

public class Turret
{
    private Transform turret = null;
    private Laser laserPrefab = null;

    private Ship ownShip = null;
    private Target ownTarget = null;
    private Weapons specs;

    private AudioSource audio = null;

    private float seed = 0f;
    private float lastFireTime = -float.MaxValue;

    private float lastTargetCheck = 0f;
    private float thinkDelay = 0f;

    private float fireChance = .5f;

    private List<Target> potentialTargets = new List<Target>();

    private Target Target = null;

    private bool IsFireAllowed => Mathf.PerlinNoise(seed, Time.time / 10f) < fireChance;

    public Turret(Ship ownShip, Target ownTarget, AudioSource audio, Transform turret, Laser prefab, float fireChance, Weapons weaponSpecs)
    {
        this.ownShip = ownShip;
        this.ownTarget = ownTarget;
        this.turret = turret;
        laserPrefab = prefab;
        this.fireChance = fireChance;
        specs = weaponSpecs;
        this.audio = audio;

        seed = Random.Range(0f, 1000f);
    }

    public void Update()
    {
        RunTargeting();

        if (Target != null)
        {
            var distanceToTarget = Vector3.Distance(Target.Position, turret.position);
            if (distanceToTarget < (specs.MuzzleVelocity * laserPrefab.TimeToLive) && IsFireAllowed)
                AttemptFire();
        }
    }

    private void RunTargeting()
    {
        if (Time.time > lastTargetCheck + thinkDelay)
        {
            thinkDelay = Random.Range(1f, 2f);

            if (Target != null)
            {
                var distanceToTarget = Vector3.Distance(turret.position, Target.Position);
                if (distanceToTarget > 350f)
                    Target = FindTarget();
            }
            else
            {
                Target = FindTarget();
            }
        }
    }

    public bool AttemptFire()
    {
        bool wasFired = false;
        if (Time.time > lastFireTime + specs.FireDelay)
        {
            var targetPoint = GunMaths.ComputeGunLead(
                Target.Position,
                Target.Velocity,
                turret.position,
                Vector3.zero,
                specs.MuzzleVelocity);

            var rotationToTarget = Quaternion.LookRotation(targetPoint - turret.position);
            var laser = Object.Instantiate(laserPrefab, turret.position, rotationToTarget);

            laser.Damage = specs.Damage;
            laser.Fire(ownShip, specs.MuzzleVelocity);
            lastFireTime = Time.time;
            audio.transform.position = turret.position;
            audio.Play();
            wasFired = true;
        }

        return wasFired;
    }

    public Target FindTarget()
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
}
