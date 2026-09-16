using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 런타임 생성 리소스 모음. 프리팹·스프라이트·폰트 없이도 게임이 돌아가게 한다.
/// (프리팹을 만들어 MapBuilder 에 꽂으면 그걸 우선 사용)
/// </summary>
public static class GameAssets
{
    static Sprite _white;
    public static Sprite WhiteSprite
    {
        get
        {
            if (_white == null)
            {
                var tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                _white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }
            return _white;
        }
    }

    static Font _font;
    /// <summary>한글 지원 폰트. OS 폰트(맑은 고딕 등) 우선, 실패 시 내장 폰트.</summary>
    public static Font UIFont
    {
        get
        {
            if (_font == null)
            {
                try
                {
                    _font = Font.CreateDynamicFontFromOSFont(
                        new[] { "Malgun Gothic", "맑은 고딕", "Apple SD Gothic Neo", "Noto Sans CJK KR", "NanumGothic" }, 32);
                }
                catch { /* 폰트 없으면 아래 내장 폰트로 */ }
                if (_font == null)
                {
                    // 내장 폰트에는 한글 글리프가 없다 → 글자가 네모로 보이면 이 경고를 확인할 것
                    Debug.LogError("[폰트] 한글 OS 폰트(맑은 고딕 등)를 찾지 못해 내장 폰트로 대체 — 한글이 네모로 보일 수 있음");
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
            }
            return _font;
        }
    }

    /// <summary>씬에 카메라가 없으면 만든다 (씬 파일을 최소로 유지하기 위해).</summary>
    public static Camera EnsureCamera()
    {
        var cam = Camera.main;
        if (cam != null) return cam;
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        cam = go.AddComponent<Camera>();
        go.AddComponent<AudioListener>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.07f);
        go.transform.position = new Vector3(0f, 0f, -10f);
        return cam;
    }

    // ────────── 월드 스프라이트 ──────────

    /// <summary>1x1 흰 스프라이트를 스케일로 늘려 사각형을 만든다.</summary>
    public static SpriteRenderer NewQuad(string name, Transform parent, Vector2 localPos, Vector2 size, Color color, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = WhiteSprite;
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        return sr;
    }

    // ────────── uGUI ──────────

    public static RectTransform NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    public static Image NewPanel(string name, Transform parent, Color color)
    {
        var rt = NewUI(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        return img;
    }

    public static Text NewText(string name, Transform parent, string content, int fontSize, Color color,
                               TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        var rt = NewUI(name, parent);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = UIFont;
        t.text = content;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    public static Button NewButton(string name, Transform parent, string label, int fontSize, UnityAction onClick,
                                   Vector2 size)
    {
        var img = NewPanel(name, parent, new Color(0.22f, 0.22f, 0.28f, 1f));
        var rt = img.rectTransform;
        rt.sizeDelta = size;
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        if (onClick != null) btn.onClick.AddListener(onClick);
        var txt = NewText("Label", rt, label, fontSize, Color.white);
        var trt = txt.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        txt.raycastTarget = false;
        return btn;
    }

    /// <summary>앵커·위치·크기 한 번에.</summary>
    public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 anchoredPos, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return rt;
    }
}
