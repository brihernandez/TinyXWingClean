using UnityEngine;

public class Cannon
{
    private Transform barrel = null;
    private Laser laserPrefab = null;

    private Ship ownShip = null;
    private Weapons specs;

    private float lastFireTime = -float.MaxValue;

    public Cannon(Ship ownShip, Transform barrelTransform, Laser prefab, Weapons weaponSpecs)
    {
        this.ownShip = ownShip;
        barrel = barrelTransform;
        laserPrefab = prefab;
        specs = weaponSpecs;
    }

    public bool AttemptFire()
    {
        bool wasFired = false;
        if (Time.time > lastFireTime + specs.FireDelay)
        {
            var laser = Object.Instantiate(laserPrefab, barrel.position, barrel.rotation);
            laser.Damage = specs.Damage;
            laser.Fire(ownShip, specs.MuzzleVelocity);
            lastFireTime = Time.time;
            wasFired = true;
        }

        return wasFired;
    }
}
