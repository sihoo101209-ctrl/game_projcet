using System.IO.Ports;
using System.Threading;
using UnityEngine;

// 아두이노에서 "ax,ay,az" 줄을 받아 기울기(tilt, -1~1)로 바꿔주는 스크립트
public class SerialReceiver : MonoBehaviour
{
    public string portName = "COM3";   // 아두이노 IDE 도구 > 포트에서 확인한 이름
    public int baudRate = 115200;      // mpu_read.ino의 Serial.begin 값과 같게
    public int axisIndex = 1;          // 좌우 기울기 축 (ax=0, ay=1, az=2)
    public static float tilt = 0f;     // 다른 스크립트가 읽는 값

    static SerialReceiver instance;    // 재시작해도 하나만 유지
    SerialPort port;
    Thread thread;                     // 시리얼은 별도 스레드에서 읽음 (게임 끊김 방지)
    volatile bool running;
    volatile float latest;             // 스레드가 쓴 최신 값

    void Awake()
    {
        // Restart로 씬이 다시 로드돼도 포트를 다시 열지 않게 처음 것만 남김
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        try
        {
            port = new SerialPort(portName, baudRate);
            port.ReadTimeout = 200;
            port.Open();
        }
        catch (System.Exception e)
        {
            Debug.LogError("포트 열기 실패 (" + portName + "): " + e.Message + " → 포트 이름 확인, 시리얼 모니터 닫기");
            return;
        }

        running = true;
        thread = new Thread(ReadLoop);
        thread.IsBackground = true;
        thread.Start();
    }

    void ReadLoop()
    {
        while (running)
        {
            try
            {
                string[] p = port.ReadLine().Trim().Split(',');
                if (p.Length < 3) continue;                          // 깨진 줄은 무시
                float raw = float.Parse(p[axisIndex]);
                latest = Mathf.Clamp(raw / 16384f, -1f, 1f);         // 16384 = 1g → -1~1로 변환
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
}
