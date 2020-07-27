using UnityEngine;
using UnityEngine.Rendering;

public class LaserScaling : MonoBehaviour
{
    public float Multiplier = .01f;
    private Vector3 startScale = Vector3.one;

    private void Awake()
    {
        startScale = transform.localScale;

        RenderPipelineManager.beginCameraRendering += OnCameraRender;
    }

    private void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= OnCameraRender;
    }

    private void OnCameraRender(ScriptableRenderContext context, Camera camera)
    {
        var distance = Vector3.Distance(camera.transform.position, transform.position);

        Vector3 scale = startScale;
        scale.x *= Mathf.Max(1f, distance * Multiplier);
        scale.y *= Mathf.Max(1f, distance * Multiplier);
        scale.z *= Mathf.Max(1f, distance / 20f * Multiplier);

        transform.localScale = scale;
    }
}
