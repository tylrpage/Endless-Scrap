using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScrapDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        BuildManager.CurrenciesUpdated += OnCurrenciesUpdated;
    }

    private void OnCurrenciesUpdated(BuildManager.Currencies currencies)
    {
        text.text = currencies.Scrap.ToString("N0");
    }

    private void OnDestroy()
    {
        BuildManager.CurrenciesUpdated -= OnCurrenciesUpdated;
    }
}
