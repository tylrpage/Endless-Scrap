using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GridIndicator gridIndicator;

    public void SetBuilding(Buildable buildable)
    {
        if (buildable == null)
        {
            spriteRenderer.sprite = null;
        }
        
        spriteRenderer.sprite = buildable.Sprite;
        gridIndicator.SetSize(buildable.Size);
        gridIndicator.SetActive(true);
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
