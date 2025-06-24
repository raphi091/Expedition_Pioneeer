using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningDamage : MonoBehaviour
{
    [Header("데미지")]
    public float damage = 40f;
    public float damageTime = 0.2f;
    public float lifeTime = 5f;

    private void Start()
    {
        StartCoroutine(DamageTime_co());

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BossControl_Raizen>() != null) return;

        IDamage damageable = other.GetComponent<IDamage>();
        if (damageable != null)
        {
            damageable.TakeDamage(this.damage);
        }
    }

    private IEnumerator DamageTime_co()
    {
        yield return new WaitForSeconds(damageTime);

        GetComponent<Collider>().enabled = false;
    }
}
