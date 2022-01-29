using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class RandomManager : MonoBehaviour
{
    private const int SEED = 1337;
    private Random _random;

    private void Awake()
    {
        _random = new Random(SEED);
    }

    public int NextInt()
    {
        return _random.Next();
    }
}
