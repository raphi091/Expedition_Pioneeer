using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [Header("Zone")]
    [SerializeField] private BGMTrackName ZoneName;
    [SerializeField] private BGMTrackName ExitZoneName;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.Instance.PlayBGM(ZoneName);

            PlayerControl pc = other.GetComponent<PlayerControl>();
            pc.isVillage = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerControl pc = other.GetComponent<PlayerControl>();
            pc.isVillage = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.Instance.PlayBGM(ExitZoneName);

            PlayerControl pc = other.GetComponent<PlayerControl>();
            pc.isVillage = false;
        }
    }
}
