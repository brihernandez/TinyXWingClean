using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Fuze
{
    public float TriggerTime = 1f;

    [Tooltip("Triggers a random explosion somewhere on the ship hull.")]
    public bool IsRandomExplosion = true;

    [Tooltip("Effect to play when this is triggered.")]
    public ParticleSystem ExplosionPrefab = null;

    [Tooltip("GameObject to be activated and physicalized for debris. " +
        "This is only meant for the final destruction of the ship.")]
    public GameObject Debris = null;

    [HideInInspector]
    public bool HasBeenTriggered = false;
}

public class CapitalDestruction : MonoBehaviour
{
    public float DestroyTime = 10f;
    public List<Fuze> Fuzes = new List<Fuze>();

    private bool isDestroyed = false;

    private float destroyTime = 0f;

    private Target ownTarget = null;
    private float shipMass = 10000f;

    private void Awake()
    {
        ownTarget = GetComponent<Target>();
        shipMass = GetComponent<Ship>().Specs.Physics.Mass;
    }

    public void Update()
    {
        if (!isDestroyed)
            return;

        destroyTime += Time.deltaTime;
        foreach (var fuze in Fuzes)
        {
            if (fuze.HasBeenTriggered)
                continue;

            if (fuze.TriggerTime <= destroyTime)
            {
                SpawnExplosion(fuze);
                PhysicalizeMesh(fuze);

                fuze.HasBeenTriggered = true;
            }
        }

        if (destroyTime > DestroyTime)
        {
            Destroy(gameObject);
        }
    }

    private void PhysicalizeMesh(Fuze fuze)
    {
        if (fuze.Debris == null)
            return;

        // Disable physics on the own ship to prevent weird physics forces from
        // spawning things inside each other. This also assumes that debris separation
        // is the very last thing that happens.
        GetComponent<Rigidbody>().isKinematic = true;

        var debris = fuze.Debris;
        debris.transform.SetParent(null);
        debris.SetActive(true);

        var meshes = fuze.Debris.GetComponentsInChildren<MeshRenderer>();
        foreach (var mesh in meshes)
        {
            var collider = mesh.gameObject.AddComponent<MeshCollider>();
            collider.convex = true;
        }

        var rigid = debris.AddComponent<Rigidbody>();
        rigid.useGravity = false;
        rigid.ResetInertiaTensor();
        rigid.ResetCenterOfMass();
        rigid.mass = shipMass;
        rigid.angularVelocity = Random.insideUnitSphere * 3f * Mathf.Deg2Rad;
        rigid.velocity = (debris.transform.position - transform.position).normalized * 5f;
        rigid.drag = .02f;
        rigid.angularDrag = .1f;
    }

    private void SpawnExplosion(Fuze fuze)
    {
        if (fuze.ExplosionPrefab == null)
            return;

        var position = transform.position;
        if (fuze.IsRandomExplosion)
        {
            position = ownTarget.GetRandomPointOnTarget();
            position = transform.TransformPoint(position);
        }

        Instantiate(fuze.ExplosionPrefab, position, transform.rotation);
    }

    public void DestroyShip()
    {
        isDestroyed = true;
    }
}
