using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.UI;

public class TTS : MonoBehaviour
{
    [Header("api 관련")]
    string apiKey = "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";
    string apiUrl = "https://api.openai.com/v1/audio/speech";

    [Header("프롬프트 & 오디오소스")]
    public string prompt;
    AudioSource myAudio;
    public bool soundCheck = true;
    public Image sound_On_Image;
    public Image sound_Off_Image;

    public OpenAI_ChatGPT openai;
    Coroutine ttsRoutine;  // 현재 실행 중인 TTS 코루틴 추적용

    public class TTS_SendData
    {
        public string model;
        public string input;
        public string voice;
        public float speed;
    }

    void Start()
    {
        myAudio = GetComponent<AudioSource>();
    }

    public IEnumerator PlayTTS(string _prompt)
    {
        //Debug.Log(_prompt);

        TTS_SendData myTTS = new TTS_SendData();
        myTTS.model = "tts-1";
        myTTS.input = _prompt;
        if (openai.characterA.activeSelf)
        {
            myTTS.voice = "fable";
        }
        else if (openai.characterB.activeSelf)
        {
            myTTS.voice = "echo";
        }
        else if (openai.characterC.activeSelf)
        {
            myTTS.voice = "shimmer";
        }
        else if (openai.characterD.activeSelf)
        {
            myTTS.voice = "nova";
        }
        myTTS.speed = 1.0f;

        string Json_myTTS = JsonConvert.SerializeObject(myTTS);
        byte[] byte_myTTS = System.Text.Encoding.UTF8.GetBytes(Json_myTTS);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.uploadHandler = new UploadHandlerRaw(byte_myTTS);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            byte[] ReceiveTTSData = request.downloadHandler.data;
            string filePath = Path.Combine(Application.persistentDataPath, "tts_temp.mp3");
            File.WriteAllBytes(filePath, request.downloadHandler.data);

            UnityWebRequest MP3FileRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG);
            yield return MP3FileRequest.SendWebRequest();

            if (MP3FileRequest.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(MP3FileRequest);
                myAudio.clip = clip;
                myAudio.Play();

                yield return new WaitForSeconds(clip.length + 0.5f);
                File.Delete(filePath);
            }

            else
            {
                Debug.Log(MP3FileRequest.error);
            }
        }

        else
        {
            Debug.Log(request.error);
        }
    }

    public void _SoundOK()
    {
        if (soundCheck)
        {
            myAudio.volume = 0;
            soundCheck = false;
            sound_On_Image.gameObject.SetActive(false);
            sound_Off_Image.gameObject.SetActive(true);
        }
        else if (!soundCheck)
        {
            myAudio.volume = 1;
            soundCheck = true;
            sound_On_Image.gameObject.SetActive(true);
            sound_Off_Image.gameObject.SetActive(false);
        }
    }

    public void PlayTTS_Safe(string prompt)
    {
        if (ttsRoutine != null)
        {
            StopCoroutine(ttsRoutine);
            myAudio.Stop(); // 이전 오디오 중지
        }

        ttsRoutine = StartCoroutine(PlayTTS(prompt));
    }

}
