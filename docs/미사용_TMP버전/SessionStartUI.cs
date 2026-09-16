using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이틀 화면. 기기 번호와 참가자 번호를 입력받고 게임 씬으로 넘어간다.
/// 이 씬에 GameLogger 오브젝트도 같이 있어야 한다.
///
/// 기기 번호는 한 번 입력하면 그 실행 내내 유지된다 (PlayerPrefs).
/// </summary>
public class SessionStartUI : MonoBehaviour
{
    [Header("입력 필드")]
    public TMP_InputField deviceField;
    public TMP_InputField participantField;

    [Header("경고 문구 표시용 (없어도 됨)")]
    public TMP_Text warningText;

    [Header("게임 씬 이름")]
    public string gameSceneName = "Game";

    const string DeviceKey = "device_id";

    void Start()
    {
        if (warningText != null) warningText.text = "";

        if (deviceField != null)
        {
            deviceField.contentType = TMP_InputField.ContentType.IntegerNumber;
            int saved = PlayerPrefs.GetInt(DeviceKey, 0);
            if (saved > 0) deviceField.text = saved.ToString();
        }

        if (participantField != null)
        {
            participantField.contentType = TMP_InputField.ContentType.IntegerNumber;
            participantField.text = "";
            participantField.Select();
        }
    }

    /// <summary>시작 버튼 OnClick 에 연결.</summary>
    public void OnStartButton()
    {
        if (!int.TryParse(deviceField.text, out int device) || device <= 0)
        {
            Warn("기기 번호를 입력해줘");
            return;
        }

        if (!int.TryParse(participantField.text, out int participant) || participant <= 0)
        {
            Warn("참가자 번호를 입력해줘");
            return;
        }

        if (GameLogger.Instance == null)
        {
            Debug.LogError("[로그] GameLogger 가 씬에 없음");
            return;
        }

        PlayerPrefs.SetInt(DeviceKey, device);
        PlayerPrefs.Save();

        GameLogger.Instance.BeginSession(device, participant);
        SceneManager.LoadScene(gameSceneName);
    }

    void Warn(string message)
    {
        if (warningText != null) warningText.text = message;
        Debug.LogWarning($"[로그] {message}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            OnStartButton();
    }
}
