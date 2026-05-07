using System;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// HUD local que muestra el marcador (nombre, kills, muertes) de TODOS los jugadores.
/// Se habilita solo para el jugador dueño.
/// </summary>
public class ClientScoreboardHUD : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI scoreboardText;

    [Header("Update")]
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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        m_MatchStartTime = Time.unscaledTime;
        EnsureUI();
    }

    void Update()
    {
        if (!IsOwner) return;
        if (Time.unscaledTime < m_NextRefreshTime) return;
        m_NextRefreshTime = Time.unscaledTime + refreshIntervalSeconds;

        EnsureUI();
        if (scoreboardText == null) return;

        var allStats = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);

        m_Sb.Clear();

        float elapsed = Time.unscaledTime - m_MatchStartTime;
        int mins = (int)(elapsed / 60f);
        int secs = (int)(elapsed % 60f);
        m_Sb.Append("Time ").Append(mins.ToString("D2")).Append(':').Append(secs.ToString("D2"));
        m_Sb.Append("   FPS ").Append(Mathf.RoundToInt(1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime)));
        m_Sb.AppendLine();
        m_Sb.AppendLine();

        for (int i = 0; i < allStats.Length; i++)
        {
            var s = allStats[i];
            string name = s.PlayerName.Value.ToString();
            if (string.IsNullOrWhiteSpace(name))
                name = $"Player {s.OwnerClientId}";

            int kills = s.Kills.Value;
            int deaths = s.Deaths.Value;
            int score = kills - deaths;

            m_Sb.Append(name);
            m_Sb.Append("  K ").Append(kills);
            m_Sb.Append("  D ").Append(deaths);
            m_Sb.Append("  Score ").Append(score >= 0 ? "+" : "").Append(score);
            m_Sb.AppendLine();
        }

        scoreboardText.text = m_Sb.ToString();
    }

    void EnsureUI()
    {
        if (scoreboardText != null) return;

        // Buscamos un Canvas en el prefab del jugador (ya existe en Human_Prefab).
        var canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null) return;

        var go = new GameObject("ScoreboardText", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-16f, -16f);
        rt.sizeDelta = new Vector2(640f, 420f);

        scoreboardText = go.AddComponent<TextMeshProUGUI>();
        var hudFont = ResolveGameHudFont();
        if (hudFont != null)
            scoreboardText.font = hudFont;
        else if (TMP_Settings.defaultFontAsset != null)
            scoreboardText.font = TMP_Settings.defaultFontAsset;

        scoreboardText.fontSize = 18;
        scoreboardText.fontStyle = FontStyles.Normal;
        scoreboardText.enableWordWrapping = true;
        scoreboardText.alignment = TextAlignmentOptions.TopRight;
        scoreboardText.text = "";
    }
}

