using System;
using UnityEngine;

/// <summary>
/// Resources/map_A.json · map_B.json 을 역직렬화하는 데이터 클래스.
/// JsonUtility 기준이므로 필드 이름이 JSON 키와 정확히 일치해야 한다.
/// </summary>
[Serializable]
public class MapData
{
    public string version;
    public SizeData roomSize;
    public Vec2Data spacing;
    public RoomData[] rooms;
    public DoorData[] doors;

    /// <summary>Resources/map_{version}.json 로드. 실패 시 null.</summary>
    public static MapData Load(string version)
    {
        var ta = Resources.Load<TextAsset>("map_" + version);
        if (ta == null)
        {
            Debug.LogError($"[맵] Resources/map_{version}.json 을 찾을 수 없음");
            return null;
        }

        var map = JsonUtility.FromJson<MapData>(ta.text);
        if (map == null || map.rooms == null || map.rooms.Length == 0)
        {
            Debug.LogError($"[맵] map_{version}.json 파싱 실패");
            return null;
        }
        return map;
    }

    public RoomData GetRoom(int id)
    {
        foreach (var r in rooms)
            if (r.id == id) return r;
        return null;
    }
}

[Serializable]
public class SizeData { public float w; public float h; }

[Serializable]
public class Vec2Data { public float x; public float y; }

[Serializable]
public class RoomData
{
    public int id;
    public int[] grid;          // [x, y]
    public string type;         // tutorial / start / normal / boss / detour
    public bool isDeadEnd;
    public bool lockDoors;
    public EnemyCounts enemies;
    public string boss;         // null / "mini" / "final"
    public string reward;       // null / "heal" / "weapon"
    public string[] floorText;  // 시작방 바닥 안내 문구

    public int GridX => grid != null && grid.Length > 0 ? grid[0] : 0;
    public int GridY => grid != null && grid.Length > 1 ? grid[1] : 0;

    public Vector2 Center(Vec2Data sp) => new Vector2(GridX * sp.x, GridY * sp.y);

    public bool HasBoss => !string.IsNullOrEmpty(boss);
    public int TotalEnemies =>
        (enemies != null ? enemies.melee + enemies.ranged + enemies.tank : 0) + (HasBoss ? 1 : 0);
}

[Serializable]
public class EnemyCounts { public int melee; public int ranged; public int tank; }

[Serializable]
public class DoorData { public int from; public int to; public bool oneWay; }
