using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotoLevel1 : BattleObject
{
    public override void Step()
    {
        MoveInDirection(Vector2.right);
    }
}
