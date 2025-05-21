using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.UI;

public class Dall_E : MonoBehaviour
{
    // api 관련
    string apiKey = "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";
    string apiUrl = "https://api.openai.com/v1/images/generations";

    [Header("뒷배경 변경 관련")]
    public bool change_background = true;
    public Image back_ground;

    [Header("대화입력")]
    public string[] Dall_E_prompts;

    [Header("출력 이미지")]
    public RawImage Dall_E_image;
    private Color waitingColor = new Color32(0x7B, 0x7B, 0x7B, 0xFF);
    private Color normalColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    public Text changing_back;

    [Header("직접 입력 모드")]
    public Image imput_back; // 인풋 백그라운드
    public InputField userPromptInput;
    private bool isUserInputMode = false;  // 직접 입력 모드 여부

    public class SendImageMakingData // ai 내용
    {
        public string model;
        public string prompt;
        public int n;
        public string size;
        public string response_format;
    }

    public class ReceiveImageMakingData // 받는 데이터
    {
        public ImageURL[] data;
    }

    public class ImageURL
    {
        public string url;
    }

    private void Start()
    {
        // 시작되면 랜덤으로 프롬프트 한번 돌리기
        if (Dall_E_prompts != null && Dall_E_prompts.Length > 0)
        {
            changing_back.gameObject.SetActive(true);
            int index = Random.Range(0, Dall_E_prompts.Length);
            change_background = false;
            
            StartCoroutine(Dall_E_SendMessage(Dall_E_prompts[index]));
        }
        else
        {
            Debug.LogWarning("⚠️ Dall_E 프롬프트 배열이 비어 있습니다.");
        }
    }

    IEnumerator Dall_E_SendMessage(string _prompt) // ai에게 메세지 보내기
    {
        // 각종 ui 띄우기 / 끄기
        if (Dall_E_image != null)
            Dall_E_image.color = waitingColor;
        changing_back.gameObject.SetActive(true);

        // 정보 입력
        SendImageMakingData myMD = new SendImageMakingData();
        myMD.model = "dall-e-2";
        string fullPrompt = _prompt + ", no text, no words, no letters, no captions";
        myMD.prompt = fullPrompt;
        myMD.n = 1;
        myMD.size = "256x256";
        myMD.response_format = "url";

        // 정보를 직렬화 & 유니코드로 변환
        string JSON_myMD = JsonConvert.SerializeObject(myMD);
        byte[] byte_myMD = Encoding.UTF8.GetBytes(JSON_myMD);

        // 변환한거 ai에게 전달
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.uploadHandler = new UploadHandlerRaw(byte_myMD);
        request.downloadHandler = new DownloadHandlerBuffer();

        // ai가 답변 줄때까지 대기...
        yield return request.SendWebRequest();

        // ai에게 답변이 올바르게 왔다면
        if (request.result == UnityWebRequest.Result.Success)
        {
            // 유니코드 & 직렬화를 역재생 및 해석
            string responseText = request.downloadHandler.text;
            ReceiveImageMakingData myRI = JsonConvert.DeserializeObject<ReceiveImageMakingData>(responseText);
            //Debug.Log(myRI.data[0].url);

            UnityWebRequest request_image = UnityWebRequestTexture.GetTexture(myRI.data[0].url);

            // 각종 UI 및 시스템에 재배치
            yield return request_image.SendWebRequest();

            if (request_image.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)request_image.downloadHandler).texture;
                Dall_E_image.texture = texture;
            }

            else
            {
                Debug.Log(request_image.error);
            }
        }

        else
        {
            Debug.Log(request.error);
        }

        request.Dispose();

        if (Dall_E_image != null)
            Dall_E_image.color = normalColor;

        change_background = true;
        changing_back.gameObject.SetActive(false);
    }

    public void _OnChangeBackgroundButtonClicked()
    {
        if (!change_background) return;

        change_background = false;
        back_ground.gameObject.SetActive(false);

        // 프롬프트 기본값
        string selectedPrompt = "";

        if (isUserInputMode) // 프롬프트 모드가 켜져있으면 실행
        {
            selectedPrompt = userPromptInput.text.Trim();

            if (string.IsNullOrEmpty(selectedPrompt))
            {
                Debug.LogWarning("사용자가 프롬프트를 입력하지 않았습니다.");
                return;
            }
            imput_back.gameObject.SetActive(false);
            back_ground.gameObject.SetActive(false);
        }

        else if (Dall_E_prompts != null && Dall_E_prompts.Length > 0) // 프롬프트 모드가 꺼져있으면 랜덤돌림
        {
            int index = Random.Range(0, Dall_E_prompts.Length);
            selectedPrompt = Dall_E_prompts[index];
            back_ground.gameObject.SetActive(false);
        }

        else
        {
            Debug.LogWarning("⚠️ Dall_E 프롬프트 배열이 비어 있습니다.");
            return;
        }

        StartCoroutine(Dall_E_SendMessage(selectedPrompt));

        
    }

    public void _ChangeBackground_Image()
    {
        if (change_background)
        {
            back_ground.gameObject.SetActive(true);
        }
    }

    public void _OnUserPromptModeButtonClicked()
    {
        isUserInputMode = true;
        imput_back.gameObject.SetActive(true);
    }

    public void _OffUserPromptModeButtonClicked()
    {
        isUserInputMode = false;
        imput_back.gameObject.SetActive(false);
    }

    public void _Off_background()
    {
        back_ground.gameObject.SetActive(false);
    }
}
