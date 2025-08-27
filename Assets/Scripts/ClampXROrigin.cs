using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ClampXROrigin : MonoBehaviour
{
    public float minX = -8.5f;
    public float maxX = 7f;

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void LateUpdate()
    {
        // Clamp the XR Origin's position
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        transform.position = pos;

        // Optional: Reset any built-up velocity if you are using gravity
        characterController.Move(Vector3.zero); // Ensures controller updates
    }
}
