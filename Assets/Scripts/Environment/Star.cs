using UnityEngine;

public class Star : MonoBehaviour
{
    public MeshRenderer mesh = null;

    public float MinSize = 1.5f;
    public float MaxSize = 3.5f;

    // Use this for initialization
    void Start()
    {
        mesh.transform.localScale = Vector3.one * Random.Range(MinSize, MaxSize);
    }
}
