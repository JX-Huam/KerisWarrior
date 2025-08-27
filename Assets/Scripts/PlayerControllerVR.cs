using UnityEngine;
using Unity.XR.CoreUtils; // For XROrigin

public class PlayerControllerVR : MonoBehaviour
{
    [Header("XR Origin")]
    [SerializeField] private XROrigin xrOrigin;

    [Header("Clamp Target")]
    [SerializeField] private Transform clampTarget; // The object to follow (X, Y only)

    [Header("Z Position Lock")]
    [SerializeField] private float fixedZ = -8f; // Optional fixed Z value

    void Update()
    {
        ClampXROriginToTarget();
    }

    void ClampXROriginToTarget()
    {
        if (xrOrigin != null && clampTarget != null)
        {
            Vector3 targetPos = clampTarget.position;
            xrOrigin.transform.position = new Vector3(targetPos.x, targetPos.y, fixedZ);
        }
    }
}
