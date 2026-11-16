#if !(NET_STANDARD || NET_STANDARD_2_0 || NET_STANDARD_2_1)
#define SERIAL_OK                          // .NET Framework일 때만 System.IO.Ports 사용 가능
#endif

#if SERIAL_OK
using System.IO.Ports;
using System.Threading;
#endif
using UnityEngine;

// 아두이노에서 "ax,ay,az" 줄을 받아 기울기(tilt, -1~1)로 바꿔주는 스크립트
public class SerialReceiver : MonoBehaviour
{
    public string portName = "auto";   // "auto"면 자동 선택. 안 되면 "COM3"처럼 직접 입력
    public int baudRate = 115200;      // mpu_read.ino의 Serial.begin 값과 같게
    public int axisIndex = 1;          // 좌우 기울기 축 (ax=0, ay=1, az=2)
    public float sensitivity = 1.5f;   // 클수록 조금만 기울여도 빠르게 움직임
    public static float tilt = 0f;     // 다른 스크립트가 읽는 값

    static SerialReceiver instance;    // 재시작해도 하나만 유지

#if SERIAL_OK
    SerialPort port;
    Thread thread;                     // 시리얼은 별도 스레드에서 읽음 (게임 끊김 방지)
    volatile bool running;
    volatile float latest;             // 스레드가 쓴 최신 값
#endif

    void Awake()
    {
        // Restart로 씬이 다시 로드돼도 포트를 다시 열지 않게 처음 것만 남김
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

#if SERIAL_OK
    void Start()
    {
        string name = PickPort();
        if (name == null) { Debug.LogError("시리얼 포트가 없어. 아두이노 USB 연결 확인"); return; }

        try
        {
            port = new SerialPort(name, baudRate);
            port.ReadTimeout = 200;
            port.Open();
            Debug.Log("시리얼 연결됨: " + name);
        }
        catch (System.Exception e)
        {
            Debug.LogError("포트 열기 실패 (" + name + "): " + e.Message + " → 포트 이름 확인, 시리얼 모니터 닫기");
            return;
        }

        running = true;
        thread = new Thread(ReadLoop);
        thread.IsBackground = true;
        thread.Start();
    }

    string PickPort()
    {
        if (portName != "auto") return portName;
        string[] names = SerialPort.GetPortNames();
        if (names.Length == 0) return null;

        foreach (string n in names)                        // 맥: usb 들어간 포트 우선
            if (n.ToLower().Contains("usb")) return n;

        System.Array.Sort(names, (a, b) =>                 // 윈도우: 번호 제일 큰 COM (보통 아두이노)
            a.Length != b.Length ? a.Length - b.Length : string.CompareOrdinal(a, b));
        return names[names.Length - 1];
    }

    void ReadLoop()
    {
        float offset = 0f;             // 시작 자세를 0으로 맞추는 보정값
        int count = 0;

        while (running)
        {
            try
            {
                string[] p = port.ReadLine().Trim().Split(',');
                if (p.Length < 3) continue;                          // 깨진 줄은 무시
                float raw = float.Parse(p[axisIndex]) / 16384f;      // 16384 = 1g

                if (count < 20) { offset += raw / 20f; count++; continue; }   // 처음 20개 평균 = 기준
                latest = Mathf.Clamp((raw - offset) * sensitivity, -1f, 1f);
            }
            catch { }                                                // 타임아웃·파싱 오류 무시
        }
    }

    void Update()
    {
        tilt = latest;                 // 메인 스레드에서 공용 값 갱신
    }

    void OnDestroy()
    {
        if (instance != this) return;
        running = false;
        if (port != null && port.IsOpen) port.Close();   // 포트 닫아야 다음 실행 때 다시 열림
        instance = null;
    }
#else
    void Start()
    {
        Debug.LogWarning("Api Compatibility Level이 .NET Framework가 아니라 센서 꺼짐. 메뉴 SensorAvoid > 게임 씬 자동 생성 실행");
    }
#endif
}
