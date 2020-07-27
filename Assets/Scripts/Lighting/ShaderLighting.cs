using UnityEngine;

[RequireComponent(typeof(Light))]
[ExecuteAlways]
public class ShaderLighting : MonoBehaviour
{
    public Light LightSource;
    public Color Ambient = new Color(0.5f, 0.5f, 0.5f);

    private void Update()
    {
        Shader.SetGlobalColor("LightColor", LightSource.color);
        Shader.SetGlobalVector("LightDirection", -LightSource.transform.forward);
        Shader.SetGlobalColor("AmbientColor", Ambient);
    }
}
