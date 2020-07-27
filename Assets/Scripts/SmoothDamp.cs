using UnityEngine;

static class SmoothDamp
{
    // Thanks to Rory Driscoll
    // http://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/
    /// <summary>
    /// Creates dampened motion between a and b that is framerate independent.
    /// </summary>
    /// <param name="a">Initial parameter</param>
    /// <param name="b">Target parameter</param>
    /// <param name="speed">Smoothing factor</param>
    /// <param name="dt">Time since last damp call</param>
    /// <returns></returns>
    static public Quaternion Rotate(Quaternion a, Quaternion b, float speed, float dt)
    {
        return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-speed * dt));
    }

    // Thanks to Rory Driscoll
    // http://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/
    /// <summary>
    /// Creates dampened motion between a and b that is framerate independent.
    /// </summary>
    /// <param name="a">Initial parameter</param>
    /// <param name="b">Target parameter</param>
    /// <param name="speed">Smoothing factor</param>
    /// <param name="dt">Time since last damp call</param>
    /// <returns></returns>
    static public Vector3 Move(Vector3 a, Vector3 b, float speed, float dt)
    {
        return Vector3.Lerp(a, b, 1 - Mathf.Exp(-speed * dt));
    }

    // Thanks to Rory Driscoll
    // http://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/
    /// <summary>
    /// Creates dampened motion between a and b that is framerate independent.
    /// </summary>
    /// <param name="a">Initial parameter</param>
    /// <param name="b">Target parameter</param>
    /// <param name="speed">Smoothing factor</param>
    /// <param name="dt">Time since last damp call</param>
    /// <returns></returns>
    static public float Move(float a, float b, float speed, float dt)
    {
        return Mathf.Lerp(a, b, 1 - Mathf.Exp(-speed * dt));
    }
}
