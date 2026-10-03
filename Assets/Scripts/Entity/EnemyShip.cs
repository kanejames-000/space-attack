using UnityEngine;

public class EnemyShip : Ship
{
    public int ScoreAwarded = 10;
    [SerializeField] private int collideDamage = 1;
    [Header("Movement")]
    [SerializeField, Min(0f)] private float horizontalSpeed = 2f;
    [SerializeField, Min(0f)] private float downwardSpeed = 1f;
    [SerializeField] private float resetYPosition = 12f;
    [SerializeField, Min(0f)] private float minHorizontalMoveDuration = 0.5f;
    [SerializeField, Min(0f)] private float maxHorizontalMoveDuration = 1.5f;
    [SerializeField, Min(0f)] private float minHorizontalPauseDuration = 0.25f;
    [SerializeField, Min(0f)] private float maxHorizontalPauseDuration = 0.75f;
    [Header("Firing")]
    [SerializeField, Min(0f)] private float fireInterval = 2f;
    private float nextEnemyFireTime;

    private Rigidbody2D enemyRigidbody;
    private float nextMovementChangeTime;
    private float horizontalDirection;

    protected override void Awake()
    {
        base.Awake();
        enemyRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        BeginHorizontalMove();

        // Add a random offset time to ensure not all enemies fire at once
        nextEnemyFireTime = Time.time + Random.Range(0f, fireInterval);
    }

    private void FixedUpdate()
    {
        if (Time.time >= nextEnemyFireTime)
        {
            Fire();
            nextEnemyFireTime = Time.time + fireInterval;
        }

        if (Time.time >= nextMovementChangeTime)
        {
            if (horizontalDirection == 0f)
            {
                BeginHorizontalMove();
            }
            else
            {
                BeginHorizontalPause();
            }
        }

        enemyRigidbody.velocity = new Vector2(horizontalDirection * horizontalSpeed, -downwardSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("BoundReset"))
        {
            Vector2 position = enemyRigidbody.position;
            position.y = resetYPosition;
            enemyRigidbody.position = position;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerShip"))
        {
            Entity otherEntity = collision.collider.GetComponentInParent<Entity>();
            if (otherEntity is Ship ship)
            {
                ship.TakeDamage(collideDamage);
            }
            OnDeath();
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterEnemyShip(this);
        }
    }

    private void BeginHorizontalMove()
    {
        horizontalDirection = Random.value < 0.5f ? -1f : 1f;
        nextMovementChangeTime = Time.time + Random.Range(minHorizontalMoveDuration, maxHorizontalMoveDuration);
    }

    private void BeginHorizontalPause()
    {
        horizontalDirection = 0f;
        nextMovementChangeTime = Time.time + Random.Range(minHorizontalPauseDuration, maxHorizontalPauseDuration);
    }

    private void OnValidate()
    {
        maxHorizontalMoveDuration = Mathf.Max(minHorizontalMoveDuration, maxHorizontalMoveDuration);
        maxHorizontalPauseDuration = Mathf.Max(minHorizontalPauseDuration, maxHorizontalPauseDuration);
        fireInterval = Mathf.Max(0.1f, fireInterval);
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        GameManager.Instance.AddScore(ScoreAwarded);
        Destroy(gameObject);
    }
}
