using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class KillCountUI : MonoBehaviour
{
    [SerializeField] private MatchStats matchStats;
    [SerializeField] private TMP_Text killCountText;

    private void OnEnable()
    {
        if (matchStats == null || killCountText == null)
        {
            Debug.LogError("KillCountUI requires Match Stats and Kill Count Text.", this);
            enabled = false;
            return;
        }

        matchStats.KillsChanged += RefreshUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        if (matchStats != null)
            matchStats.KillsChanged -= RefreshUI;
    }

    private void RefreshUI()
    {
        killCountText.text = matchStats.DeathMessageVisible ? "VOCÊ MORREU"
            : matchStats.Victory ? "FASE 1 CONCLUÍDA"
            : matchStats.BossSpawned ? "DERROTE O BOSS"
            : matchStats.GhostKills >= MatchStats.GhostTarget ? $"{MatchStats.GhostTarget}/{MatchStats.GhostTarget} FANTASMAS\nBOSS CHEGANDO!"
            : $"Fantasmas: {matchStats.GhostKills}/{MatchStats.GhostTarget}\nFaltam {MatchStats.GhostTarget - matchStats.GhostKills} para o boss";
    }
}
