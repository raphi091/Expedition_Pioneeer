using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;
    public List<ActorProfile> allProfiles;

    private Dictionary<string, ActorProfile> profileDict;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            profileDict = new Dictionary<string, ActorProfile>();
            foreach (var profile in allProfiles)
            {
                profileDict[profile.alias] = profile;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public ActorProfile GetProfile(string id)
    {
        if (profileDict.TryGetValue(id, out ActorProfile profile))
        {
            return profile;
        }
        Debug.LogError("해당 ID의 프로필을 없음: " + id);
        return null;
    }
}
