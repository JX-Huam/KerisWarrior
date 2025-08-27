using UnityEngine;

public class BonusUFO : MonoBehaviour
{
    public float speed = 5f;
    private GameManager gameManager;
    public AudioClip   mysterySFX;

    public void SetManager(GameManager gm)
    {
        gameManager = gm;
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Destroy if off screen
        if (transform.position.x > 10f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            gameManager.mysterySource.PlayOneShot(mysterySFX);
            Destroy(other.gameObject);
            gameManager.ActivatePowerUp();
            Destroy(gameObject);
        }
    }
}

