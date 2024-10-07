using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackManager : MonoBehaviour
{

    private float defaultForce = 300f;
    private float upwardForce = 150f;
    public float movementTime = 0.1f;
    private bool meleeAttack;
    private float meleeAttackCooldown = 0.4f;
    private float meleeAttackTimer = 0;
    private Animator playerAnim;
    private PlayerController player;
    private Rigidbody2D rb;

    public Transform upAttackTransform, sideAttackTransform, downAttackTransform;
    public Vector2 upAttackArea, sideAttackArea, downAttackArea;

    private float damageAmount = 20f;
    private Vector2 direction;
    private bool collided;
    private bool downwardStrike;

    private void Start()
    {
        playerAnim = GetComponent<Animator>();
        player = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();

    }

    private void Update()
    {
        CheckInputs();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void CheckInputs() 
    {
        meleeAttackTimer += Time.deltaTime;
        if (Input.GetButtonDown("Melee Attack") && meleeAttackTimer >= meleeAttackCooldown) 
        {
            meleeAttack = true;
            PerformMeleeAttack();
            meleeAttackTimer = 0;
        }
        else { meleeAttack = false; }
    }

    private void PerformMeleeAttack() 
    {
        // Upward Strike
        if (player.GetYAxis() > 0)
        {
            Debug.Log("Upward Strike");
            playerAnim.SetTrigger("UpwardMelee");
            Hit(upAttackTransform, upAttackArea);
        }

        // Downward Strike
        if (player.GetYAxis() < 0 && !player.IsGrounded())
        {
            Debug.Log("Downward Strike");
            playerAnim.SetTrigger("DownwardMelee");
            Hit(downAttackTransform, downAttackArea);
        }

        // Forward Strike
        if (player.GetYAxis() == 0 || player.GetYAxis() < 0 && player.IsGrounded())
        {
            Debug.Log("Forward Strike");
            playerAnim.SetTrigger("ForwardMelee");
            Hit(sideAttackTransform, sideAttackArea);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(upAttackTransform.position, upAttackArea);
        Gizmos.DrawWireCube(sideAttackTransform.position, sideAttackArea);
        Gizmos.DrawWireCube(downAttackTransform.position, downAttackArea);
    }

    public void Hit(Transform attackTransform, Vector2 attackArea)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(attackTransform.position, attackArea, 0);
        Debug.Log(objectsToHit.Length);
        foreach (Collider2D collider in objectsToHit) 
        {
            if (collider.GetComponent<EnemyHealth>()) 
            {
                HandleCollision(collider.GetComponent<EnemyHealth>());
            }
        }
    }

    private void HandleCollision(EnemyHealth objectHealth)
    {
        if (objectHealth.giveUpwardForce && player.GetYAxis() < 0 && !player.IsGrounded())
        {
            direction = Vector2.up;
            downwardStrike = true;
            collided = true;
        }
        if (player.GetYAxis() > 0 && !player.IsGrounded())
        {
            direction = Vector2.down;
            collided = true;
        }
        if (player.GetYAxis() <= 0 && player.IsGrounded() || player.GetYAxis() == 0)
        {
            if (player.IsFacingRight())
            {
                direction = Vector2.left;
            }
            else
            {
                direction = Vector2.right;
            }
            collided = true;
        }

        objectHealth.TakeDamage(damageAmount);
        StartCoroutine(NoLongerColliding());
    }

    private void HandleMovement()
    {
        if (collided)
        {
            if (downwardStrike)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.AddForce(direction * upwardForce);
            }
            else
            {
                rb.AddForce(direction * defaultForce);
            }
        }
    }

    IEnumerator NoLongerColliding()
    {
        yield return new WaitForSeconds(movementTime);
        collided = false;
        downwardStrike = false;
    }

    IEnumerator StopVerticalForce() 
    {
        yield return new WaitForSeconds(0.01f);
        rb.velocity = new Vector2(rb.velocity.x, 0);
    }

    public float GetDamage()
    {
        return damageAmount;
    }
    public void SetDamage(float amount)
    {
        damageAmount = amount;
    }

    public Vector2 GetDirection() 
    {
        return direction;
    }

}
