using UnityEngine;

public class ClampPlayer : MonoBehaviour
{
    void Update()
    {
        float x = Mathf.Clamp(transform.position.x, -8.5f, 7f);
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
}

