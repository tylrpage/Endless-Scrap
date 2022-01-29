using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    [SerializeField] private float cameraSizeMin;
    [SerializeField] private float cameraSizeMax;
    
    private Vector2 _previousMousePosition;
    private Camera _camera;
    private float _sizeTarget;

    private void Awake()
    {
        if (Camera.main != null)
        {
            _camera = Camera.main;
            _sizeTarget = _camera.orthographicSize;
        }
    }

    private void LateUpdate()
    {
        Vector2 mouseToWorldPoint = _camera.ScreenToWorldPoint(Input.mousePosition);
        
        if (_camera != null)
        {
            // Drag with MMB or shift + left click
            if (IsDragging())
            {
                Vector2 difference = mouseToWorldPoint - _previousMousePosition;
                _camera.transform.position -= (Vector3)difference;
                // Recalc world point since the camera moved
                mouseToWorldPoint = _camera.ScreenToWorldPoint(Input.mousePosition);
            }

            // Adjust size target per mouse wheel click
            _sizeTarget -= Input.mouseScrollDelta.y;
            _sizeTarget = Mathf.Clamp(_sizeTarget, cameraSizeMin, cameraSizeMax);
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _sizeTarget, Time.deltaTime * 5f);
        }

        _previousMousePosition = mouseToWorldPoint;
    }

    public static bool IsDragging()
    {
        return Input.GetMouseButton(2) || (Input.GetMouseButton(0) && GetShift());
    }

    private static bool GetShift()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }
    
    public float Map(float from, float to, float from2, float to2, float value){
        if(value <= from2){
            return from;
        }else if(value >= to2){
            return to;
        }else{
            return (to - from) * ((value - from2) / (to2 - from2)) + from;
        }
    }
}
