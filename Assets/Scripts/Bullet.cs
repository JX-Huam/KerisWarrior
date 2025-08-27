using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public AudioClip EDamageSFX;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        if (transform.position.z > 20)
            Destroy(gameObject);
    }

   void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Enemy"))
    {
        GameManager gm = FindObjectOfType<GameManager>();
        gm.AddScore(10);
        gm.mysterySource.PlayOneShot(EDamageSFX);
        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}

}
