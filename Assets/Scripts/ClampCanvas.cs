using UnityEngine;

public class ClampCanvas : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private Canvas canvas;

    [Header("Clamp Target")]
    [SerializeField] private Transform clampTarget; // The object to follow (X, Y only)

    [Header("Z Position Lock")]
    [SerializeField] private float fixedZ = -8f; // Optional fixed Z value

    void Update()
    {
        ClampCanv();
    }

    void ClampCanv()
    {
        if (canvas != null && clampTarget != null)
        {
            Vector3 targetPos = clampTarget.position;
            canvas.transform.position = new Vector3(targetPos.x, targetPos.y, fixedZ);
        }
    }
}
