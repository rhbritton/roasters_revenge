using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D player;
    // private Animator anim;
    private BoxCollider2D coll;
    // private SpriteRenderer sprite;

    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] private TrailRenderer tr;

    [SerializeField] private float runVelocity = 8.0f;
    [SerializeField] private float jumpVelocity = 15.0f;
    [SerializeField] private float doubleJumpVelocity = 15.0f;

    [SerializeField] private float horizontalWallJumpVelocity = 15.0f;
    [SerializeField] private float verticalWallJumpVelocity = 15.0f;

    [SerializeField] private float dashingPower = 20.0f;
    [SerializeField] private float dashingTime = 0.15f;
    [SerializeField] private float dashingCooldown = 0.25f;

    [SerializeField] private AudioSource jumpSound;
    [SerializeField] private AudioSource dashSound;

    private enum MovementState { idle, running, falling, jumping, doublejumping };

    private float dirx = 0f;
    private float diry = 0f;
    
    private bool isGroundedBool = false;
    private bool canWallJumpBool = false;
    private bool hasDoubleJumped = false;
    // private bool hasWallJumped = false;

    private bool canDash = true;
    private bool isDashing = false;

    // Start is called before the first frame update
    private void Start()
    {
        player = GetComponent<Rigidbody2D>();
        player.freezeRotation = true;

        // anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
        // sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (isDashing)
        {
            return;
        }

        dirx = Input.GetAxisRaw("Horizontal");
        diry = Input.GetAxisRaw("Vertical");
        player.velocity = new Vector2(dirx * runVelocity, player.velocity.y);
        isGroundedBool = isGrounded();
        canWallJumpBool = canWallJump();

        if (isGroundedBool)
        {
            hasDoubleJumped = false;
            // hasWallJumped = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (canWallJumpBool)
            {
                // hasWallJumped = true;
                jumpSound.Play();
                player.velocity = new Vector2(-horizontalWallJumpVelocity, verticalWallJumpVelocity);
            }
            else if (isGroundedBool)
            {
                jumpSound.Play();
                player.velocity = new Vector2(player.velocity.x, jumpVelocity);
            }
            else if (!hasDoubleJumped) {
                hasDoubleJumped = true;
                jumpSound.Play();
                player.velocity = new Vector2(player.velocity.x, doubleJumpVelocity);
            }
        }

        if (Input.inputString == "c" && canDash)
        {
            StartCoroutine(Dash());
        }

        // UpdateAnimationState();
    }

    // private void UpdateAnimationState()
    // {
    //     MovementState state = MovementState.idle;

    //     if (hasDoubleJumped)
    //     {
    //         state = MovementState.doublejumping;
    //     } else if (player.velocity.y > 0.01f)
    //     {
    //         state = MovementState.jumping;
    //     }
    //     else if (player.velocity.y < -0.01f)
    //     {
    //         state = MovementState.falling;
    //     }
    //     else if (dirx != 0f)
    //     {
    //         state = MovementState.running;
    //     }

    //     if (dirx > 0f) 
    //     {
    //         sprite.flipX = false;
    //     }
    //     else if (dirx < 0f) 
    //     {
    //         sprite.flipX = true;
    //     }

    //     anim.SetInteger("state", (int)state);
    // }

    private bool isGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 1f, jumpableGround);
    }

    private bool canWallJump()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 1f, jumpableGround);
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = player.gravityScale;
        player.gravityScale = 0f;

        dashSound.Play();

        float xVel = dirx * dashingPower;
        float xSign = xVel / dashingPower;

        float yVel = diry * dashingPower;
        float ySign = yVel / dashingPower;

        if (dirx != 0f && diry != 0f)
        {
            float val = Mathf.Sqrt((dashingPower*dashingPower)/2);

            xVel = xSign*val;
            yVel = ySign*val;
        }

        player.velocity = new Vector2(xVel, yVel);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        player.gravityScale = originalGravity;
        player.velocity = new Vector2(0, 0);
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
