using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    private bool movingRight = true;
    private float xLimit = 10f;       // Side-to-side movement limit
    private float stepDownZ = 1f;    // How much to move down each time
    private float gameOverZ = -8f;   // Z-position that triggers game over

    [Header("Shooting")]
    public GameObject enemyBulletPrefab;
    public float shootIntervalMin = 5f;
    public float shootIntervalMax = 10f;
    private float nextShootTime;

    [Header("Game Management")]
    public GameManager gameManager;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        SetNextShootTime();
    }

    void Update()
    {
        if (gameManager != null && gameManager.isGameOver) return;

        MoveEnemy();

        if (Time.time >= nextShootTime)
        {
            Shoot();
            SetNextShootTime();
        }

        // Check if enemy has reached the game over position
        CheckGameOverCondition();
    }

    void MoveEnemy()
    {
        float move = moveSpeed * Time.deltaTime;

        if (movingRight)
            transform.Translate(move, 0, 0);
        else
            transform.Translate(-move, 0, 0);

        if (transform.position.x >= xLimit)
        {
            movingRight = false;
            MoveDown();
        }
        else if (transform.position.x <= -xLimit)
        {
            movingRight = true;
            MoveDown();
        }
    }

    void MoveDown()
    {
        transform.Translate(0, 0, -stepDownZ);
    }

    void SetNextShootTime()
    {
        nextShootTime = Time.time + Random.Range(shootIntervalMin, shootIntervalMax);
    }

    void Shoot()
    {
        if (enemyBulletPrefab == null)
        {
            Debug.LogWarning("Enemy bullet prefab is not assigned!");
            return;
        }

        Vector3 bulletSpawnPos = transform.position + Vector3.back * 1.5f;
        Instantiate(enemyBulletPrefab, bulletSpawnPos, Quaternion.identity);
    }

    void CheckGameOverCondition()
    {
        if (transform.position.z <= gameOverZ)
        {
            if (gameManager != null)
                gameManager.GameOver();
        }
    }
}