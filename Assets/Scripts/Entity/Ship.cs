using UnityEngine;

public abstract class Ship : Entity
{
    [Header("Firing")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Vector3 fireOffset = new Vector3(0, 3, 0);
    [SerializeField] private Vector2 fireDirection = Vector2.up;
    [SerializeField, Min(0f)] private float fireCooldown = 1f;
    private float nextFireTime;

    [Header("Stats")]
    [SerializeField, Min(1)] private int health = 3;
    private int startingHealth;

    public int Health => health;

    protected override void Awake()
    {
        base.Awake();
        startingHealth = health;
    }

    public void ResetHealth()
    {
        health = startingHealth;
        nextFireTime = 0f;
    }

    protected void Fire()
    {
        if (projectilePrefab == null || Time.time < nextFireTime)
        {
            return;
        }

        GameObject projectileObject = Instantiate(projectilePrefab, transform.position + fireOffset, Quaternion.identity);
        Projectile projectile = projectileObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.SetSource(this);
            projectile.SetDirection(fireDirection);
        }

        nextFireTime = Time.time + fireCooldown;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || health <= 0)
        {
            return;
        }

        health = Mathf.Max(0, health - damage);
        if (this is PlayerShip && GameManager.Instance != null)
        {
            GameManager.Instance.UpdatePlayerHealth(health);
        }

        if (health == 0)
        {
            OnDeath();
        }
    }

    protected virtual void OnDeath()
    {

    }
}
