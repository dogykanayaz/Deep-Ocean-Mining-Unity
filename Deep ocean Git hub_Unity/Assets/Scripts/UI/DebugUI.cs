using System.Collections.Generic;
using DeepOceanMining.Data;
using DeepOceanMining.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeepOceanMining.UI
{
    public class DebugUI : MonoBehaviour
    {
        Vector2 scroll;
        string openSellPanelFor = null;
        Dictionary<string, double> sellAmounts = new Dictionary<string, double>();

        void OnGUI()
        {
            if (GameManager.Instance == null || GameManager.Instance.State == null) return;
            var state = GameManager.Instance.State;

            var titleStyle = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 16, fontStyle = FontStyle.Bold };
            var normalStyle = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 13 };
            var resourceBtn = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, fontSize = 13 };

            // ==== SAĞ ÜST KÖŞE: KAYIT BUTONLARI ====
            GUILayout.BeginArea(new Rect(Screen.width - 230, 10, 220, 120));
            if (GUILayout.Button("KAYDET (manuel)", GUILayout.Height(28)))
            {
                GameManager.Instance.SaveNow();
                Debug.Log("[UI] Manuel kayıt yapıldı.");
            }
            if (GUILayout.Button("<color=#FF6666>SIFIRLA (kayıt sil + reload)</color>", GUILayout.Height(28)))
            {
                SaveSystem.Delete();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            GUILayout.Label($"<size=10>Kayıt: {SaveSystem.SavePath}</size>", normalStyle);
            GUILayout.EndArea();

            // ==== SOL TARAFTAKİ ANA PANEL ====
            GUILayout.BeginArea(new Rect(10, 10, 680, Screen.height - 20));

            GUILayout.Label($"<color=#7CFC00>PARA:</color> ${state.Money:F2}", titleStyle);
            GUILayout.Label($"Simülasyon hızı: x{GameManager.Instance.simulationSpeed}", normalStyle);

            GUILayout.Space(8);
            GUILayout.Label("ENVANTER (satmak için bir kaynağa tıkla)", titleStyle);

            if (state.Inventory.Count == 0)
            {
                GUILayout.Label("  (henüz kaynak yok)", normalStyle);
            }
            else
            {
                var keys = new List<string>(state.Inventory.Keys);
                foreach (var key in keys)
                {
                    double amount = state.GetResourceAmount(key);
                    ResourceData res = state.Database != null ? state.Database.GetResourceById(key) : null;
                    string display = res != null ? res.displayName : key;
                    double unitPrice = state.GetActualSellPrice(res);

                    bool isOpen = openSellPanelFor == key;
                    string arrow = isOpen ? "▼" : "▶";
                    string line = $"  {arrow} {display,-22} {amount,10:F2} birim  (@ ${unitPrice:F2})";

                    if (GUILayout.Button(line, resourceBtn, GUILayout.Width(640)))
                    {
                        openSellPanelFor = isOpen ? null : key;
                        if (!sellAmounts.ContainsKey(key)) sellAmounts[key] = 0;
                    }

                    if (isOpen) DrawSellPanel(state, key, amount, unitPrice);
                }
            }

            GUILayout.Space(8);
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Label("MADEN SAHALARI", titleStyle);

            foreach (var site in state.Sites)
            {
                if (site.Data == null) continue;
                DrawSite(state, site, normalStyle);
                GUILayout.Space(6);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void DrawSellPanel(GameState state, string resourceId, double haveAmount, double unitPrice)
        {
            double current = sellAmounts.ContainsKey(resourceId) ? sellAmounts[resourceId] : 0;
            if (current > haveAmount) current = haveAmount;
            if (current < 0) current = 0;

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);

            float maxF = (float)System.Math.Max(0.0, haveAmount);
            float curF = (float)System.Math.Min(current, (double)maxF);
            float newF = GUILayout.HorizontalSlider(curF, 0f, maxF, GUILayout.Width(300));

            if (Mathf.Abs(newF - curF) > 0.0001f)
                sellAmounts[resourceId] = newF;

            double shown = sellAmounts.ContainsKey(resourceId) ? sellAmounts[resourceId] : 0;
            if (shown > haveAmount) shown = haveAmount;
            if (shown < 0) shown = 0;

            GUILayout.Label($"  {shown:F2} / {haveAmount:F2}", GUILayout.Width(140));

            if (GUILayout.Button("MAX", GUILayout.Width(60)))
                sellAmounts[resourceId] = haveAmount;

            GUI.enabled = haveAmount > 0 && shown > 0;
            if (GUILayout.Button("SAT", GUILayout.Width(70)))
            {
                double earned; string reason;
                if (state.SellResource(resourceId, shown, out earned, out reason))
                {
                    Debug.Log($"{shown:F2} birim satıldı → +${earned:F2}");
                    sellAmounts[resourceId] = 0;
                }
                else Debug.Log($"Satış başarısız: {reason}");
            }
            GUI.enabled = true;

            GUILayout.Label($"  Kazanç: ${(shown * unitPrice):F2}", GUILayout.Width(160));
            GUILayout.EndHorizontal();
        }

        void DrawSite(GameState state, SiteRuntime site, GUIStyle normalStyle)
        {
            string status = site.IsUnlocked ? "<color=#7CFC00>AÇIK</color>" : "<color=#FF6B6B>KİLİTLİ</color>";
            GUILayout.Label($"<b>[{status}] {site.Data.displayName}</b> <color=#888>(sıra: {site.Data.unlockOrder})</color>", normalStyle);

            if (!site.IsUnlocked)
            {
                DrawLockedSite(state, site, normalStyle);
                return;
            }

            GUILayout.Label($"    {site.CurrentShipCount} ROV | rate: {site.CurrentMiningRate:F2}/tur | tur: {site.EffectiveTripTime:F2}s", normalStyle);

            DrawUpgradeRow(state, site, UpgradeType.MiningRate, "Mining Rate");
            DrawUpgradeRow(state, site, UpgradeType.ShipSpeed, "Ship Speed");
            DrawUpgradeRow(state, site, UpgradeType.ShipCount, "Ship Count");
        }

        void DrawLockedSite(GameState state, SiteRuntime site, GUIStyle normalStyle)
        {
            double cost = state.GetActualUnlockCost(site);
            string reason;
            bool canUnlock = state.CanUnlockSite(site, out reason);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"    Kilit maliyeti: ${cost:F2}", normalStyle, GUILayout.Width(320));

            GUI.enabled = canUnlock;
            if (GUILayout.Button(canUnlock ? "KİLİT AÇ" : "KİLİTLİ", GUILayout.Width(110)))
            {
                string failReason;
                if (state.TryUnlockSite(site, out failReason))
                    Debug.Log($"{site.Data.displayName} kilidi açıldı!");
                else
                    Debug.Log($"Kilit açma başarısız: {failReason}");
            }
            GUI.enabled = true;

            if (!canUnlock && !string.IsNullOrEmpty(reason) && !reason.StartsWith("Zaten"))
                GUILayout.Label($"  <color=#FFAA33>{reason}</color>", normalStyle);

            GUILayout.EndHorizontal();
        }

        void DrawUpgradeRow(GameState state, SiteRuntime site, UpgradeType type, string label)
        {
            int level = site.GetLevel(type);
            double cost = state.GetActualUpgradeCost(site, type);
            bool canUp = site.CanUpgrade(type);
            bool hasMoney = state.Money >= cost;

            GUILayout.BeginHorizontal();

            string levelText = canUp
                ? $"Lv.{level}   →   Lv.{level + 1}   (${cost:F2})"
                : $"Lv.{level}   (MAX)";
            GUILayout.Label($"    {label,-14} {levelText}", GUILayout.Width(320));

            GUI.enabled = canUp && hasMoney;
            string btnLabel = !canUp ? "MAX" : "UPGRADE";
            if (GUILayout.Button(btnLabel, GUILayout.Width(110)))
            {
                string reason;
                if (!state.TryUpgrade(site, type, out reason))
                    Debug.Log($"Upgrade başarısız: {reason}");
                else
                    Debug.Log($"{site.Data.displayName} → {label} yükseltildi!");
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }
    }
}