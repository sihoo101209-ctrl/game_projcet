using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ESC 메뉴 안의 맵 화면. 표시 규칙이 엄격하다:
///   - 클리어한 방 = 채워진 사각형 + 방 번호
///   - 현재 위치 = 강조 색 + 표식
///   - 클리어한 방과 문으로 연결된 미탐색 방 = 윤곽 + ? (번호 숨김)
///   - 그 너머 = 아예 그리지 않음
///   - 문은 방향 없는 선으로만 — 일방통행 방향 표시 금지 (되돌림 복도가 함정으로 기능해야 함)
/// </summary>
public class MapScreen : MonoBehaviour
{
    const float CellW = 64f, CellH = 44f;
    const float PitchX = 88f, PitchY = 68f;

    static readonly Color ClearedFill = new Color(0.4f, 0.42f, 0.5f);
    static readonly Color CurrentFill = new Color(0.95f, 0.75f, 0.2f);
    static readonly Color OutlineColor = new Color(0.6f, 0.6f, 0.65f, 0.8f);
    static readonly Color DoorLine = new Color(0.55f, 0.55f, 0.6f);

    public void Rebuild()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        var map = MapBuilder.CurrentMap;
        if (map == null) return;

        int current = RoomController.CurrentRoomId;
        var known = new HashSet<int>(RoomController.Cleared) { current };

        // 알려진 방과 문으로 연결된 미탐색 방 (문 방향은 따지지 않는다 — 방향 정보 은닉)
        var frontier = new HashSet<int>();
        foreach (var d in map.doors)
        {
            if (known.Contains(d.from) && !known.Contains(d.to)) frontier.Add(d.to);
            if (known.Contains(d.to) && !known.Contains(d.from)) frontier.Add(d.from);
        }

        var drawn = new HashSet<int>(known);
        drawn.UnionWith(frontier);

        // 그려지는 방들의 중심을 컨텐츠 중앙에 맞추기
        Vector2 sum = Vector2.zero;
        int count = 0;
        foreach (var r in map.rooms)
        {
            if (!drawn.Contains(r.id)) continue;
            sum += new Vector2(r.GridX * PitchX, r.GridY * PitchY);
            count++;
        }
        Vector2 offset = count > 0 ? -sum / count : Vector2.zero;

        Vector2 CellPos(RoomData r) => new Vector2(r.GridX * PitchX, r.GridY * PitchY) + offset;

        // 문 (방 사각형보다 먼저 = 뒤에 깔림)
        foreach (var d in map.doors)
        {
            if (!drawn.Contains(d.from) || !drawn.Contains(d.to)) continue;
            var a = map.GetRoom(d.from);
            var b = map.GetRoom(d.to);
            if (a == null || b == null) continue;
            Vector2 mid = (CellPos(a) + CellPos(b)) * 0.5f;
            bool horizontal = a.GridY == b.GridY;
            var line = GameAssets.NewPanel("Door", transform, DoorLine);
            GameAssets.Place(line.rectTransform, new Vector2(0.5f, 0.5f), mid,
                horizontal ? new Vector2(PitchX - CellW + 8f, 6f) : new Vector2(6f, PitchY - CellH + 8f));
        }

        // 방
        foreach (var r in map.rooms)
        {
            if (!drawn.Contains(r.id)) continue;
            Vector2 pos = CellPos(r);

            if (known.Contains(r.id))
            {
                bool isCurrent = r.id == current;
                var cell = GameAssets.NewPanel("Room_" + r.id, transform, isCurrent ? CurrentFill : ClearedFill);
                GameAssets.Place(cell.rectTransform, new Vector2(0.5f, 0.5f), pos, new Vector2(CellW, CellH));
                var num = GameAssets.NewText("Num", cell.transform, r.id.ToString(), 20,
                    isCurrent ? Color.black : Color.white);
                GameAssets.Place(num.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(CellW, CellH));
                if (isCurrent)
                {
                    var marker = GameAssets.NewText("Here", cell.transform, "●", 12, Color.black);
                    GameAssets.Place(marker.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(20f, 14f));
                }
            }
            else
            {
                // 미탐색: 윤곽 + ? (번호 숨김)
                DrawOutline(pos);
                var q = GameAssets.NewText("Q", transform, "?", 22, OutlineColor);
                GameAssets.Place(q.rectTransform, new Vector2(0.5f, 0.5f), pos, new Vector2(CellW, CellH));
            }
        }
    }

    void DrawOutline(Vector2 pos)
    {
        const float t = 2f;
        Vector2[] centers =
        {
            pos + new Vector2(0f,  CellH * 0.5f),
            pos + new Vector2(0f, -CellH * 0.5f),
            pos + new Vector2(-CellW * 0.5f, 0f),
            pos + new Vector2( CellW * 0.5f, 0f),
        };
        Vector2[] sizes =
        {
            new Vector2(CellW, t), new Vector2(CellW, t),
            new Vector2(t, CellH), new Vector2(t, CellH),
        };
        for (int i = 0; i < 4; i++)
        {
            var line = GameAssets.NewPanel("Outline", transform, OutlineColor);
            GameAssets.Place(line.rectTransform, new Vector2(0.5f, 0.5f), centers[i], sizes[i]);
        }
    }
}
