using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Footstep : MonoBehaviour
{
    public AudioClip[] footstepClips;
    public float stepInterval = 0.5f; // time between steps while walking
    public float minSpeed = 0.1f;     // minimum movement to count as walking

    private AudioSource src;
    private Transform player;
    private Vector3 lastPos;
    private float stepTimer;

    void Start()
    {
        src = GetComponent<AudioSource>();
        player = transform.parent; // assumes FootstepAudio is a child of player
        lastPos = player.position;
    }

    void Update()
    {
        float speed = (player.position - lastPos).magnitude / Time.deltaTime;
        lastPos = player.position;

        if (speed > minSpeed)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = stepInterval; // reset so it's instant when moving again
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        src.clip = clip;
        src.Play();
    }
}
