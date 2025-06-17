using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    [Header("TimeSet")]
    public float DailyCycleSec = 1800f;

    [Header("Sun")]
    public Light sunLight;

    [Header("Day / Night")]
    public Gradient sunColorGradient;
    public Gradient ambientColorGradient;

    private float timeOfDay;


    private void Start()
    {
        StartCoroutine(DayNightRoutine());
    }

    private IEnumerator DayNightRoutine()
    {
        while (true)
        {
            timeOfDay += Time.deltaTime / DailyCycleSec;
            timeOfDay = Mathf.Repeat(timeOfDay, 1f);

            UpdateSunRotation();
            UpdateLightColors();

            yield return null;
        }
    }

    private void UpdateSunRotation()
    {
        if (sunLight == null) return;

        float sunAngle = timeOfDay * 360f - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0);
    }

    private void UpdateLightColors()
    {
        if (sunLight == null) return;

        sunLight.color = sunColorGradient.Evaluate(timeOfDay);
        RenderSettings.ambientLight = ambientColorGradient.Evaluate(timeOfDay);
    }
}
