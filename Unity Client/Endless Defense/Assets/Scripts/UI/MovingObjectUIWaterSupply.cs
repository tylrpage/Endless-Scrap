using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovingObjectUIWaterSupply : MovingObjectUI
{
    [Tooltip("UI Slider to display water")]
    public Slider PlayerWaterSlider;
    public RectTransform PlayerWaterDamageFill;
    public GameObject WaterPanel;
    
    private Coroutine _damageTweenCRWater;

    public void SetWater(float water, float maxWater)
    {
        // Hide water panel unless we are missing some water
        WaterPanel.SetActive(water < maxWater);
        
        Vector2 oldAnchorMax = PlayerWaterSlider.fillRect.anchorMax;
        float oldValue = PlayerWaterSlider.value;
        PlayerWaterSlider.value = water / maxWater;

        if (water == maxWater || !gameObject.activeInHierarchy)
        {
            PlayerWaterDamageFill.anchorMax = PlayerWaterSlider.fillRect.anchorMax;
        }
        else if (PlayerWaterSlider.value < oldValue)
        {
            if (_damageTweenCRWater != null)
                StopCoroutine(_damageTweenCRWater);
            _damageTweenCRWater = StartCoroutine(DamageTweenCoroutine(oldAnchorMax));
        }
    }

    // PlayerWaterDamageFill tweens from PlayerWaterSlider's previous size to its current size
    private IEnumerator DamageTweenCoroutine(Vector2 oldAnchorMax)
    {
        Vector2 newAnchorMax = PlayerWaterSlider.fillRect.anchorMax;
        PlayerWaterDamageFill.anchorMax = oldAnchorMax;
        yield return new WaitForSeconds(0.2f);

        float elapsedTime = 0f;
        while (elapsedTime < DamageTweenDuration)
        {
            PlayerWaterDamageFill.anchorMax = Vector2.Lerp(oldAnchorMax, newAnchorMax, elapsedTime / DamageTweenDuration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }
        PlayerWaterDamageFill.anchorMax = newAnchorMax;
        _damageTweenCRWater = null;
    }
}
