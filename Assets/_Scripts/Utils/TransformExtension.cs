using System.Linq;
using UnityEngine;

public static class TransformExtension
{
    // 슬롯을 이름으로 찾는다 , 대소문자 상관없음
    public static Transform FindSlot(this Transform root, params string[] slotnames)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>();
        
        foreach (var slot in slotnames)
        {
            foreach (Transform t in children)
                if(t.name.ToLower().Contains(slot.ToLower()))
                    return t;
        }

        Debug.LogWarning($"못 찾음 : {slotnames.ToList()}");
            return null;
    }
}
