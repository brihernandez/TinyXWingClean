using UnityEngine;
using UnityEngine.Rendering;

public class Billboard : MonoBehaviour
{
    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        transform.forward = -camera.transform.forward;
    }
}
