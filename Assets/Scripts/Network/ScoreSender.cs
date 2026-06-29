using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ScoreSender : MonoBehaviour
{
    private int _time = 0;
    private int _score = 0;
    private int _level = 0;
    public void SendScore(int score)
    {
        _score = score;
        StartCoroutine(SendScoreCoroutine(_score, _level, _time));
    }

    IEnumerator SendScoreCoroutine(int score, int level, int time)
    {
        string json = $"{{\"score\":{score},\"level\":{level},\"time\":{time}}}";
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:8080/scores", json, "application/json"))
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
