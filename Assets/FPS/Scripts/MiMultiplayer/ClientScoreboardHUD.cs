using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD de pantalla que muestra duración de la partida, FPS y la línea
/// kills - deaths (Score) por cada jugador. Se autohospeda: si no encuentra
/// un Canvas Screen-Space en escena, crea uno propio. Basta con dejar este
/// componente en cualquier GameObject de la escena.
/// </summary>
public class ClientScoreboardHUD : MonoBehaviour
{
    [Header("UI (opcional, se crea automáticamente si no se asigna)")]
    [SerializeField] TextMeshProUGUI scoreboardText;

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
        if (scoreboardText == null) return;

        var tags = FindObjectsByType<PlayerNameTag>(FindObjectsSortMode.None);

        m_Sb.Clear();

        float elapsed = Time.unscaledTime - m_MatchStartTime;
        int mins = (int)(elapsed / 60f);
        int secs = (int)(elapsed % 60f);
        m_Sb.Append("Time ").Append(mins.ToString("D2")).Append(':').Append(secs.ToString("D2"));
        m_Sb.Append("   FPS ").Append(Mathf.RoundToInt(1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime)));
        m_Sb.AppendLine();
        m_Sb.AppendLine();

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

    void EnsureUI()
    {
        if (scoreboardText != null) return;

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

        var go = new GameObject("ScoreboardText", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-16f, -240f);
        rt.sizeDelta = new Vector2(640f, 420f);

        scoreboardText = go.AddComponent<TextMeshProUGUI>();
        var hudFont = ResolveGameHudFont();
        if (hudFont != null)
            scoreboardText.font = hudFont;
        else if (TMP_Settings.defaultFontAsset != null)
            scoreboardText.font = TMP_Settings.defaultFontAsset;

        scoreboardText.fontSize = 22;
        scoreboardText.fontStyle = FontStyles.Normal;
        scoreboardText.alignment = TextAlignmentOptions.TopRight;
        scoreboardText.richText = true;
        scoreboardText.text = "";
    }
}
