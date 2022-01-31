using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingIndicator : MonoBehaviour
{
    [SerializeField] private GameObject buildingToCopy;

    private List<(SpriteRenderer, GridIndicator)> _buildings;

    private void Awake()
    {
        _buildings = new List<(SpriteRenderer, GridIndicator)>();
        _buildings.Add((buildingToCopy.GetComponent<SpriteRenderer>(), buildingToCopy.GetComponentInChildren<GridIndicator>()));
    }

    public void SetPoints(Buildable buildable, Vector2[] points, bool success)
    {
        if (points.Length > _buildings.Count)
        {
            for (int i = 0; i < points.Length - _buildings.Count; i++)
            {
                GameObject newBuilding = Instantiate(buildingToCopy, transform);
                _buildings.Add((newBuilding.GetComponent<SpriteRenderer>(), newBuilding.GetComponentInChildren<GridIndicator>()));
            }
        }

        for (int i = 0; i < points.Length; i++)
        {
            _buildings[i].Item1.gameObject.SetActive(true);
            _buildings[i].Item1.transform.position = new Vector3(points[i].x, points[i].y);
            _buildings[i].Item1.sprite = buildable.Sprite;
            _buildings[i].Item2.SetSize(buildable.Size);
            _buildings[i].Item2.SetSuccess(success);
            _buildings[i].Item2.SetActive(true);
        }

        for (int i = points.Length; i < _buildings.Count; i++)
        {
            _buildings[i].Item1.gameObject.SetActive(false);
            _buildings[i].Item2.SetActive(false);
        }
    }

    public void Clear()
    {
        if (_buildings == null)
        {
            return;
        }
        
        foreach (var building in _buildings)
        {
            building.Item1.gameObject.SetActive(false);
        }
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
