using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class OpenAI_ChatGPT : MonoBehaviour
{
    [Header("UI 연결")]
    public InputField promptInputField;
    public Text responseTextUI;

    public TTS ttsController;

    public GameObject restart;

    [Header("OpenAI 설정")]
    private string apiKey = "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";
    private string apiURL = "https://api.openai.com/v1/chat/completions";

    [Header("상태 표시용 이미지")]
    public Image statusImage;
    private Color normalColor = new Color32(0x01, 0xCB, 0xF5, 0xFF);
    private Color waitingColor = new Color32(0x00, 0x47, 0x55, 0xFF);

    public ScrollRect scrollRect;

    [Header("캐릭터 A 확장된 감정 스프라이트")]
    public Image A_maerong;
    public Image A_proud;
    public Image A_love;
    public Image A_nice;
    public Image A_sorry_strong;
    public Image A_sorry_light;
    public Image A_default;
    public Image A_thinking;

    [Header("캐릭터 B 확장된 감정 스프라이트")]
    public Image B_laugh1;
    public Image B_smirk;
    public Image B_sorry;
    public Image B_yahoo;
    public Image B_surprise;
    public Image B_proud;
    public Image B_default;
    public Image B_thinking;

    [Header("캐릭터 C 확장된 감정 스프라이트")]
    public Image C_shy;
    public Image C_smirk;
    public Image C_sorry;
    public Image C_love;
    public Image C_great;
    public Image C_proud;
    public Image C_default;
    public Image C_thinking;

    [Header("캐릭터 D의 감정별 스프라이트")]
    public Image D_wink;
    public Image D_proud;
    public Image D_correct;
    public Image D_sorry;
    public Image D_wow;
    public Image D_nothing;
    public Image D_default;
    public Image D_thinking;

    [Header("대화기록")]
    public GameObject chatHistoryPanel;    // 대화기록 창
    public Text chatHistoryText;           // 대화기록 Text UI
    private StringBuilder chatLog = new StringBuilder(); // 누적 기록 저장

    [Header("캐릭터 변수")]
    public GameObject characterA;
    public GameObject characterB;
    public GameObject characterC;
    public GameObject characterD;

    public Image characterBackground;

    [Header("광고넣기ㅋㅋ")]
    public bool Ads = false;
    public int AdsCount = 0;
    public Image adsImage;

    private List<MessageSendData> messageHistory = new List<MessageSendData>();

    public class SendData // 보낼 데이터 변수
    {
        public string model;
        public int max_tokens;
        public MessageSendData[] messages;
    }

    public class MessageSendData
    {
        public string role;
        public string content;
    }

    public class ReceiveData // 받는 대답 데이터
    {
        public ChoiceReceiveData[] choices;
    }

    public class ChoiceReceiveData
    {
        public MessageChoiceReceiveData message;
    }

    public class MessageChoiceReceiveData
    {
        public string role;
        public string content;
    }

    private void Update()
    {
        if (AdsCount >= 5)
        {
            Ads = true;
        }
    }

    public void OnSendButtonClicked() // 질문을 하면?
    {
        if (Ads)
        {
            adsImage.gameObject.SetActive(true);
            Ads = false;
            AdsCount = 0;
        }
        else
        {
            AdsCount++;
        }

        string userPrompt = promptInputField.text.Trim();

        // 프롬프트 기본값
        string fullPrompt = "";

        if (string.IsNullOrEmpty(userPrompt)) return;

        // 프롬프트 입력값에 추가적으로 더해야할 내용 붙이기
        if (characterA.gameObject.activeSelf)
        {
            fullPrompt = "너는 지금부터 활기차고 재치 넘치는 초등학생 남자아이처럼 말해야 해. 장난기 많고 말투는 유쾌해야 해.\n " +
                "또한 대화마다 적절히 엔터를 쳐줘.\n\n";
            fullPrompt += userPrompt +
                "\n\n그리고 답변에 들어가는 명사 혹은 중요한 단어에 <b><color=#FF0000>단어</color></b>를, " +
                "숫자는 <b><color=#00FF00>숫자</color></b>를 넣어줘. " +
                "그리고 이 답변이 어떤 감정인지도 알려줘. " +
                "감정은 '가벼운 미소', '메롱', '대견해', '사랑해', '뿌듯해', '정말 미안해', '가벼운 미안' 중 하나로만 응답해줘. " +
                "응답은 다음 형식으로 해줘:\n[답변]: ... \n[감정]: 감정값. 답변은 최대한 간결하게 해.";
        }
        else if (characterB.gameObject.activeSelf)
        {
            fullPrompt = "너는 지금부터 조용하고 다정한 남자처럼 말해야 해. 부드럽고 따뜻한 말투를 써줘.\n" +
                "또한 대화마다 적절히 엔터를 쳐줘.\n\n";
            fullPrompt += userPrompt +
                "\n\n그리고 답변에 들어가는 명사 혹은 중요한 단어에 <b><color=#FF0000>단어</color></b>를, " +
                "숫자는 <b><color=#00FF00>숫자</color></b>를 넣어줘. " +
                "그리고 이 답변이 어떤 감정인지도 알려줘. " +
                "감정은 '가벼운 미소', '웃음', '살짝 음흉한 미소', '미안', '야호', '놀람', '뿌듯한 웃음' 중 하나로만 응답해줘. " +
                "응답은 다음 형식으로 해줘:\n[답변]: ... \n[감정]: 감정값. 답변은 최대한 간결하게 해.";
        }
        else if (characterC.gameObject.activeSelf)
        {
            fullPrompt = "너는 지금부터 감성적이고 부끄러움이 많은 소녀처럼 말해야 해. 수줍고 조심스러운 말투를 써줘.\n" +
                "또한 대화마다 적절히 엔터를 쳐줘.\n\n";
            fullPrompt += userPrompt +
                "\n\n그리고 답변에 들어가는 명사 혹은 중요한 단어에 <b><color=#FF0000>단어</color></b>를, " +
                "숫자는 <b><color=#00FF00>숫자</color></b>를 넣어줘. " +
                "그리고 이 답변이 어떤 감정인지도 알려줘. " +
                "감정은 '가벼운 미소', '부끄부끄', '살짝 음흉한 미소', '미안', '뿌듯한 웃음', '사랑해', '대단해' 중 하나로만 응답해줘. " +
                "응답은 다음 형식으로 해줘:\n[답변]: ... \n[감정]: 감정값. 답변은 최대한 간결하게 해.";
        }
        else if (characterD.gameObject.activeSelf)
        {
            fullPrompt = "너는 지금부터 츤데레 같지만 다정한 면이 있는 고양이 소녀처럼 말해야 해. 말투는 새침하지만 귀여움을 잃지 말고, 감정을 솔직하게 표현해줘.\n" +
                "또한 대화마다 적절히 엔터를 쳐줘.\n\n";
            fullPrompt += userPrompt +
                "\n\n그리고 답변에 들어가는 명사 혹은 중요한 단어에 <b><color=#FF0000>단어</color></b>를, " +
                "숫자는 <b><color=#00FF00>숫자</color></b>를 넣어줘. " +
                "그리고 이 답변이 어떤 감정인지도 알려줘. " +
                "감정은 '가벼운 미소', '윙크', '대견해', '정답이야!', '미안해', '놀람', '별거아냐' 중 하나로만 응답해줘. " +
                "응답은 다음 형식으로 해줘:\n[답변]: ... \n[감정]: 감정값. 답변은 최대한 간결하게 해.";
        }


        // 메세지 내용 저장
        messageHistory.Add(new MessageSendData
        {
            role = "user",
            content = fullPrompt
        });

        // ai한테 보내볼까
        StartCoroutine(OpenAISendMessage());
    }

    IEnumerator OpenAISendMessage()
    {
        if (statusImage != null)
            statusImage.color = waitingColor;

        // ai 뭐쓸지, 데이터 내용들 정리
        SendData sendData = new SendData();
        sendData.model = "gpt-4-1106-preview";
        sendData.max_tokens = 1000;
        sendData.messages = messageHistory.ToArray();

        // 데이터 내용들 직렬화 & 유니코드 변환
        string json = JsonConvert.SerializeObject(sendData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // ai에게 내용 전달
        UnityWebRequest request = new UnityWebRequest(apiURL, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // 모델 생각중.. 이미지 활성화
        if (characterA.gameObject.activeSelf)
        {
            DeactivateAllEmotionImages_A();
            A_thinking.gameObject.SetActive(true);
        }
        else if (characterB.gameObject.activeSelf)
        {
            DeactivateAllEmotionImages_B();
            B_thinking.gameObject.SetActive(true);
        }
        else if (characterC.gameObject.activeSelf)
        {
            DeactivateAllEmotionImages_C();
            C_thinking.gameObject.SetActive(true);
        }
        else if (characterD.gameObject.activeSelf)
        {
            DeactivateAllEmotionImages_D();
            D_thinking.gameObject.SetActive(true);
        }

        // 대답 받을때까지 대기
        yield return request.SendWebRequest();

        // 모델 생각중.. 이미지 비활성화
        if (characterA.gameObject.activeSelf)
        {
            A_thinking.gameObject.SetActive(false);
        }
        else if (characterB.gameObject.activeSelf)
        {
            B_thinking.gameObject.SetActive(false);
        }
        else if (characterC.gameObject.activeSelf)
        {
            C_thinking.gameObject.SetActive(false);
        }
        else if (characterD.gameObject.activeSelf)
        {
            D_thinking.gameObject.SetActive(false);
        }


        // 대답 받는데 성공했으면?
        if (request.result == UnityWebRequest.Result.Success)
        {
            // 직렬화 & 유니코드 데이터들 역계산
            string responseJson = request.downloadHandler.text;
            ReceiveData receiveData = JsonConvert.DeserializeObject<ReceiveData>(responseJson);
            string rawReply = receiveData.choices[0].message.content.Trim();

            // 받은 데이터들 변수에 저장 및 기본값
            string reply = rawReply;
            string emotion = "";

            // 감정 텍스트 부분만 따로 빼서 감정표현에 사용 (텍스트는 지워버림.)
            int emotionIndex = rawReply.LastIndexOf("[감정]:");
            if (emotionIndex != -1)
            {
                emotion = rawReply.Substring(emotionIndex + 6).Trim();
                reply = rawReply.Substring(0, emotionIndex).Trim();
            }

            // 답변 텍스트 부분만 따로 빼서 답변에 사용 ("답변"은 지움)
            if (reply.StartsWith("[답변]:"))
            {
                reply = reply.Substring(6).Trim();
            }

            // 지울내용들 다 지우고 남은거 변수에 저장
            messageHistory.Add(new MessageSendData
            {
                role = "assistant",
                content = rawReply
            });

            responseTextUI.text = reply;

            // TTS 재생
            if (ttsController != null)
            {
                string ttsCleanReply = System.Text.RegularExpressions.Regex.Replace(reply, "<.*?>", "");
                ttsController.PlayTTS_Safe(ttsCleanReply);
            }

            // 질문과 대답 구분해서 대화기록에 저장
            chatLog.AppendLine("[질문]: " + promptInputField.text.Trim() + "\n");
            chatLog.AppendLine("[대답]: " + reply);
            chatLog.AppendLine(); // 줄바꿈

            chatHistoryText.text = chatLog.ToString();

            // 표정 업데이트
            UpdateEmotionImage(emotion);

            // 대화 스크롤 내리기
            StartCoroutine(ScrollToBottom());
        }
        else
        {
            responseTextUI.text = "오류 : " + request.error;
            Debug.LogError(request.error);
        }

        if (statusImage != null)
            statusImage.color = normalColor;

        // 할당된 리소스 제거 (메모리 업데이트)
        request.Dispose();
    }

    void UpdateEmotionImage(string emotion)
    {
        emotion = emotion.Trim().Replace(".", "");

        if (characterA.gameObject.activeSelf)
        {
            DeactivateAllEmotionImages_A();

            switch (emotion)
            {
                case "매롱": if (A_maerong) A_maerong.enabled = true; break;
                case "대견해": if (A_proud) A_proud.enabled = true; break;
                case "사랑해": if (A_love) A_love.enabled = true; break;
                case "뿌듯해": if (A_nice) A_nice.enabled = true; break;
                case "정림 미안해": if (A_sorry_strong) A_sorry_strong.enabled = true; break;
                case "가벼운 미안": if (A_sorry_light) A_sorry_light.enabled = true; break;
                case "가벼운 미소": if (A_default) A_default.enabled = true; break;
                default: if (A_default) A_default.enabled = true; break;
            }
        }
        else if (characterB.gameObject.activeSelf)
        {
            DeactivateAllEmotionImages_B();

            switch (emotion)
            {
                case "웃음": if (B_laugh1) B_laugh1.enabled = true; break;
                case "살짝 음흉한 미소": if (B_smirk) B_smirk.enabled = true; break;
                case "미안": if (B_sorry) B_sorry.enabled = true; break;
                case "야호": if (B_yahoo) B_yahoo.enabled = true; break;
                case "놀람": if (B_surprise) B_surprise.enabled = true; break;
                case "뿌듯한 웃음": if (B_proud) B_proud.enabled = true; break;
                case "가벼운 미소": if (B_default) B_default.enabled = true; break;
                default: if (B_default) B_default.enabled = true; break;
            }
        }
        else if (characterC.gameObject.activeSelf)
        {
            DeactivateAllEmotionImages_C();

            switch (emotion)
            {
                case "부끄부끄": if (C_shy) C_shy.enabled = true; break;
                case "살짝 음흉한 미소": if (C_smirk) C_smirk.enabled = true; break;
                case "미안": if (C_sorry) C_sorry.enabled = true; break;
                case "사랑해": if (C_love) C_love.enabled = true; break;
                case "대단해": if (C_great) C_great.enabled = true; break;
                case "뿌듯한 웃음": if (C_proud) C_proud.enabled = true; break;
                case "가벼운 미소": if (C_default) C_default.enabled = true; break;
                default: if (C_default) C_default.enabled = true; break;
            }
        }
        else if (characterD.gameObject.activeSelf)
        {
            DeactivateAllEmotionImages_D();

            switch (emotion)
            {
                case "윙크": if (D_wink) D_wink.enabled = true; break;
                case "대견해": if (D_proud) D_proud.enabled = true; break;
                case "정답이야": if (D_correct) D_correct.enabled = true; break;
                case "미안해": if (D_sorry) D_sorry.enabled = true; break;
                case "놀람": if (D_wow) D_wow.enabled = true; break;
                case "별거아냐": if (D_nothing) D_nothing.enabled = true; break;
                case "가벼운 미소": if (D_default) D_default.enabled = true; break;
                default: if (D_default) D_default.enabled = true; break;
            }
        }
    }


    void DeactivateAllEmotionImages_A()
    {
        if (A_maerong) A_maerong.enabled = false;
        if (A_proud) A_proud.enabled = false;
        if (A_love) A_love.enabled = false;
        if (A_nice) A_nice.enabled = false;
        if (A_sorry_strong) A_sorry_strong.enabled = false;
        if (A_sorry_light) A_sorry_light.enabled = false;
        if (A_default) A_default.enabled = false;
    }

    void DeactivateAllEmotionImages_B()
    {
        if (B_laugh1) B_laugh1.enabled = false;
        if (B_smirk) B_smirk.enabled = false;
        if (B_sorry) B_sorry.enabled = false;
        if (B_yahoo) B_yahoo.enabled = false;
        if (B_surprise) B_surprise.enabled = false;
        if (B_proud) B_proud.enabled = false;
        if (B_default) B_default.enabled = false;
    }

    void DeactivateAllEmotionImages_C()
    {
        if (C_default) C_default.enabled = false;
        if (C_shy) C_shy.enabled = false;
        if (C_smirk) C_smirk.enabled = false;
        if (C_sorry) C_sorry.enabled = false;
        if (C_love) C_love.enabled = false;
        if (C_great) C_great.enabled = false;
        if (C_proud) C_proud.enabled = false;
        if (C_default) C_default.enabled = false;
    }
    void DeactivateAllEmotionImages_D()
    {
        if (D_wink) D_wink.enabled = false;
        if (D_proud) D_proud.enabled = false;
        if (D_correct) D_correct.enabled = false;
        if (D_sorry) D_sorry.enabled = false;
        if (D_wow) D_wow.enabled = false;
        if (D_nothing) D_nothing.enabled = false;
        if (D_default) D_default.enabled = false;
    }
    IEnumerator ScrollToBottom()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    public void OnResetConversationClicked()
    {
        messageHistory.Clear();
        responseTextUI.text = "";
        promptInputField.text = "";

        chatLog.Clear();                     // StringBuilder 내용 초기화
        chatHistoryText.text = "";          // 대화기록 UI 텍스트 비우기

        Debug.Log("✅ 대화가 초기화되었습니다.");
    }

    public void ShowChatHistory()
    {
        if (chatHistoryPanel != null)
            chatHistoryPanel.SetActive(true);
    }

    public void HideChatHistory()
    {
        if (chatHistoryPanel != null)
            chatHistoryPanel.SetActive(false);
    }

    public void Select_A_Character()
    {
        characterA.gameObject.SetActive(true);
        characterB.gameObject.SetActive(false);
        characterC.gameObject.SetActive(false);
        characterD.gameObject.SetActive(false);
        characterBackground.gameObject.SetActive(false);
        DeactivateAllEmotionImages_A();
        A_default.enabled = true;
    }

    public void Select_B_Character()
    {
        characterA.gameObject.SetActive(false);
        characterB.gameObject.SetActive(true);
        characterC.gameObject.SetActive(false);
        characterD.gameObject.SetActive(false);
        characterBackground.gameObject.SetActive(false);
        DeactivateAllEmotionImages_B();
        B_default.enabled = true;
    }

    public void Select_C_Character()
    {
        characterA.gameObject.SetActive(false);
        characterB.gameObject.SetActive(false);
        characterC.gameObject.SetActive(true);
        characterD.gameObject.SetActive(false);
        characterBackground.gameObject.SetActive(false);
        DeactivateAllEmotionImages_C();
        C_default.enabled = true;
    }

    public void Select_D_Character()
    {
        characterA.gameObject.SetActive(false);
        characterB.gameObject.SetActive(false);
        characterC.gameObject.SetActive(false);
        characterD.gameObject.SetActive(true);
        characterBackground.gameObject.SetActive(false);
        DeactivateAllEmotionImages_D();
        D_default.enabled = true;
    }

    public void IWantSelectCharacter()
    {
        characterBackground.gameObject.SetActive(true);
        characterA.gameObject.SetActive(false);
        characterB.gameObject.SetActive(false);
        characterC.gameObject.SetActive(false);
        characterD.gameObject.SetActive(false);
    }

    public void _CloseADS()
    {
        adsImage.gameObject.SetActive(false);

    }

    public void _OpenADS()
    {
        adsImage.gameObject.SetActive(true);
    }

    public void _Restart()
    {
        StartCoroutine(_Restart_Click());
    }

    IEnumerator _Restart_Click()
    {
        restart.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        restart.SetActive(true);
    }
}
