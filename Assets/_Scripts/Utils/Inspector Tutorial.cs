using UnityEngine;
using CustomInspector;

public class InspectorTutorial : MonoBehaviour
{   
    [Title("인스펙터 꾸미기", underlined: true, fontSize = 15, alignment = TextAlignment.Center)]
    [HorizontalLine(color: FixedColor.BabyBlue, thickness: 0.5f,message: "세부 속성"), HideField] public bool _L0;

    public int testint1;
    [Range(0,10)] public int testint2;
    [Range(0.0f,10.0f)] public float testfloat;

    [RichText(true)]
    public string teststring1;

    [Multiline(lines: 4)]
    public string teststring2;

    [TextArea(minLines: 2,maxLines: 10)]
    public string teststring3;

    [Space(15), ReadOnly(DisableStyle.OnlyText)] public string testReadOnly = "ReadOnly 테스트";

    [HorizontalLine(color: FixedColor.DustyBlue, thickness: 0.5f, message: "프리뷰"), HideField] public bool _L1;
    [Preview(Size.big)] public Sprite sprite;
    
    [HorizontalLine(color: FixedColor.DustyBlue, thickness: 0.5f, message: "버튼"), HideField] public bool _L2;
    [Space(20), Button("Method1", size = Size.big), HideField] public bool _b0;
    void Method1()
    {
        Debug.Log("Method Test");
    }

    [Button("Method2", size = Size.big), HideField] public bool _b1;
    void Method2()
    {

    }

    [Button("Method3",true)] public int inputNumber;
    void Method3(int n)
    {
        Debug.Log($"입력한 숫자 : {n}");
    }

    [Space(20), HideField] public bool _b3;
    [HorizontalLine(color: FixedColor.Blue, thickness: 0.5f), HideField] public bool _L3;
}
