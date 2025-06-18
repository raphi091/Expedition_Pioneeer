using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image healthBar;
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

            UpdateHealthBar(pc.State.health, pc.Profile.health);
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

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar == null) return;

        healthBar.fillAmount = currentHealth / maxHealth;

        if (currentHealth / maxHealth < 0.2f)
        {
            healthBar.color = lowHealthColor;
        }
        else
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
