using System.Collections;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class Wisper : MonoBehaviour
{
    [Header("API 관련")]
    string apiKey = "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";
    string apiUrl = "https://api.openai.com/v1/audio/transcriptions";

    [Header("음성 파일 경로")]
    public string filePath;

    public class WhisperResponse
    {
        public string text;
    }

    private void Start()
    {
        StartCoroutine(PlayerWisper(filePath));
    }

    IEnumerator PlayerWisper(string _filePath)
    {
        if (!File.Exists(_filePath))
        {
            Debug.Log("오디오 파일 없음 " + _filePath);
            yield break;
        }

        byte[] audioData = File.ReadAllBytes(_filePath);

        if (audioData == null || audioData.Length == 0)
        {
            Debug.Log("오디오 파일에 문제있음");
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, Path.GetFileName(filePath), "audio/x-m4a");
        form.AddField("model", "whisper-1");
        form.AddField("language", "ko");

        UnityWebRequest request;
        request = UnityWebRequest.Post(apiUrl, form);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string WisperResponseText = request.downloadHandler.text;
            WhisperResponse wisperResponse = JsonConvert.DeserializeObject<WhisperResponse>(WisperResponseText);
            Debug.Log(wisperResponse.text);
        }

        else
        {
            Debug.Log(request.error);
        }
    }
}
