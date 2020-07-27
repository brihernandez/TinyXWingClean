using UnityEngine;

public class Flyby : MonoBehaviour
{
    public AudioSource source = null;
    public AudioClip[] clips = System.Array.Empty<AudioClip>();
    public float flybyDistance = 75f;

    private float flybyCooldown = 15f;

    private Transform listener = null;

    private Ship ownShip = null;

    private void Awake()
    {
        ownShip = GetComponentInParent<Ship>();
        flybyCooldown = Random.Range(0, 15f);
    }

    private void FixedUpdate()
    {
        if (ownShip.Pilot.IsPlayer)
            return;

        if (listener == null)
        {
            // This is super bad, but it only happens if there is no camera and if
            // there is no camera that means something has gone very wrong and you wouldn't
            // be there to see it anyway. :)
            var listenerObject = FindObjectOfType<AudioListener>();
            if (listenerObject != null)
                listener = listenerObject.transform;
        }

        if (listener != null)
        {
            flybyCooldown -= Time.fixedDeltaTime;

            if (flybyCooldown <= 0f)
            {
                float distanceToListener = Vector3.Distance(listener.position, transform.position);
                if (distanceToListener < flybyDistance && flybyCooldown <= 0f)
                {
                    source.clip = clips[Random.Range(0, clips.Length)];
                    source.Play();
                    Debug.Log($"Trigger flyby {ownShip.name}");
                    flybyCooldown = Random.Range(10f, 15f);
                }
            }
        }
    }
}
