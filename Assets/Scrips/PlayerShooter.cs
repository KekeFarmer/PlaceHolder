using UnityEngine;

/// <summary>
/// Creates a small projectile when the player presses the left mouse button.
/// This component is independent from PlayerCtrl, so movement remains unchanged.
/// </summary>
public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifetime = 2f;
    [SerializeField] private float spawnOffset = 0.45f;
    [SerializeField] private float projectileSize = 0.16f;
    [SerializeField] private Color projectileColor = Color.yellow;
    [Tooltip("Optional bullet image (e.g. ObjX/BalaProyectil). Leave empty to use the yellow square.")]
    [SerializeField] private Sprite bulletSprite;
    [Tooltip("Scale of the bullet image. 1 = its native size (a 16px image at 32 pixels per unit is 0.5 units wide).")]
    [SerializeField, Min(0.01f)] private float bulletSpriteScale = 1f;
    [SerializeField] private int maxAmmo = 5;
    [SerializeField, Min(0)] private int startingCasings = 0;
    [SerializeField] private KeyCode reloadKey = KeyCode.R;
    [SerializeField, Min(0f)] private float reloadTime = 1f;
    [SerializeField, Min(0f)] private float timeBetweenShots = 0.4f;

    private static Sprite projectileSprite;
    private Camera gameCamera;
    private float timeSinceLastShot;
    private bool isReloading;
    private float reloadTimeLeft;

    private void Awake()
    {
        gameCamera = Camera.main;
        AmmoProgress.Init(maxAmmo, startingCasings);
        timeSinceLastShot = timeBetweenShots;
    }

    private void Update()
    {
        if (InventoryUI.IsOpen) return;

        timeSinceLastShot += Time.deltaTime;

        if (isReloading)
        {
            reloadTimeLeft -= Time.deltaTime;
            if (reloadTimeLeft <= 0f)
            {
                AmmoProgress.Reload(maxAmmo);
                isReloading = false;
            }
            return;
        }

        if (Input.GetKeyDown(reloadKey) && AmmoProgress.CanReload(maxAmmo))
        {
            isReloading = true;
            reloadTimeLeft = reloadTime;
            return;
        }

        if (AmmoProgress.Loaded > 0 && timeSinceLastShot >= timeBetweenShots && Input.GetMouseButtonDown(0))
        {
            if (Shoot())
            {
                AmmoProgress.UseRound();
                timeSinceLastShot = 0f;
            }
        }
    }

    private void OnGUI()
    {
        if (InventoryUI.IsOpen) return;
        GUI.Label(new Rect(20f, 50f, 400f, 24f), AmmoProgress.HudText(maxAmmo, isReloading));
    }

    private bool Shoot()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }

        if (gameCamera == null || !TryGetMouseWorldPosition(out Vector3 mouseWorldPosition))
        {
            return false;
        }

        Vector2 direction = (mouseWorldPosition - transform.position).normalized;
        if (direction == Vector2.zero)
        {
            return false;
        }

        Vector3 spawnPosition = transform.position + (Vector3)(direction * spawnOffset);
        spawnPosition.z = transform.position.z;

        bool useBulletSprite = bulletSprite != null;
        float scale = useBulletSprite ? bulletSpriteScale : projectileSize;

        GameObject projectile = new GameObject("Projectile");
        projectile.transform.position = spawnPosition;
        projectile.transform.localScale = Vector3.one * scale;
        if (useBulletSprite)
        {
            // The bullet image points to the right, so turn it toward the shot direction.
            projectile.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        SpriteRenderer spriteRenderer = projectile.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = useBulletSprite ? bulletSprite : CreateProjectileSprite();
        spriteRenderer.color = useBulletSprite ? Color.white : projectileColor;
        spriteRenderer.sortingOrder = 10;

        Rigidbody2D projectileBody = projectile.AddComponent<Rigidbody2D>();
        projectileBody.gravityScale = 0f;
        projectileBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        projectileBody.velocity = direction * projectileSpeed;

        CircleCollider2D projectileCollider = projectile.AddComponent<CircleCollider2D>();
        projectileCollider.isTrigger = true;
        projectileCollider.radius = projectileSize * 0.5f / scale; // same hit size whatever the image
        projectile.AddComponent<Projectile>();
        Destroy(projectile, projectileLifetime);
        return true;
    }

    private bool TryGetMouseWorldPosition(out Vector3 mouseWorldPosition)
    {
        Ray mouseRay = gameCamera.ScreenPointToRay(Input.mousePosition);
        Plane playerPlane = new Plane(Vector3.forward, transform.position);

        if (playerPlane.Raycast(mouseRay, out float rayDistance))
        {
            mouseWorldPosition = mouseRay.GetPoint(rayDistance);
            return true;
        }

        mouseWorldPosition = default;
        return false;
    }

    private static Sprite CreateProjectileSprite()
    {
        if (projectileSprite == null)
        {
            projectileSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
        }

        return projectileSprite;
    }
}
