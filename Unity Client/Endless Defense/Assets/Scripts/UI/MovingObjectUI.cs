using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovingObjectUI : MonoBehaviour
{
    [Tooltip("UI Slider to display Player's Health")]
    public Slider PlayerHealthSlider;
    public RectTransform PlayerHealthDamageFill;
    public float DamageTweenDuration;
    public GameObject HealthPanel;
    
    private BattleObject _target;
    private Coroutine _damageTweenCR;
    private Vector3 _offsetFromTarget;
    
    private void LateUpdate()
    {
        if (_target != null)
        {
            transform.position = _target.transform.position + _offsetFromTarget;
        }
    }

    public void SetTarget(BattleObject target, Vector3 offset)
    {
        _offsetFromTarget = offset;
        _target = target;
    }

    public void SetHealth(float health, float maxHealth)
    {
        // Hide health panel unless we are missing some health
        HealthPanel.SetActive(health < maxHealth);
        
        Vector2 oldAnchorMax = PlayerHealthSlider.fillRect.anchorMax;
        float oldValue = PlayerHealthSlider.value;
        PlayerHealthSlider.value = health / maxHealth;

        if (health == maxHealth || !gameObject.activeInHierarchy)
        {
            PlayerHealthDamageFill.anchorMax = PlayerHealthSlider.fillRect.anchorMax;
        }
        else if (PlayerHealthSlider.value < oldValue)
        {
            if (_damageTweenCR != null)
                StopCoroutine(_damageTweenCR);
            _damageTweenCR = StartCoroutine(DamageTweenCoroutine(oldAnchorMax));
        }
    }

    // PlayerHealthDamageFill tweens from PlayerHealthSlider's previous size to its current size
    private IEnumerator DamageTweenCoroutine(Vector2 oldAnchorMax)
    {
        Vector2 newAnchorMax = PlayerHealthSlider.fillRect.anchorMax;
        PlayerHealthDamageFill.anchorMax = oldAnchorMax;
        yield return new WaitForSeconds(0.2f);

        float elapsedTime = 0f;
        while (elapsedTime < DamageTweenDuration)
        {
            PlayerHealthDamageFill.anchorMax = Vector2.Lerp(oldAnchorMax, newAnchorMax, elapsedTime / DamageTweenDuration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }
        PlayerHealthDamageFill.anchorMax = newAnchorMax;
        _damageTweenCR = null;
    }
}
