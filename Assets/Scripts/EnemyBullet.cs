using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        transform.Translate(Vector3.back * speed * Time.deltaTime);
        if (transform.position.z < -10)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        other.GetComponent<PlayerHealth>().TakeDamage();
        Destroy(gameObject);
    }
}
}
