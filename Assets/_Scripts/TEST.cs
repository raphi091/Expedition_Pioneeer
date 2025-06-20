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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            pc.SetWeapon(id.allWeapons[0]);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            pc.SetWeapon(id.allWeapons[4]);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            pc.SetWeapon(id.allWeapons[8]);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            pc.SetWeapon(id.allWeapons[12]);
    }
}
