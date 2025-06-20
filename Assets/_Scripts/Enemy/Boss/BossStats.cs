using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStats : MonoBehaviour
{
    [Header("Profile")]
    public ActorProfile profile;

    [Header("Stats")]
    public int currentHp;
    public bool isDead;

    private BossControl bc;

    private void Awake()
    {
        if (!TryGetComponent(out bc))
            Debug.LogWarning("BossStats ] BossControl 없음");
    }

    public void Initialize()
    {
        if (profile == null)
        {
            Debug.LogError("BossStats에 ActorProfile이 할당되지 않았습니다!", gameObject);
            return;
        }

        currentHp = profile.health;
        isDead = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            currentHp = 0;
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnTargetBossDefeated(profile.model);
        }

        bc.OnDeath();
    }
}
