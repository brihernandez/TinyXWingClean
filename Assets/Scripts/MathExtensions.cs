using UnityEngine;

public static class Maths
{
    public static float Remap(float fromMin, float fromMax, float toMin, float toMax, float value)
    {
        float lerp = Mathf.InverseLerp(fromMin, fromMax, value);
        return Mathf.Lerp(toMin, toMax, lerp);
    }
}
