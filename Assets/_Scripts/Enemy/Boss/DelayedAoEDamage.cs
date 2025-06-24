using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedAoEDamage : MonoBehaviour
{
    [Header("데미지 설정")]
    public float damage = 80f;
    public float stanceDamage = 40f;

    [Header("타이밍 설정")]
    public float warningDuration = 1.5f;
    public float activeDuration = 0.2f;

    private SphereCollider damageCollider;
    private List<IDamage> alreadyHitTargets = new List<IDamage>();

    void Awake()
    {
        damageCollider = GetComponent<SphereCollider>();
        damageCollider.enabled = false;
    }

    void Start()
    {
        StartCoroutine(ExplosionSequence_co());
    }

    private IEnumerator ExplosionSequence_co()
    {
        yield return new WaitForSeconds(warningDuration);

        damageCollider.enabled = true;
        alreadyHitTargets.Clear();

        yield return new WaitForSeconds(activeDuration);

        damageCollider.enabled = false;

        Destroy(gameObject, 2f); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BossControl_Raizen>() != null) return;

        IDamage damageable = other.GetComponent<IDamage>();

        if (damageable != null && !alreadyHitTargets.Contains(damageable))
        {
            damageable.TakeDamage(this.damage);
            alreadyHitTargets.Add(damageable);
        }
    }
}
