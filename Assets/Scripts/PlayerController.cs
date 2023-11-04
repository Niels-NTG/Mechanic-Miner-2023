using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Part of this is based on https://gist.github.com/bendux/5fab0c176855d4e37bf6a38bb071b4a4

    [Header("Only listen to input devices if true, should be set to false while simulation is running.")]
    public bool allowHumanInput;

    private float horizontal;
    public float speed = 8f;
    // Default value of 6.6 is just enough to jump over 2 blocks.
    public float jumpingPower = 7.9f;

    public Rigidbody2D rigidBody;
    public LayerMask groundLayer;
    public LayerMask exitLayer;
    public LayerMask killLayer;
    public EdgeCollider2D groundCollider;

    public ToggleableGameMechanic toggleableGameMechanic;

    [Header("Components of the player that can be used to construct a Toggleable Game Mechanic (TGM)")]
    public List<Component> componentsWithToggleableProperties;

    [NonSerialized]
    public bool hasTouchedExit;
    [NonSerialized]
    public bool hasTouchedSpikes;

    public Vector2 Jump()
    {
        if (IsGrounded())
        {
            Vector2 jumpVector = new Vector2(rigidBody.velocity.x, jumpingPower);
            rigidBody.velocity = jumpVector;
            return jumpVector;
        }
        return Vector2.zero;
    }

    public Vector2 MoveLeft()
    {
        return Vector2.left * speed;
    }

    public Vector2 MoveRight()
    {
        return Vector2.right * speed;
    }

    public void ToggleSpecial()
    {
        toggleableGameMechanic.Toggle();
    }

    public bool IsGrounded()
    {
        return Physics2D.IsTouchingLayers(groundCollider, groundLayer);
    }

    public void ResetPlayer()
    {
        hasTouchedExit = false;
        hasTouchedSpikes = false;
        gameObject.SetActive(true);
        if (toggleableGameMechanic != null)
        {
            toggleableGameMechanic.Reset();
        }
    }

    void Update()
    {
        if (!allowHumanInput)
        {
            return;
        }
        horizontal = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        if (Input.GetButtonDown("Special"))
        {
            ToggleSpecial();
        }
    }

    private void FixedUpdate()
    {
        rigidBody.velocity = new Vector2(horizontal * speed, rigidBody.velocity.y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject otherGameObject = other.gameObject;
        hasTouchedExit = ((1 << otherGameObject.layer) & exitLayer) != 0;
        hasTouchedSpikes = ((1 << otherGameObject.layer) & killLayer) != 0;
        if (hasTouchedExit || hasTouchedSpikes)
        {
            gameObject.SetActive(false);
        }
    }
}
