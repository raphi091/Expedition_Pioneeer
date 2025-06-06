using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance = null;

    public Texture2D defaultCursor;
    // public Texture2D hoverCursor; // 특정 상황용 커서
    public Vector2 hotspot = Vector2.zero;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetCursorDefault();
    }

    public void SetCursorDefault()
    {
        Cursor.SetCursor(defaultCursor, hotspot, CursorMode.Auto);
    }

    //특정 커서로 변경
    //public void SetCursorHover()
    //{
    //    Cursor.SetCursor(hoverCursor, hotspot, CursorMode.Auto);
    //}
}