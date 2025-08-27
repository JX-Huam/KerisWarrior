using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    [SerializeField] private InputActionProperty moveAction; // VR joystick input
    [SerializeField] private InputActionProperty shootAction;
    [SerializeField] private InputActionProperty openMenu;
    [SerializeField] public GameObject pausePanel;


    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float shootCooldown = 0.5f;

    [Header("Power-up")]
    private bool isPoweredUp = false;
    private int poweredShots = 0;

    [Header("Health")]
    public int health = 3;

    private float lastShotTime = 0f;

    void Start()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            GameObject currentKeris = gameManager.GetCurrentKerisPrefab();
            SetBulletPrefab(currentKeris);
        }
        else
        {
            Debug.LogWarning("PlayerController: GameManager not found!");
        }

        moveAction.action.Enable(); // Make sure VR input is active
        shootAction.action.Enable();

    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
        PauseGame();
    }

        public void PauseGame()
    {
        if (openMenu.action.WasPressedThisFrame()){
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
        }
    }

    void HandleMovement()
    {
        // Get horizontal input from keyboard
        float keyboardX = Input.GetAxis("Horizontal");

        // Get horizontal input from VR joystick
        float vrX = moveAction.action.ReadValue<Vector2>().x;

        // Combine both (VR takes priority if pressed)
        float finalX = Mathf.Abs(vrX) > 0.01f ? vrX : keyboardX;

        Vector3 move = new Vector3(finalX, 0f, 0f) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);
    }

    void HandleShooting()
    {
        bool keyboardShoot = Input.GetKeyDown(KeyCode.Space);
        bool vrShoot = shootAction.action.WasPressedThisFrame();

        if ((keyboardShoot || vrShoot) && Time.time >= lastShotTime + shootCooldown)
        {
            Shoot();
            lastShotTime = Time.time;
        }
    }

    void Shoot()
    {
        if (isPoweredUp && poweredShots > 0)
        {
            Vector3 left = transform.position + new Vector3(-0.3f, 1.5f, 1);
            Vector3 right = transform.position + new Vector3(0.3f, 1.5f, 1);

            Instantiate(bulletPrefab, left, Quaternion.identity);
            Instantiate(bulletPrefab, right, Quaternion.identity);

            poweredShots--;
            if (poweredShots <= 0)
            {
                isPoweredUp = false;
                FindAnyObjectByType<GameManager>()?.ClearPowerUpUI();
            }
        }
        else
        {
            Vector3 spawnPos = transform.position + new Vector3(0, 1.5f, 1);
            Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        }
    }

    public void SetBulletPrefab(GameObject newBulletPrefab)
    {
        if (newBulletPrefab != null)
        {
            bulletPrefab = newBulletPrefab;
        }
        else
        {
            Debug.LogWarning("PlayerController: Attempted to set null bullet prefab!");
        }
    }

    public void EnablePowerUp(int shots)
    {
        isPoweredUp = true;
        poweredShots = shots;
    }

    public int GetHealth() => health;

    public void SetShootCooldown(float newCooldown)
    {
        shootCooldown = Mathf.Max(0.1f, newCooldown);
    }
}
