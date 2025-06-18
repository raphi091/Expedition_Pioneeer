using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image healthBar;
    public Image healthBarRecoverable;
    public Image staminaBar;

    [Header("Colors")]
    public Color lowHealthColor = Color.red;
    private Color normalHealthColor;

    private PlayerControl pc;

    void Start()
    {
        pc = FindObjectOfType<PlayerControl>();
        if (pc != null)
        {
            pc.OnHealthChanged += UpdateHealthBar;
            pc.OnStaminaChanged += UpdateStaminaBar;

            if (healthBar != null)
            {
                normalHealthColor = healthBar.color;
            }

            UpdateHealthBar(pc.CurrentHealth, pc.RecoverableHealth, pc.MaxHealth);
            UpdateStaminaBar(pc.CurrentStamina, pc.MaxStamina);
        }
        else
        {
            Debug.LogError("PlayerUIController: PlayerControl을 찾을 수 없습니다!");
        }
    }

    private void OnDestroy()
    {
        if (pc != null)
        {
            pc.OnHealthChanged -= UpdateHealthBar;
            pc.OnStaminaChanged -= UpdateStaminaBar;
        }
    }

    private void UpdateHealthBar(float current, float recoverable, float max)
    {
        if (max <= 0) return;

        if (healthBar != null)
        {
            healthBar.fillAmount = current / max;
        }

        if (healthBarRecoverable != null)
        {
            healthBarRecoverable.fillAmount = recoverable / max;
        }

        if (healthBar != null && current / max < 0.15f)
        {
            healthBar.color = lowHealthColor;
        }
        else if (healthBar != null)
        {
            healthBar.color = normalHealthColor;
        }
    }

    private void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaBar == null) return;

        staminaBar.fillAmount = currentStamina / maxStamina;
    }
}
