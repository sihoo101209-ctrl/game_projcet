using System.Collections.Generic;
using System.IO;
using UnityEditor;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.Build;
#endif
using UnityEditor.SceneManagement;
using UnityEngine;

// 메뉴 SensorAvoid > 게임 씬 자동 생성 : 유니티 세팅(B 단계)을 한 번에 해줌
[InitializeOnLoad]
public static class SensorAvoidSetup
{
    // 스크립트 넣자마자 Api Compatibility Level을 .NET Framework로 자동 변경
    static SensorAvoidSetup()
    {
        EditorApplication.delayCall += FixApiLevel;
    }

    static void FixApiLevel()
    {
#if UNITY_2021_2_OR_NEWER
        var target = NamedBuildTarget.Standalone;
        var level = ApiCompatibilityLevel.NET_Unity_4_8;   // 메뉴에 ".NET Framework"로 보이는 값
#else
        var target = BuildTargetGroup.Standalone;
        var level = ApiCompatibilityLevel.NET_4_6;         // 구버전 이름 ".NET 4.x"
#endif
        if (PlayerSettings.GetApiCompatibilityLevel(target) == level) return;
        PlayerSettings.SetApiCompatibilityLevel(target, level);
        Debug.Log("[SensorAvoid] Api Compatibility Level → .NET Framework 변경함");
    }

    [MenuItem("SensorAvoid/게임 씬 자동 생성")]
    static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        FixApiLevel();

        Directory.CreateDirectory("Assets/Sprites");
        Directory.CreateDirectory("Assets/Prefabs");
        Directory.CreateDirectory("Assets/Scenes");
        AssetDatabase.Refresh();

        Sprite square = MakeSquareSprite();
        GameObject obstaclePrefab = MakeObstaclePrefab(square);

        // 지금 열린 씬에 만들기 (템플릿의 카메라·조명 그대로 사용). 다시 실행하면 새로 만듦
        foreach (string n in new[] { "Player", "Serial", "GameManager" })
        {
            GameObject old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }

        // Player
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0f, -4f, 0f);
        var psr = player.AddComponent<SpriteRenderer>();
        psr.sprite = square;
        psr.color = new Color(0.3f, 0.8f, 1f);
        player.AddComponent<BoxCollider2D>().size = Vector2.one;
        var prb = player.AddComponent<Rigidbody2D>();
        prb.bodyType = RigidbodyType2D.Dynamic;       // Kinematic끼리는 트리거가 안 잡힐 수 있어서 Dynamic
        prb.gravityScale = 0f;                        // 중력 없음
        prb.constraints = RigidbodyConstraints2D.FreezeRotation;
        prb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        player.AddComponent<PlayerMove>();

        // Serial
        new GameObject("Serial").AddComponent<SerialReceiver>();

        // GameManager
        var gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();
        gm.AddComponent<Spawner>().obstaclePrefab = obstaclePrefab;

        // 씬 저장 + Build Settings 등록 (Restart용)
        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        if (string.IsNullOrEmpty(scene.path)) EditorSceneManager.SaveScene(scene, "Assets/Scenes/Game.unity");
        else EditorSceneManager.SaveScene(scene);

        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        list.RemoveAll(s => s.path == scene.path);
        list.Insert(0, new EditorBuildSettingsScene(scene.path, true));
        EditorBuildSettings.scenes = list.ToArray();

        Debug.Log("[SensorAvoid] 씬 생성 완료. Play 누르면 됨");
    }

    // 1x1 크기 흰 사각형 스프라이트 생성
    static Sprite MakeSquareSprite()
    {
        string path = "Assets/Sprites/Square.png";
        if (!File.Exists(path))
        {
            var tex = new Texture2D(4, 4);
            var px = new Color[16];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
        }

        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        if (imp.textureType != TextureImporterType.Sprite || imp.spritePixelsPerUnit != 4f)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 4f;              // 4픽셀 = 1유닛
            imp.filterMode = FilterMode.Point;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // 빨간 장애물 프리팹 생성
    static GameObject MakeObstaclePrefab(Sprite square)
    {
        var go = new GameObject("Obstacle");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = square;
        sr.color = Color.red;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        col.isTrigger = true;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        go.AddComponent<Obstacle>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Obstacle.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }
}
