using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    // To help with floating point determinisim
    // https://shaderfun.com/2020/10/25/understanding-determinism-part-1-intro-and-floating-points/
    public static float LosePrecision(float a)
    {
        a = a * 65536f;
        a = Mathf.Round(a);
        a = a / 65536f;
        return a;
    }

    public static float RoundToNearestX(float a, float x)
    {
        x = LosePrecision(x);
        float inverse = LosePrecision(1f / x);
        
        a = a * inverse;
        a = Mathf.Round(a);
        a = a / inverse;

        return LosePrecision(a);
    }

    public static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            value = min;
        }

        if (value > max)
        {
            value = max;
        }

        return value;
    }

    public static T GetRandomElementFromList<T>(List<T> list, int randomInt)
    {
        return list[randomInt % list.Count];
    }
}
