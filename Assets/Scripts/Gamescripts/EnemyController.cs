﻿using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {

    private GameManager gameManager;
    private PlayerController player;

    private GameObject destructionPoint;

    public int orbsRewarded;
    public int baseHealth;
    public bool isAerialType;
    public float moveSpeed;

    private bool isAlive;
    private bool isAttacking;

    public float flyingSwing;

    private int currentHealth;
    private bool isKnockedBack;

    private float knockbackTime;
    public float knockbackLength;
    public float knockbackAmplifier;

    private Rigidbody2D myRigidbody;
    private Animator myAnimator;
    private Collider2D myCollider;

    public PhysicsMaterial2D deathMaterial;
    public GameObject explosion;

    private Transform startTransform;

	// Use this for initialization
	void Start () {
        gameManager = FindObjectOfType<GameManager>().GetComponent<GameManager>();
        currentHealth = baseHealth;

        destructionPoint = GameObject.Find("DestructionPoint");

        player = FindObjectOfType<PlayerController>();
    
        isKnockedBack = false;
        isAlive = true;
        isAttacking = false;

        myRigidbody = this.GetComponent<Rigidbody2D>();
        myAnimator = this.GetComponentInChildren<Animator>();
        myCollider = this.GetComponentInChildren<Collider2D>();

        if (isAerialType) {
            myRigidbody.isKinematic = true;
        }

        startTransform = transform;
	}

    public void RestoreVariables() {
        currentHealth = baseHealth;

        if (startTransform != null) {
            transform.position = startTransform.position;
            transform.rotation = new Quaternion();
        }

        isKnockedBack = false;
        isAlive = true;
        isAttacking = false;

        if (isAerialType) {
            GetComponent<Rigidbody2D>().isKinematic = true;
        } else {
            GetComponent<Rigidbody2D>().isKinematic = false;
        }

        gameObject.layer = LayerMask.NameToLayer("Enemies");
        transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Enemies");

        if (myCollider != null) {
            myCollider.sharedMaterial = null;
            myCollider.isTrigger = false;
        }

        //myRigidbody.velocity = new Vector2(0, 0);
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        if (transform.position.x < destructionPoint.transform.position.x) {
            ObjectPooler.instance.AddToPool(gameObject);
        }

        if (isAlive) {

            /* check if knockback is done */
            knockbackTime -= Time.deltaTime;

            if(isAerialType) {
                if(player.transform.position.x + 5 >= transform.position.x && transform.position.x > player.transform.position.x && !isAttacking && !isKnockedBack) {
                    isAttacking = true;
                }
            } else {
                if(player.transform.position.x + 4 >= transform.position.x && transform.position.x > player.transform.position.x && !isAttacking && !isKnockedBack) {
                    isAttacking = true;
                }
            }

            
            if (!isKnockedBack && !isAttacking) {
                myRigidbody.velocity = new Vector2(-moveSpeed, myRigidbody.velocity.y);
            } else if (isKnockedBack && knockbackTime <= 0) {      
                isKnockedBack = false;

                if(isAerialType) {
                    myRigidbody.isKinematic = true;
                }
            } else if (isAttacking && myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !myAnimator.IsInTransition(0)) {
                transform.position = myAnimator.gameObject.transform.position;
                isAttacking = false;
            }

            if (currentHealth <= 0) {

                /* set the enemy state to dead */
                isAlive = false;

                /* prepare enemy for becoming a projectile */
                gameObject.layer = LayerMask.NameToLayer("DeadEnemies");
                transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("DeadEnemies");

                myCollider.sharedMaterial = deathMaterial;

                /* start using the enemy as projectile and destroy it afterwards */
                StartCoroutine(DestroyEnemyRoutine(1));

                /* create explosion on defeated enemy location */
                GameObject newExplosion = ObjectPooler.instance.GetObjectByName(explosion.name, true);
                newExplosion.transform.position = transform.position;
                newExplosion.gameObject.GetComponent<ExplosionController>().StartExplosion(isAerialType);

                gameManager.addOrbs(Random.Range(Mathf.FloorToInt(orbsRewarded * 0.50f),orbsRewarded));
                gameManager.addDefeatedEnemy();

                /* check if we need to bounce the enemy up (ground) or down (aerial) */
                if (isAerialType) {
                    this.myRigidbody.velocity = new Vector2(12, -3);      
                } else {
                    this.myRigidbody.velocity = new Vector2(12, 6);
                }
            }

            /* prevent strange collisions moving the rigidbody at unrealistic speeds */
            if(myRigidbody.velocity.x > moveSpeed * 15) {
                myRigidbody.velocity = new Vector2(moveSpeed * 15, myRigidbody.velocity.y);
            }


            /* set enemy animator states */
            myAnimator.SetBool("Alive", isAlive);
            myAnimator.SetFloat("Speed", Mathf.Abs(myRigidbody.velocity.x));
            myAnimator.SetBool("Knockback", isKnockedBack);
            myAnimator.SetBool("Attacking", isAttacking);

        } else {

            /* make sure the enemy is not going the wrong way */
            if(myRigidbody.velocity.x < 0) {
                myRigidbody.AddForce(new Vector2(0,0));
                myRigidbody.velocity = new Vector2(Mathf.Abs(myRigidbody.velocity.x), myRigidbody.velocity.y);
            }

            transform.Rotate (0,0,360 * Time.deltaTime);

        }
	}

    public bool isEnemyAlive() {
        return isAlive;
    }

    public IEnumerator destroyRemains() {
        /* allow it to ignore collisions from now on */
        myCollider.isTrigger = true;

        /* put it in front of the forground so it doesn't dissapear behind it */
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 3);

        yield return new WaitForSeconds(3);
        ObjectPooler.instance.AddToPool(gameObject);

    }

    /* if enemy doesn't get hit anything within *time* - destroy it anyways */
    IEnumerator DestroyEnemyRoutine(float time) {
        yield return new WaitForSeconds(time);
        StartCoroutine(destroyRemains());
    }

    public void TakeDamage(int damage) {
        transform.position = myCollider.gameObject.transform.position;
        myRigidbody.isKinematic = false;

        currentHealth -= damage;
        isKnockedBack = true;
        knockbackTime = knockbackLength;
        isAttacking = false;

        if (isAerialType) {
            myAnimator.gameObject.transform.position = transform.position;
        }

        myRigidbody.velocity = new Vector2(10,1.5f * knockbackAmplifier);


    }

    public bool IsAerial() {
        return isAerialType;
    }

    public void HitByTimebend() {
        currentHealth = 0;
        if (isAerialType) {
            myAnimator.gameObject.transform.position = transform.position;
        }
    }
    
    
}
