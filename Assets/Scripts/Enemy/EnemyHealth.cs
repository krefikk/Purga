using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private bool damageable = true;
    [SerializeField] private bool moveable = false;
    [SerializeField] private float health = 100f;
    [SerializeField] private float weight = 70f;
    float currentHealth;
    [SerializeField] private float invulnerabilityTime = 0.2f;
    public bool giveUpwardForce = true;
    bool hit;

    [Header("Player")]
    public GameObject player;

    // Components
    private MeleeAttackManager meleeAttackManager;
    private Rigidbody2D rb;

    [Header("Attack Reaction")]
    [SerializeField] private float defaultForce = 300f;
    [SerializeField] private float verticalForce = 150f;

    private void Start()
    {
        currentHealth = health;
        meleeAttackManager = player.GetComponent<MeleeAttackManager>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(float amount) 
    {
        if (damageable && !hit && currentHealth > 0) 
        {
            hit = true;
            currentHealth -= amount;
            if (moveable) 
            {
                Vector2 direction = -1 * meleeAttackManager.GetDirection();
                if (direction.y != 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x, 0);
                    rb.AddForce(direction * verticalForce);
                }
                else { rb.AddForce(direction * defaultForce); }
            }
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                gameObject.SetActive(false);
            }
            else 
            {
                StartCoroutine(TurnOffHit());
            }
        }
    }

    IEnumerator TurnOffHit() 
    {
        yield return new WaitForSeconds(invulnerabilityTime);
        hit = false;
    }

    public float GetWeight() 
    {
        return weight;
    }
}
