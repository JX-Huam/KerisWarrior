using UnityEngine;

public class Shield : MonoBehaviour
{
    public int health = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            Destroy(other.gameObject); // Destroy enemy bullet
            health--;

            if (health <= 0)
            {
                Destroy(gameObject); // Destroy the shield
            }
        }
        else if (other.CompareTag("PlayerBullet"))
        {
            Destroy(other.gameObject); // Destroy player bullet only, no shield damage
        }
    }
}

