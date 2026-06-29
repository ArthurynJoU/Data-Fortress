using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ScoreSender : MonoBehaviour
{
    private int _score = 0;
    private int _level = 0;
    public void SendScore(int score, int level)
    {
        _score = score;
        _level = level;
        StartCoroutine(SendScoreCoroutine(_score, _level));
    }

    IEnumerator SendScoreCoroutine(int score, int level)
    {
        string json = $"{{\"score\":{score},\"level\":{level}}}";
        using (UnityWebRequest www = UnityWebRequest.Post("http://44.204.9.41:8080/scores", json, "application/json"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Score have sended!");
            }
        }
    }
}
