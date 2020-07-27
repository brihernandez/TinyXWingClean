using UnityEngine;

public class StarSphere : MonoBehaviour
{
    public Star starPrefab = null;
    public int starCount = 500;
    public float distance = 10000f;

    private void Awake()
    {
        for (int i = 0; i < starCount; ++i)
        {
            var position = Random.onUnitSphere * 10000f;
            var star = Instantiate(starPrefab, transform);
            star.transform.localPosition = position;
        }
    }

    private void FixedUpdate()
    {
        if (Camera.main != null)
            transform.position = Camera.main.transform.position;
    }
}
