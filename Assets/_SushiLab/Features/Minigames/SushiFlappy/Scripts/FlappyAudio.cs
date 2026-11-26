using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FlappyAudio : MonoBehaviour
{
    [Header("Arrastra los WAV del kit limpio")]
    [SerializeField] private AudioClip flapClip;
    [SerializeField] private AudioClip pointClip;
    [SerializeField] private AudioClip hitClip;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
    }

    public void PlayFlap()
    {
        if (flapClip != null) source.PlayOneShot(flapClip);
    }

    public void PlayPoint()
    {
        if (pointClip != null) source.PlayOneShot(pointClip);
    }

    public void PlayHit()
    {
        if (hitClip != null) source.PlayOneShot(hitClip);
    }
}
