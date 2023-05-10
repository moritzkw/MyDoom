using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

enum BotState
{
    STANDING,
    WALKING,
    FOLLOWING,
}

public class Bot : MonoBehaviour
{
    public float horizontalViewAngle = 160f;
    public float verticalViewAngle = 90f;
    public float viewDistance = 35f;
    public Transform playerTransform;
    public Transform gunTip;
    public GameObject flareEffect;
    public float attackDelay = 2f;

    private Animator animator;
    private float attackTime;

    private NavMeshAgent navMeshAgent;

    public int maxHp = 100;

    public UI UI;


    private int hp;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        hp = maxHp;
        attackTime = attackDelay;
    }

    // Update is called once per frame
    void Update()
    {
        navMeshAgent.destination = playerTransform.position;

        NavMeshPath path = new NavMeshPath();
        navMeshAgent.CalculatePath(playerTransform.position, path);

        switch (path.status)
        {
            case NavMeshPathStatus.PathComplete:
                animator.SetBool("isWalking", !PlayerReached());
                if (CanSeePlayer() && hp > 0)
                {
                    PlayerInSight();
                }
                else
                {
                    attackTime = attackDelay;
                }
                break;
            case NavMeshPathStatus.PathPartial:
            case NavMeshPathStatus.PathInvalid:
            default:
                animator.SetBool("isWalking", false);
                break;
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float horizontalAngle = Vector3.Angle(directionToPlayer, transform.forward);
        float verticalAngle = Vector3.Angle(directionToPlayer, transform.up);

        if (horizontalAngle < horizontalViewAngle / 2f
            && verticalAngle > 90f - verticalViewAngle / 2f
            && verticalAngle < 90f + verticalViewAngle / 2f)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, directionToPlayer, out hitInfo, viewDistance))
            {
                if (hitInfo.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool PlayerReached()
    {
        return navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
    }

    private void PlayerInSight()
    {
        bool reached = PlayerReached();
        animator.SetBool("isWalking", !reached);
        animator.SetBool("isStandingAiming", reached);

        attackTime -= Time.deltaTime;

        if (attackTime <= 0)
        {
            Attack();
            attackTime = attackDelay;
        }
    }

    private void Attack()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            Player player = hit.transform.GetComponent<Player>();
            if (player != null)
            {
                player.Hit(10);
            }

            animator.SetTrigger("shoot");
            GameObject flash = Instantiate(flareEffect, gunTip.transform.position, Quaternion.LookRotation(-hit.normal));
            flash.transform.parent = gunTip;
            Destroy(flash, 0.2f);
        }
    }

    public void Hit(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("hit");
        }
    }

    private void Die()
    {
        UI.DecreaseEnemyCount();
        navMeshAgent.enabled = false;
        animator.SetTrigger("die");
        animator.SetBool("isWalking", false);
        Destroy(gameObject, 5f);
    }
}
