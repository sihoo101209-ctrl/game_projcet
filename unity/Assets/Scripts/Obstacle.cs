using UnityEngine;

// 아래로 떨어지는 장애물. 플레이어에 닿으면 게임 오버
public class Obstacle : MonoBehaviour
{
    public float speed = 5f;    // 낙하 속도 (난이도)

    void Update()
    {
        if (GameManager.isOver) return;

        transform.Translate(Vector3.down * speed * Time.deltaTime);
        if (transform.position.y < -7f) Destroy(gameObject);   // 화면 아래로 나가면 삭제
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            GameManager.instance.GameOver();
    }
}
