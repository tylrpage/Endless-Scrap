using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObjectUIManager : MonoBehaviour
{
    [SerializeField] private Transform worldSpaceCanvases;
    [SerializeField] private MovingObjectUI defaultMovingObjectUI;
    
    private Dictionary<BattleObject, MovingObjectUI> _movingObjectUIs = new Dictionary<BattleObject, MovingObjectUI>();

    /// <summary>
    /// Creates a world space ui for a battle object
    /// </summary>
    /// <param name="caller"></param>
    /// <param name="movingObjectUI">Can be used to override the moving object prefab that is created, if left null the default is used</param>
    /// <param name="offset"></param>
    /// <param name="sizeMult">Used to widen the ui</param>
    /// <returns></returns>
    public MovingObjectUI CreateMovingObject(BattleObject caller, MovingObjectUI movingObjectUI, Vector3 offset, float sizeMult)
    {
        movingObjectUI ??= defaultMovingObjectUI;
        
        if (_movingObjectUIs.TryGetValue(caller, out MovingObjectUI existingMovingObjectUI))
        {
            Destroy(existingMovingObjectUI.gameObject);
        }

        Vector3 position = caller.transform.position + offset;
        MovingObjectUI newMovingObjectUI = Instantiate(movingObjectUI, position, Quaternion.identity, worldSpaceCanvases);
        newMovingObjectUI.SetTarget(caller, offset);
        RectTransform rectTransform = newMovingObjectUI.GetComponent<RectTransform>();
        //newMovingObjectUI.transform.localScale *= sizeMult;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x * sizeMult, rectTransform.sizeDelta.y);
        _movingObjectUIs[caller] = newMovingObjectUI;

        return newMovingObjectUI;
    }
}
