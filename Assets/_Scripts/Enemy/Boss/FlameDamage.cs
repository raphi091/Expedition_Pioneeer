using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameDamage : MonoBehaviour
{
    [Header("데미지 및 속도 설정")]
    public float damage = 40f;
    public float speed = 20f;
    public float lifeTime = 5f;

    void Start()
    {
        GetComponent<Rigidbody>().velocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BossControl_Raizen>() != null) return;
        if (other.GetComponent<FlameDamage>() != null) return;

        IDamage damageable = other.GetComponent<IDamage>();
        if (damageable != null)
        {
            damageable.TakeDamage(this.damage);
        }
    }
}
