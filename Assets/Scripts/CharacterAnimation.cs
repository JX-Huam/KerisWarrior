using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimation : MonoBehaviour
{
    private Animator animator;
    private Vector3 lastPosition;

    void Awake()
    {
        animator = GetComponent<Animator>();
        lastPosition = transform.position;
    }

    void Update()
    {
        // Calculate speed based on movement since last frame
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;

        // Clamp to avoid jittery values if not moving
        if (speed < 0.01f) speed = 0f;

        animator.SetFloat("speed", speed);

        lastPosition = transform.position;
    }
}
