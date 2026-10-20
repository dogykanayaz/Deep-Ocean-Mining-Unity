using TMPro;
using DeepOceanMining.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace DeepOceanMining.UI
{
    /// <summary>
    /// Haritadaki tek bir saha ikonunu temsil eder.
    /// Prefab üzerinde durur; Bind() ile bir SiteRuntime'a bağlanır.
    /// </summary>
    public class SiteIconView : MonoBehaviour
    {
        [Header("Referanslar")]
        public Button button;
        public Image backgroundImage;
        public Image resourceIcon;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI levelText;
        public GameObject lockOverlay;

        [Header("Renkler")]
        public Color unlockedColor = new Color(0.10f, 0.45f, 0.75f, 1f);
        public Color lockedColor = new Color(0.18f, 0.18f, 0.20f, 1f);

        public SiteRuntime Site { get; private set; }
        public System.Action<SiteRuntime> OnClick;

        void Awake()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    if (Site != null) OnClick?.Invoke(Site);
                });
            }
        }

        public void Bind(SiteRuntime site)
        {
            Site = site;
            Refresh();
        }

        public void Refresh()
        {
            if (Site == null || Site.Data == null) return;

            if (nameText != null)
                nameText.text = Site.Data.displayName;

            if (levelText != null)
            {
                if (Site.IsUnlocked)
                {
                    int total = Site.MiningRateLevel + Site.ShipSpeedLevel + Site.ShipCountLevel;
                    levelText.text = $"Lv. {total}";
                }
                else
                {
                    levelText.text = "";
                }
            }

            if (backgroundImage != null)
                backgroundImage.color = Site.IsUnlocked ? unlockedColor : lockedColor;

            if (resourceIcon != null)
            {
                if (Site.Data.resource != null && Site.Data.resource.icon != null)
                {
                    resourceIcon.sprite = Site.Data.resource.icon;
                    resourceIcon.enabled = true;
                }
                else
                {
                    resourceIcon.enabled = false;
                }
            }

            if (lockOverlay != null)
                lockOverlay.SetActive(!Site.IsUnlocked);
        }
    }
}