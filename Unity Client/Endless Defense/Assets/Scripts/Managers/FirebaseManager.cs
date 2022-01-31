using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    [SerializeField] private LeaderboardEntry entryPrefab;
    [SerializeField] private Transform entryContainer;
    [SerializeField] private GameObject leaderboard;
    [SerializeField] private GameObject submitPanel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private GameObject loadingPanel;

    public void StartSubmitting()
    {
        submitPanel.SetActive(true);
        
        //testing
        // string json =
        //     "{\"-Muje2Zxk1nGFowANzB8\":{\"name\":\"Tyler\",\"score\":50},\"-MujqUf30PyNAo0T6LuE\":{\"name\":\"Managers\",\"score\":90},\"-MujrIGGs0CQp0B6Odo4\":{\"name\":\"Managers\",\"score\":90},\"-MujsVRXNX7hfJQoJG9H\":{\"name\":\"Managers\",\"score\":30},\"-MujxgzzyzRyqgldRvr5\":{\"name\":\"tylR\",\"score\":30},\"-MujzR6iuURqIZ8pMsRj\":{\"name\":\"tylR\",\"score\":30}}";
        // var scores = JsonConvert.DeserializeObject<Dictionary<string, Score>>(json);
        // var scoresList = scores.Values.ToList();
        //
        // foreach (Transform child in entryContainer)
        // {
        //     Destroy(child.gameObject);
        // }
        // for (int i = 0; i < scoresList.Count; i++)
        // {
        //     LeaderboardEntry entry = Instantiate(entryPrefab, entryContainer);
        //     entry.rank.text = (i + 1).ToString("N0");
        //     entry.name.text = scoresList[i].name;
        //     entry.score.text = scoresList[i].score.ToString("N0");
        // }
        // leaderboard.SetActive(true);
    }

    public void SubmitName()
    {
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            SendScore(nameInput.text, GameManager.Instance.BuildManager.CurrenciesAmount.Scrap);
            GameManager.Instance.BuildManager.RemoveAllScrap();
            submitPanel.SetActive(false);
            loadingPanel.SetActive(true);
        }
    }
    
    public void SendScore(string name, int score)
    {
        PushScore(name, score);
    }

    [Serializable]
    public class Scores
    {
        public Score[] scores;
    }

    [Serializable]
    public class Score
    {
        public string name;
        public int score;
    }
    private void ScoresReceived(string scoresJson)
    {
        loadingPanel.SetActive(false);

        var scores = JsonConvert.DeserializeObject<List<Score>>(scoresJson);
        
        foreach (Transform child in entryContainer)
        {
            Destroy(child.gameObject);
        }

        int i = 0;
        foreach (var score in scores)
        {
            LeaderboardEntry entry = Instantiate(entryPrefab, entryContainer);
            entry.rank.text = (i + 1).ToString("N0");
            entry.name.text = score.name;
            entry.score.text = score.score.ToString("N0");
            i++;
        }
        leaderboard.SetActive(true);
    }

    public void CloseLeaderboard()
    {
        leaderboard.SetActive(false);
    }

    private void ScoreSent()
    {
        // show leaderboard
        GetScores();
    }

    private void OnError(string error)
    {
        Debug.LogError($"GOT FIREBASE ERROR: {error}");
    }

    [DllImport("__Internal")]
    public static extern void GetScores();
    [DllImport("__Internal")]
    public static extern void PushScore(string name, int score);
}
