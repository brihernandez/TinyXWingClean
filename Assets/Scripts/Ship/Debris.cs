using UnityEngine;

public class Debris : MonoBehaviour
{
    public ParticleSystem DestroyFxPrefab;
    public float TimeToLive = 5f;

    private void FixedUpdate()
    {
        TimeToLive -= Time.fixedDeltaTime;
        if (TimeToLive <= 0f)
        {
            Instantiate(DestroyFxPrefab, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    public void InitializePhysics(Vector3 velocity)
    {
        var rigid = gameObject.AddComponent<Rigidbody>();
        rigid.useGravity = false;
        rigid.velocity = velocity;
        rigid.drag = 0f;
        rigid.angularDrag = 0f;
        rigid.angularVelocity = Random.onUnitSphere * 30f;

        var collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = 1f;
    }
}
