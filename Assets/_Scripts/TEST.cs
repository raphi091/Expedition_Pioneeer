using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour
{
    private PlayerControl pc;
    private ItemDatabase id;

    private void Awake()
    {
        pc = FindObjectOfType<PlayerControl>();

        if (!TryGetComponent(out id))
            Debug.LogWarning("1");
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(3f);

        pc.SetWeapon(id.allWeapons[4]);
    }
}
