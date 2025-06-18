using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerAttackControl : MonoBehaviour
{
    private PlayerControl playerControl;
    private Animator animator;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRadius = 0.7f;
    public LayerMask enemyLayerMask;

    public void Initialize(PlayerControl pc)
    {
        this.playerControl = pc;
        this.animator = pc.animator;
    }

    public void RequestPrimaryAttack()
    {
        animator.SetTrigger(AnimatorHashSet.ATTACK);
    }

    public void AnimationEvent_PerformHitCheck()
    {
        if (playerControl == null) return;

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRadius, enemyLayerMask);
        foreach (Collider enemy in hitEnemies)
        {
            // enemy.GetComponent<EnemyHealth>()?.TakeDamage(playerControl.State.damage);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
