using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MiniMap : MonoBehaviour
{
    [Header("Color")]
    [SerializeField] private Color targetColor;
    [SerializeField] private float duration;

    private Image image;
    private Color baseColor;


    private void Awake()
    {
        if (!TryGetComponent(out image))
            Debug.LogWarning("MiniMap ] Image 없음");

        baseColor = image.color;
    }

    public void SetTargetColor()
    {
        StartCoroutine(ChangeColor(image, targetColor, duration));
    }

    public void SetBaseColor()
    {
        StartCoroutine(ChangeColor(image, baseColor, duration));
    }


    private IEnumerator ChangeColor(Image imageToChange, Color targetColor, float duration)
    {
        Color startColor = imageToChange.color;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);

            imageToChange.color = Color.Lerp(startColor, targetColor, progress);

            yield return null;
        }

        imageToChange.color = targetColor;
    }
}
