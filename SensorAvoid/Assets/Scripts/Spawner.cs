using UnityEngine;

// 일정 간격으로 화면 위에 장애물 생성
public class Spawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float interval = 0.8f;   // 생성 간격 (작을수록 어려움)
    public float rangeX = 8f;       // 생성 x 범위
    float timer;

    void Update()
    {
        if (GameManager.isOver) return;
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            Vector3 pos = new Vector3(Random.Range(-rangeX, rangeX), 6f, 0f);
            Instantiate(obstaclePrefab, pos, Quaternion.identity);
        }
    }
}
