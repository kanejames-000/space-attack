using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public abstract class Entity : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float movementForce = 5f;

    private Rigidbody2D entRigidbody;

    protected virtual void Awake()
    {
        entRigidbody = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Applies physics force in the requested direction. Should be called from FixedUpdate.
    /// /// </summary>
    public virtual void Move(Vector2 direction)
    {
        if (direction.sqrMagnitude == 0f)
        {
            return;
        }

        entRigidbody.AddForce(direction.normalized * movementForce);
    }
}
