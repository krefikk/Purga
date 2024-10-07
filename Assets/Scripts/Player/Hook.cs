using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class Hook : MonoBehaviour
{
    public bool isAttached = false;
    public bool canPullAttachedObject = false;
    public bool isMovingForward = true;
    public bool isMovingBackward = false;
    public bool isPullingPlayer = false;
    public float moveSpeed = 7f;
    public string obstacleTag = "Obstacle";
    public string enemyTag = "Enemy";
    public Rigidbody2D rb;
    protected Vector3 direction = new Vector3(0, 0, 0); // Unit vector represents the direction of hook
    public float range = 10f;
    public GameObject player;
    public GameObject attachedObject = null;

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void FixedUpdate()
    {
        if (isMovingForward) 
        {
            MoveForward();
            FaceTo(direction);
        }
        if (isMovingBackward) 
        {
            MoveBackward();
            FaceTo(direction);
        }
        if (isAttached) 
        {
            LimitPlayerDistance();
        }
    }

    public void Update()
    {
        if (player == null) 
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    public void MoveForward()
    {
        rb.MovePosition(transform.position + direction * moveSpeed);
    }

    public void MoveBackward()
    {
        rb.MovePosition(transform.position + direction * -1 * moveSpeed);
    }

    public void FaceTo(Vector3 direction) 
    {
        // Calculate the angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply the rotation to the character
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
    }

    public float GetRotationAsDegrees()
    {
        // Get the Z-axis rotation in degrees
        return transform.eulerAngles.z;
    }

    public void SetDirection(Vector3 nd) 
    {
        direction = nd;
    }

    public Vector3 GetDirection()
    {
        return direction;
    }

    private void LimitPlayerDistance()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // If the player moves farther than the range, stop them from moving further away
        if (distanceToPlayer >= range)
        {
            if (isAttached) 
            {
                Vector2 directionToHook = (transform.position - player.transform.position).normalized;
                player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                player.transform.position = transform.position - (Vector3)directionToHook * range;
            }
            if (isMovingForward) 
            {
                isMovingForward = false;
                isMovingBackward = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        isMovingForward = false;
        isAttached = true;

        // If collided with an obstacle
        if (collision.CompareTag(obstacleTag)) 
        {
            if (isMovingForward) 
            {
                isAttached = true;
                canPullAttachedObject = false;
            }
            if (isMovingBackward) 
            {
                Destroy(gameObject);
            }
        }
        // If collided with an enemy
        if (collision.CompareTag(enemyTag) || isMovingForward) 
        {
            EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
            if (enemy != null) 
            {
                isAttached = true;
                if (enemy.GetWeight() < 100) 
                {
                    canPullAttachedObject = true;
                    attachedObject = enemy.gameObject;
                    enemy.transform.SetParent(transform);
                }
                else 
                {
                    canPullAttachedObject = false;
                    attachedObject = enemy.gameObject;
                    transform.SetParent(enemy.transform);
                }
            }
        }
        // If collided with the player while moving backwards
        if (collision.CompareTag("Player"))
        {
            if (isMovingBackward || isAttached) 
            {
                Destroy(gameObject);
            }
        }
    }
}
