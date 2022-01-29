using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer gridRenderer;
    [SerializeField] private Color successColor;
    [SerializeField] private Color errorColor;

    public void SetSize(Vector2 size)
    {
        gridRenderer.drawMode = SpriteDrawMode.Tiled;
        gridRenderer.size = size;
    }

    public void SetActive(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void SetSuccess(bool success)
    {
        gridRenderer.color = success ? successColor : errorColor;
    }
}
