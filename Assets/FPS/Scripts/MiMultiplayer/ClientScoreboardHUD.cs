using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD de pantalla con dos paneles:
///  - Cabecera arriba a la derecha (tiempo, FPS).
///  - Marcador (kills - deaths) por jugador, debajo del minimapa.
/// Se autohospeda: si no encuentra un Canvas Screen-Space en escena, crea uno propio.
/// </summary>
public class ClientScoreboardHUD : MonoBehaviour
{
    [Header("UI (opcional, se crean automáticamente si no se asignan)")]
    [SerializeField] TextMeshProUGUI headerText;
    [SerializeField] TextMeshProUGUI scoreboardText;

    [Header("Layout cabecera (Time / FPS)")]
    [SerializeField] Vector2 headerOffset = new(-12f, -2f);
    [SerializeField] int headerFontSize = 14;

    [Header("Layout marcador (Kills / Deaths)")]
    [SerializeField] Vector2 scoreboardOffset = new(-12f, -210f);
    [SerializeField] int scoreboardFontSize = 14;

    [Header("Refresco")]
    [SerializeField] float refreshIntervalSeconds = 0.25f;

    float m_NextRefreshTime;
    float m_MatchStartTime;
    readonly StringBuilder m_Sb = new StringBuilder(512);

    static TMP_FontAsset s_CachedRobotoHudFont;

    static TMP_FontAsset ResolveGameHudFont()
    {
        if (s_CachedRobotoHudFont != null) return s_CachedRobotoHudFont;
        var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        for (int i = 0; i < fonts.Length; i++)
        {
            var f = fonts[i];
            if (f != null && string.Equals(f.name, "Roboto-Black SDF", StringComparison.Ordinal))
            {
                s_CachedRobotoHudFont = f;
                break;
            }
        }
        return s_CachedRobotoHudFont;
    }

    void Start()
    {
        m_MatchStartTime = Time.unscaledTime;
        EnsureUI();
    }

    void Update()
    {
        if (Time.unscaledTime < m_NextRefreshTime) return;
        m_NextRefreshTime = Time.unscaledTime + refreshIntervalSeconds;

        EnsureUI();

        if (headerText != null)
        {
            float elapsed = Time.unscaledTime - m_MatchStartTime;
            int mins = (int)(elapsed / 60f);
            int secs = (int)(elapsed % 60f);
            int fps = Mathf.RoundToInt(1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime));
            headerText.text = $"Time {mins:D2}:{secs:D2}   FPS {fps}";
        }

        if (scoreboardText != null)
        {
            var tags = FindObjectsByType<PlayerNameTag>(FindObjectsSortMode.None);
            m_Sb.Clear();
            for (int i = 0; i < tags.Length; i++)
            {
                var t = tags[i];
                string name = t.NetworkedName.Value.ToString();
                if (string.IsNullOrWhiteSpace(name))
                    name = $"Player {t.OwnerClientId}";

                int kills = t.Kills.Value;
                int deaths = t.Deaths.Value;
                int score = kills - deaths;

                m_Sb.Append(name);
                m_Sb.Append("   Kills ").Append(kills);
                m_Sb.Append("   Deaths ").Append(deaths);
                m_Sb.Append("   Score ").Append(score >= 0 ? "+" : "").Append(score);
                m_Sb.AppendLine();
            }
            scoreboardText.text = m_Sb.ToString();
        }
    }

    void EnsureUI()
    {
        if (headerText != null && scoreboardText != null) return;

        Canvas canvas = null;
        var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay || c.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas = c;
                break;
            }
        }

        if (canvas == null)
        {
            var canvasGo = new GameObject("MatchHUDCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        if (headerText == null)
            headerText = CreateText(canvas, "MatchHUDHeader", headerOffset, headerFontSize);

        if (scoreboardText == null)
            scoreboardText = CreateText(canvas, "MatchHUDScoreboard", scoreboardOffset, scoreboardFontSize);
    }

    TextMeshProUGUI CreateText(Canvas canvas, string name, Vector2 offset, int fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(560f, 320f);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        var hudFont = ResolveGameHudFont();
        if (hudFont != null)
            tmp.font = hudFont;
        else if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;

        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.TopRight;
        tmp.richText = true;
        tmp.text = "";
        return tmp;
    }
}
