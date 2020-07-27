using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private Text frameRate = null;

    private Queue<float> deltaTimes = new Queue<float>();

    private void Update()
    {
        int rawFPS = Mathf.FloorToInt(1f / Time.unscaledDeltaTime);

        // Average the time over the past second.
        while (deltaTimes.Count > rawFPS)
            deltaTimes.Dequeue();

        deltaTimes.Enqueue(Time.unscaledDeltaTime);

        var averageTime = 0f;
        for (int i = 0; i < deltaTimes.Count; ++i)
        {
            var delta = deltaTimes.Dequeue();
            averageTime += delta;
            deltaTimes.Enqueue(delta);
        }

        if (deltaTimes.Count > 0)
        {
            averageTime /= deltaTimes.Count;
            int displayFPS = Mathf.RoundToInt(1f / averageTime);
            displayFPS = Mathf.Clamp(displayFPS, 0, 299);
            frameRate.text = displayFPS.ToString();
        }
    }
}
