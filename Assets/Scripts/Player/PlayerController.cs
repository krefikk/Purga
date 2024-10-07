using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    Animator anim;
    private float xAxis;
    private float yAxis;
    [Header("Movement")]
    float walkSpeed = 7f;
    float jumpForce = 10f;
    bool isFacingRight = true;
    private bool jumping = false;
    private bool falling = false;
    [Header("ADvanced Movement")]
    private float jumpBufferTime = 60;
    private float coyoteTime = 0.1f;
    private float jumpBufferCounter = 0;
    private float coyoteTimeCounter = 0;
    [Header("Ground Check")]
    public Transform groundCheckPoint;
    float groundCheckY = 0.2f;
    float groundCheckX = 0.5f;
    public LayerMask groundLayer;
    [Header("Dash")]
    public bool canDash = true; // Flag that controls dash skill is enabled or not
    bool dashLoaded = true; // Checks if cooldown for dash is finished
    bool dashing = false; // Checks if player is currently dashing
    float dashTime = 0.3f;
    float dashSpeed = 21f;
    float dashCooldown = 0.9f;
    TrailRenderer dashTrail;
    [Header("Double Jump")]
    public bool canDoubleJump = true; // Flag that controls dash skill is enabled or not
    bool doubleJumpUsed = false; // Checks if player double jumped and touched the ground
    float doubleJumpForce = 10;
    [Header("Wall Sliding & Jumping")]
    // Wall Slide
    public Transform wallCheck;
    bool holdingWall = false;
    float wallSlideSpeed = 2f;
    // Wall Jump
    public bool canWallJump = true;
    bool isWallJumping = false;
    float wallJumpingDirection;
    float wallJumpingTime = 0.2f;
    float wallJumpingCounter;
    float wallJumpingDuration = 0.2f;
    Vector2 wallJumpPower = new Vector2(7, 10);
    [Header("Edge Hold & Climb")]
    public bool canEdgeHoldAndClimb = true;
    public Transform edgeCheck;
    bool holdingEdge = false;
    bool isEdgeClimbing = false;
    [Header("Hook")]
    public GameObject hookPrefab;
    public Hook currentHook = null;
    public bool isHookThrown = false;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        dashTrail = GetComponent<TrailRenderer>();
        wallJumpPower = new Vector2(7, jumpForce);
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        AnimationCheck();
        if (dashing) { return; }
        Move();
        Look();
        Flip();
        UpdateJumpVariables();
        Jump();
        //DoubleJump();
        Dash();
        WallSlide();
        WallJump();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
    }

    private void Move()
    {
        if (!isWallJumping)
        {
            rb.velocity = new Vector3(walkSpeed * xAxis, rb.velocity.y, 0);
        } 
    }

    private void Look() 
    {
        if (!isWallJumping)
        {
            yAxis = Input.GetAxis("Vertical");
        }
    }

    void AnimationCheck() 
    {
        // Walking
        if (IsGrounded() && Mathf.Abs(rb.velocity.x) > 0) 
        { 
            anim.SetBool("Walking", true); 
        }
        else 
        { 
            anim.SetBool("Walking", false); 
        }
        
        // Dashing
        if (dashing) 
        {
            anim.SetTrigger("Dashing");
        }

        // Jumping
        if (rb.velocity.y > 0)
        {
            jumping = true;
        }
        else 
        {
            jumping = false;
        }
        anim.SetBool("Jumping", jumping);

        // Falling
        if (rb.velocity.y <= 0 && !IsGrounded() && !holdingWall)
        {
            falling = true;
        }
        else
        {
            falling = false;
        }
        anim.SetBool("Falling", falling);

        // Wall Slide
        anim.SetBool("HoldingWall", holdingWall);
    }

    public bool IsGrounded()
    {
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, groundLayer)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, groundLayer)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, groundLayer))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(wallCheck.position, new Vector2(0.1f, 0.1f));
        Gizmos.DrawWireCube(edgeCheck.position, new Vector2(0.1f, 0.1f));
    }

    public bool IsTouchingWall() 
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, groundLayer);
    }

    void WallSlide() 
    {
        if (!IsGrounded() && IsTouchingWall() && xAxis != 0 && !holdingEdge) 
        {
            holdingWall = true;
            rb.velocity = new Vector2(0, -wallSlideSpeed);
        }
        else { holdingWall = false; }
    }

    void WallJump() 
    {
        if (canWallJump) 
        {
            if (holdingWall)
            {
                isWallJumping = false;
                wallJumpingDirection = -1 * transform.localScale.x;
                wallJumpingCounter = wallJumpingTime;
                CancelInvoke(nameof(StopWallJumping));
            }
            else
            {
                wallJumpingCounter -= Time.deltaTime;
            }

            if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
            {
                isWallJumping = true;
                rb.velocity = new Vector2(wallJumpingDirection * wallJumpPower.x, wallJumpPower.y);
                wallJumpingCounter = 0f;

                if (transform.localScale.x != wallJumpingDirection) 
                {
                    isFacingRight = !isFacingRight;
                    Vector3 localScale = transform.localScale;
                    localScale.x *= -1;
                    transform.localScale = localScale;
                }

                Invoke(nameof(StopWallJumping), wallJumpingDuration);
            }
        }
    }

    void StopWallJumping() 
    {
        isWallJumping = false;
    }

    void Flip() 
    {
        if (!isWallJumping) 
        {
            if ((isFacingRight && xAxis < 0) || (!isFacingRight && xAxis > 0))
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1;
                transform.localScale = localScale;
            }
        }
    }

    void Jump()
    {
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            rb.velocity += new Vector2(rb.velocity.x, jumpForce);
        }
        else if (canDoubleJump && Input.GetButtonDown("Jump") && !IsGrounded() && !doubleJumpUsed && !holdingWall) 
        {
            jumpBufferCounter = 0;
            anim.SetTrigger("DoubleJump");
            doubleJumpUsed = true;
            rb.velocity += new Vector2(rb.velocity.x, doubleJumpForce);
        }
    }

    void UpdateJumpVariables() 
    {
        // Jump Input Buffer
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else 
        {
            jumpBufferCounter--;
        }

        // Coyote Time
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else 
        {
            if (coyoteTimeCounter <= 0)
            {
                coyoteTimeCounter = 0;
            }
            else 
            {
                coyoteTimeCounter -= Time.deltaTime;
            }
        }

        // Double Jump
        if (IsGrounded() && doubleJumpUsed) 
        {
            doubleJumpUsed = false;
        }
    }

    public void Dash() 
    {
        if (Input.GetButtonDown("Dash"))
        {
            StartCoroutine(DashCoroutine());
        }
    }

    IEnumerator DashCoroutine() 
    {
        if (dashLoaded) 
        {
            dashing = true;
            dashLoaded = false;
            dashTrail.emitting = true;
            if (xAxis == 0)
            {
                if (isFacingRight) { rb.velocity = new Vector2(dashSpeed, 0); }
                else { rb.velocity = new Vector2(dashSpeed * -1, 0); }
            }
            else { rb.velocity = new Vector2(dashSpeed * xAxis, 0); }
            rb.gravityScale = 0;
            yield return new WaitForSeconds(dashTime);
            rb.gravityScale = 1;
            rb.velocity = new Vector2(0, 0);
            dashTrail.emitting = false;
            dashing = false;
            yield return new WaitForSeconds(dashCooldown);
            dashLoaded = true;
        }   
    }

    public void ThrowHook() 
    {
        // Adjust the flags
        isHookThrown = true;

        // Get the mouse position in world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Calculate the direction vector from the character to the mouse
        Vector3 direction = mousePosition - transform.position;

        // Make sure the z-axis remains the same
        direction.z = 0f;

        // Instantiate the hook
        GameObject hookObject = Instantiate(hookPrefab, transform.position, Quaternion.identity);
        Hook hook = hookObject.GetComponent<Hook>();
        hook.SetDirection(direction);
        currentHook = hook;
    }

    public void PullHook() 
    {
        if (isHookThrown) 
        {
            currentHook.isAttached = false;
            if (currentHook.canPullAttachedObject)
            {
                currentHook.isMovingBackward = true;
            }
            else 
            {
                rb.MovePosition(currentHook.transform.position);
            }
        }
    }

public float GetXAxis() 
    {
        return xAxis;
    }
    public float GetYAxis() 
    {
        return yAxis;
    }

    public bool IsFacingRight() 
    {
        return isFacingRight;
    }

    public bool IsJumping() 
    {
        return jumping;
    }

    public bool IsFalling() 
    {
        return falling;
    }
}
