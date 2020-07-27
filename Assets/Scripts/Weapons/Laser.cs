using UnityEngine;

public class Laser : MonoBehaviour
{
    public float TimeToLive = 5f;
    public int Damage = 10;
    public ParticleSystem HitFXPrefab = null;

    public bool IsFired { get; private set; } = false;

    private Ship firedFrom = null;
    private float speed = 300f;

    private void FixedUpdate()
    {
        if (!IsFired)
            return;

        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        var hitObject = other.transform.GetComponentInParent<Ship>();
        bool wasSomethingHit = false;

        if (hitObject != null && hitObject != firedFrom)
        {
            if (hitObject != firedFrom)
            {
                var damageEvent = new DamageEventParams { Damage = Damage, DamagedBy = firedFrom };
                hitObject.ApplyDamage(damageEvent);
            }

            wasSomethingHit = true;
        }

        if (wasSomethingHit)
        {
            var hitPos = other.ClosestPoint(transform.position);
            Instantiate(HitFXPrefab, hitPos, transform.rotation);
            Destroy(gameObject);
        }
    }

    public void Fire(Ship firedFrom, float muzzleVelocity)
    {
        IsFired = true;
        speed = muzzleVelocity;
        this.firedFrom = firedFrom;

        Destroy(gameObject, TimeToLive);
    }
}
