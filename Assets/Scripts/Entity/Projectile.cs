using UnityEngine;

public class Projectile : Entity
{
    private const float MaxLifetimeSeconds = 3f;

    [SerializeField, Min(0)] private int damage = 1;
    [SerializeField, Min(0f)] private float maxSpeed = 10f;
    private Vector2 direction = Vector2.up;
    private Rigidbody2D projectileRigidbody;

    public Ship Source { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        projectileRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, MaxLifetimeSeconds);
    }

    public void SetSource(Ship source)
    {
        Source = source;
    }

    public void SetDirection(Vector2 newDirection)
    {
        if (newDirection.sqrMagnitude > 0f)
        {
            direction = newDirection.normalized;
        }
    }

    private void FixedUpdate()
    {
        Move(direction);
        projectileRigidbody.velocity = Vector2.ClampMagnitude(projectileRigidbody.velocity, maxSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Entity otherEntity = collision.collider.GetComponentInParent<Entity>();
        if (otherEntity == null || otherEntity == Source)
        {
            return;
        }

        if (otherEntity is Projectile otherProjectile)
        {
            Destroy(otherProjectile.gameObject);
        }
        else if (otherEntity is Ship ship)
        {
            ship.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
