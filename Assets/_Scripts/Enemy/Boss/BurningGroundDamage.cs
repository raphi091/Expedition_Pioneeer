using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningGroundDamage : MonoBehaviour
{
    [Header("장판 능력치")]
    public float damagePerTick = 7f;
    public float tickInterval = 0.7f;
    public float duration = 7.0f;

    private List<IDamage> targetsInside = new List<IDamage>();


    void Start()
    {
        StartCoroutine(DamageTicker_co());
        Destroy(gameObject, duration);
    }

    private IEnumerator DamageTicker_co()
    {
        while (true)
        {
            yield return new WaitForSeconds(tickInterval);

            for (int i = targetsInside.Count - 1; i >= 0; i--)
            {
                if (targetsInside[i] != null)
                {
                    targetsInside[i].TakeDamage(damagePerTick);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BossControl_Raizen>() != null) return;

        IDamage damageable = other.GetComponent<IDamage>();

        if (damageable != null && !targetsInside.Contains(damageable))
        {
            targetsInside.Add(damageable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<BossControl_Raizen>() != null) return;

        IDamage damageable = other.GetComponent<IDamage>();

        if (damageable != null && targetsInside.Contains(damageable))
        {
            targetsInside.Remove(damageable);
        }
    }
}
