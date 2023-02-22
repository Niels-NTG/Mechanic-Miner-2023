using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Part of this is based on https://gist.github.com/bendux/5fab0c176855d4e37bf6a38bb071b4a4
    
    private float horizontal;
    public float speed = 8f;
    public float jumpingPower = 8f;

    public Rigidbody2D rigidBody;
    public LayerMask groundLayer;
    public LayerMask exitLayer;
    public EdgeCollider2D groundCollider;

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
    }

    public void Jump()
    {
        if (IsGrounded())
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpingPower);
        }
    }

    public void MoveLeft()
    {
        horizontal = -1f;
    }
    
    public void MoveRight()
    {
        horizontal = 1f;
    }

    private void FixedUpdate()
    {
        rigidBody.velocity = new Vector2(horizontal * speed, rigidBody.velocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.IsTouchingLayers(groundCollider, groundLayer);
    }
}
