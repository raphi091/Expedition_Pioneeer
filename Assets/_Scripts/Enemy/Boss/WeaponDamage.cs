using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    public float damage = 50f;

    private Collider weaponCollider;

    void Awake()
    {
        if (!TryGetComponent(out weaponCollider))
            Debug.LogWarning("Weapon ] Collider 없음");

        weaponCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamage damageable = other.GetComponent<IDamage>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);

            weaponCollider.enabled = false;
        }
    }
}
