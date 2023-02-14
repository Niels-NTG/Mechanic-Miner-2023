using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Part of this is based on https://gist.github.com/bendux/5fab0c176855d4e37bf6a38bb071b4a4
    
    private float horizontal;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpingPower = 8f;

    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private EdgeCollider2D groundCollider;

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpingPower);
        }
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
