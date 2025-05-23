using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpatialAudio : MonoBehaviour
{
    [Header("Distance Settings")]
    public float maxDistance = 20f;
    public float minPitch = 0.8f;
    public float maxPitch = 1.5f;

    [Header("Beep Settings")]
    public AudioClip beepClip;         // short beep sound
    public float minBeepRate = 0.5f;
    public float maxBeepRate = 5f;

    [Header("Voice Cues")]
    public AudioClip turnLeftClip;
    public AudioClip turnRightClip;
    public float angleThreshold = 15f; // degrees off forward
    public float voiceCooldown = 2f;   // seconds between voice prompts

    private AudioSource src;
    private float beepTimer = 0f;
    private float voiceTimer = 0f;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        src.loop = false;
        src.playOnAwake = false;
    }

    void Update()
    {
        Transform target = TaskManager.Instance.GetCurrentTarget();
        if (!target) return;

        Vector3 toTarget = target.position - transform.position;
        float dist = toTarget.magnitude;

        // --- 1) Adjust volume and pitch by distance ---
        float t = Mathf.Clamp01(1f - dist / maxDistance);
        src.volume = t;
        src.pitch = Mathf.Lerp(minPitch, maxPitch, t);

        // --- 2) Compute angle to target ---
        float angle = Vector3.SignedAngle(transform.forward, toTarget, Vector3.up);
        voiceTimer -= Time.deltaTime;

        // --- 3) Voice cue if turning needed ---
        if (voiceTimer <= 0f && Mathf.Abs(angle) > angleThreshold)
        {
            AudioClip cue = angle > 0 ? turnRightClip : turnLeftClip;
            if (cue != null)
            {
                src.Stop();           // stop current sound (e.g. beep)
                src.clip = cue;
                src.Play();
                voiceTimer = voiceCooldown;
                return;              // skip beep while voice cue plays
            }
        }

        // --- 4) Beep logic (if not currently playing voice) ---
        if (!src.isPlaying)
        {
            float beepRate = Mathf.Lerp(minBeepRate, maxBeepRate, t);
            beepTimer += Time.deltaTime;
            if (beepTimer >= 1f / beepRate)
            {
                src.clip = beepClip;
                src.Play();
                beepTimer = 0f;
            }
        }

        // --- 5) Stereo panning ---
        Vector3 localDir = transform.InverseTransformDirection(toTarget.normalized);
        src.panStereo = Mathf.Clamp(localDir.x, -1f, 1f);
    }
}
