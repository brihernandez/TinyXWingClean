using UnityEngine;

public class EngineAudio : MonoBehaviour
{
    public Ship ship = null;
    public new AudioSource audio = null;

    public float MinVolume = 0.4f;
    public float MaxVolume = 0.8f;

    public float MinPitch = 0.8f;
    public float MaxPitch = 1f;

    private float smoothPitch = 1f;
    private float smoothVolume = 0f;

    private void Start()
    {
        audio.volume = 0f;
        audio.pitch = MinPitch;
    }

    private void FixedUpdate()
    {
        var targetVolume = Mathf.Lerp(MinVolume, MaxVolume, ship.Pilot.Throttle);
        smoothVolume = SmoothDamp.Move(audio.volume, targetVolume, 0.5f, Time.fixedDeltaTime);
        audio.volume = smoothVolume;

        var targetPitch = Mathf.Lerp(MinPitch, MaxPitch, ship.Pilot.Throttle);
        smoothPitch = SmoothDamp.Move(audio.pitch, targetPitch, 0.5f, Time.fixedDeltaTime);
        audio.pitch = smoothPitch;
    }
}
