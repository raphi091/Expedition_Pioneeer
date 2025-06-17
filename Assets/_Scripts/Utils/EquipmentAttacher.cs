using UnityEngine;
using System.Collections.Generic;

public class EquipmentAttacher : MonoBehaviour
{
    // 1. 기준이 될 캐릭터의 몸 SkinnedMeshRenderer (보통 몸통)
    public SkinnedMeshRenderer referenceBodyPart;

    // 2. 장착할 장비 파츠들의 SkinnedMeshRenderer 리스트
    public List<SkinnedMeshRenderer> equipmentParts;

    [ContextMenu("Attach All Equipment Now")]
    public void AttachAll()
    {
        if (referenceBodyPart == null || equipmentParts == null || equipmentParts.Count == 0)
        {
            Debug.LogError("기준 몸 파츠와 장비 파츠 리스트를 모두 할당해주세요!");
            return;
        }

        // 기준이 되는 캐릭터의 뼈대 정보를 가져옴
        Transform[] characterBones = referenceBodyPart.bones;
        Transform characterRootBone = referenceBodyPart.rootBone;

        // 모든 장비 파츠에 대해 반복 작업
        foreach (SkinnedMeshRenderer equipmentPart in equipmentParts)
        {
            if (equipmentPart != null)
            {
                // 장비 파츠의 뼈대 정보를 캐릭터의 뼈대 정보로 교체
                equipmentPart.bones = characterBones;
                equipmentPart.rootBone = characterRootBone;
            }
        }

        Debug.Log(equipmentParts.Count + "개의 장비 파츠가 캐릭터 뼈대에 성공적으로 연결되었습니다!");
    }
}