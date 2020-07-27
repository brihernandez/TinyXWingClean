using UnityEngine;
using System.Collections.Generic;

public enum Faction
{
    Imperial,
    Rebel
}

public class Target : MonoBehaviour
{
    public Faction Faction = Faction.Imperial;
    public bool IsCapital = false;

    public Rigidbody Rigidbody { get; private set; } = null;
    public Transform Transform => transform;

    public Vector3 Position => transform.position;
    public Vector3 Velocity => Rigidbody.velocity;
    public Vector3 Forward => transform.forward;
    public Vector3 Right => transform.right;
    public Vector3 Up => transform.up;

    public Ray Ray => new Ray(transform.position, transform.forward);

    public static List<Target> AllTargets = new List<Target>();

    [SerializeField]
    private Collider[] colliders = System.Array.Empty<Collider>();

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        AllTargets.Add(this);
    }

    private void OnDisable()
    {
        AllTargets.Remove(this);
    }

    public Vector3 GetLocalToWorldPoint(Vector3 localSpace)
    {
        return transform.TransformPoint(localSpace);
    }

    /// <summary>
    /// VERY EXPENSIVE! Gets the position of a random vertex on this model in
    /// this target's local space.
    /// </summary>
    public Vector3 GetRandomPointOnTarget()
    {
        var allMeshes = GetComponentsInChildren<MeshFilter>();
        Vector3[] potentialPoints = new Vector3[allMeshes.Length + 1];
        potentialPoints[0] = Vector3.zero;

        //var vertices = new List<Vector3>();

        for (int i = 0; i < allMeshes.Length; ++i)
        {
            var vertices = allMeshes[i].sharedMesh.vertices;
            var position = vertices[Random.Range(0, vertices.Length)];

            // Convert from mesh space to this target's own local space.
            position = allMeshes[i].transform.TransformPoint(position);
            position = transform.InverseTransformPoint(position);

            potentialPoints[i + 1] = position;
        }

        return potentialPoints[Random.Range(0, potentialPoints.Length)];
    }

    public float DistanceFromAllColliders(Vector3 testPoint)
    {
        float minDistance = Vector3.Distance(testPoint, transform.position);
        foreach (var collider in colliders)
        {
            var closestPoint = collider.ClosestPoint(testPoint);
            var distanceToCollider = Vector3.Distance(testPoint, closestPoint);
            minDistance = Mathf.Min(minDistance, distanceToCollider);
        }

        return minDistance;
    }
}
