using UnityEngine;

public class RandomAudioClipSelector : MonoBehaviour
{
    public AudioSource source = null;
    public AudioClip[] clips = System.Array.Empty<AudioClip>();

    private void Awake()
    {
        source.clip = clips[Random.Range(0, clips.Length)];
    }

    private void Start()
    {
        source.Play();
    }
}
