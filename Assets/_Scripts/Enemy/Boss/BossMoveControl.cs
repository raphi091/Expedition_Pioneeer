using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class BossMoveControl : MonoBehaviour
{
    private BossControl bc;
    private NavMeshAgent agent;
    private Animator animator;

    private float patrolTimer;
    private float patrolWaitTime;
    private float patrolSpeed;
    private float chaseSpeed;
    private Vector3 territoryCenter;
    private float territoryRadius;
    private float patrolWaitTimer;


    public void Initialize(BossControl bossControl)
    {
        bc = bossControl;
        agent = bossControl.Agent;
        animator = bossControl.Animator;

        patrolSpeed = bossControl.patrolSpeed;
        chaseSpeed = bossControl.chaseSpeed;
        territoryRadius = bossControl.territoryRadius;
        patrolWaitTime = bossControl.patrolWaitTime;

        territoryCenter = transform.position;
        patrolWaitTimer = 0f;
    }

    public void Patrol()
    {
        if (agent.isStopped) agent.isStopped = false;
        agent.speed = this.patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolWaitTimer += Time.deltaTime;

            if (patrolWaitTimer > patrolWaitTime)
            {
                SetNewPatrolDestination();
                patrolWaitTimer = 0f;
            }
            else
            {
                int randomChoice = Random.Range(0, 1000);

                if (randomChoice < 10)
                    animator.SetTrigger("DoLOOK");
                else if (randomChoice < 15)
                    animator.SetTrigger("DoROAR");
                else if (randomChoice < 20)
                    animator.SetTrigger("DoPRAY");
            }
        }
        else
        {
            patrolWaitTimer = 0f;
        }
    }

    public void Chase(Transform target)
    {
        if (agent.isStopped) 
            agent.isStopped = false;

        agent.speed = chaseSpeed;
        agent.SetDestination(target.position);
    }

    public void Stop()
    {
        if (agent.path.corners.Length == 0 || !agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    public void Resume()
    {
        if (agent.isStopped)
        {
            agent.isStopped = false;
        }
    }

    private void SetNewPatrolDestination()
    {
        Vector3 randomPoint = territoryCenter + Random.insideUnitSphere * territoryRadius;
        NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, territoryRadius, 1);
        agent.SetDestination(hit.position);
    }

    public void UpdateAnimator()
    {
        if (agent == null || animator == null) return;

        Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        animator.SetFloat("MOVEZ", localVelocity.z);
        animator.SetFloat("MOVEX", localVelocity.x);
    }
}
