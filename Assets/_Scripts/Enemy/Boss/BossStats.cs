using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomInspector;


public class BossStats : MonoBehaviour, IDamage
{
    [Header("데이터")]
    [Foldout] public BossData data;

    private float currentHealth;
    private float currentStance;

    public event Action OnDamaged;
    public event Action<int> OnStanceBroken;
    public event Action OnPhaseTransition;
    public event Action OnDeath;

    public float HealthPercentage => currentHealth / data.maxHealth;

    private int currentPhase = 1;


    void Awake()
    {
        currentHealth = data.maxHealth;
        currentStance = 0f;
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentStance += damage;

        OnDamaged?.Invoke();

        if (currentPhase == 1 && HealthPercentage <= 0.5f)
        {
            currentPhase = 2;
            OnPhaseTransition?.Invoke();
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath?.Invoke();
        }
    }

    public bool CheckAndBreakStance()
    {
        if (currentStance >= data.maxStance)
        {
            currentStance = 0;
            OnStanceBroken?.Invoke(0);
        }
        return false;
    }

    public void TriggerPhaseTransition()
    {
        OnPhaseTransition?.Invoke();
    }
}
