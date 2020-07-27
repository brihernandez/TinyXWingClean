using UnityEngine;

public class ToggleHud : MonoBehaviour
{
    public MonoBehaviour Component = null;
    public KeyCode ToggleKey = KeyCode.F1;

    private void Update()
    {
        if (Input.GetKeyDown(ToggleKey))
            Component.enabled = !Component.enabled;
    }
}
