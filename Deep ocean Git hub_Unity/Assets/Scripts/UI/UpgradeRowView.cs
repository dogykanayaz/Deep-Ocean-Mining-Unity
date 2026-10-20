using TMPro;
using DeepOceanMining.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace DeepOceanMining.UI
{
    public class UpgradeRowView : MonoBehaviour
    {
        [Header("Referanslar")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI valueText;
        public TextMeshProUGUI costText;
        public Button upgradeButton;
        public TextMeshProUGUI buttonLabel;

        private SiteRuntime site;
        private UpgradeType type;

        void Awake()
        {
            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }
        }

        public void Bind(SiteRuntime site, UpgradeType type)
        {
            this.site = site;
            this.type = type;
            Refresh();
        }

        public void Refresh()
        {
            if (site == null || site.Data == null) return;
            var state = GameManager.Instance.State;
            if (state == null) return;

            var def = site.GetDefinition(type);
            if (def == null) return;

            if (titleText != null) titleText.text = def.displayName;

            int level = site.GetLevel(type);
            bool canUp = site.CanUpgrade(type);
            double cost = state.GetActualUpgradeCost(site, type);

            if (levelText != null)
                levelText.text = canUp ? $"Lv. {level} → Lv. {level + 1}" : $"Lv. {level} (MAX)";

            if (valueText != null)
            {
                double cur = def.GetValueAtLevel(level);
                if (canUp)
                {
                    double next = def.GetValueAtLevel(level + 1);
                    valueText.text = $"{FormatValue(cur, def.unitSuffix)} → {FormatValue(next, def.unitSuffix)}";
                }
                else valueText.text = FormatValue(cur, def.unitSuffix);
            }

            if (costText != null)
                costText.text = canUp ? $"${cost:F2}" : "—";

            if (upgradeButton != null)
            {
                bool affordable = state.Money >= cost;
                upgradeButton.interactable = canUp && affordable;
                if (buttonLabel != null) buttonLabel.text = canUp ? "UPGRADE" : "MAX";
            }
        }

        string FormatValue(double v, string suffix)
        {
            if (string.IsNullOrEmpty(suffix) && System.Math.Abs(v - System.Math.Floor(v)) < 0.001)
                return v.ToString("F0");
            return $"{v:F2}{suffix}";
        }

        void OnUpgradeClicked()
        {
            if (site == null) return;
            string reason;
            if (GameManager.Instance.State.TryUpgrade(site, type, out reason))
            {
                var panel = GetComponentInParent<SiteDetailPanel>();
                if (panel != null) panel.RefreshAll();
            }
            else Debug.Log($"[UI] Upgrade başarısız: {reason}");
        }
    }
}