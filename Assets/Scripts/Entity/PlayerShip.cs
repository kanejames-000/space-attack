using UnityEngine;

public class PlayerShip : Ship
{
    [Header("Controls")]
    [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode moveRightKey = KeyCode.D;
    [SerializeField] private KeyCode fireKey = KeyCode.Space;
    private void FixedUpdate()
    {
        MovementInput();
        FireInput();
    }

    private void MovementInput()
    {
        if (Input.GetKey(moveLeftKey))
        {
            Move(transform.right * -1);
        }
        else if (Input.GetKey(moveRightKey))
        {
            Move(transform.right);
        }
    }

    private void FireInput()
    {
        if (Input.GetKey(fireKey))
        {
            Fire();
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowGameOver();
        }

        gameObject.SetActive(false);
    }
}
