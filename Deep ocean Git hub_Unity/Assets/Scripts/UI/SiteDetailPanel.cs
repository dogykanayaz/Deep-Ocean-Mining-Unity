using TMPro;
using DeepOceanMining.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace DeepOceanMining.UI
{
    public class SiteDetailPanel : MonoBehaviour
    {
        [Header("Bağlantılar")]
        public MapController mapController;

        [Header("Kök")]
        public GameObject rootPanel;
        public Button backdropButton;

        [Header("Başlık")]
        public Image siteIcon;
        public TextMeshProUGUI siteNameText;
        public TextMeshProUGUI resourceInfoText;
        public Button closeButton;

        [Header("Upgrade Satırları")]
        public UpgradeRowView miningRateRow;
        public UpgradeRowView shipSpeedRow;
        public UpgradeRowView shipCountRow;

        [Header("Yenileme")]
        public float refreshIntervalSeconds = 0.25f;

        private SiteRuntime currentSite;
        private float refreshTimer;

        void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Close);
            }
            if (backdropButton != null)
            {
                backdropButton.onClick.RemoveAllListeners();
                backdropButton.onClick.AddListener(Close);
            }
            if (rootPanel != null) rootPanel.SetActive(false);
        }

        void Start()
        {
            if (mapController != null) mapController.OnSiteClicked += Open;
        }

        void OnDestroy()
        {
            if (mapController != null) mapController.OnSiteClicked -= Open;
        }

        public void Open(SiteRuntime site)
        {
            if (site == null || site.Data == null) return;
            if (!site.IsUnlocked)
            {
                Debug.Log($"[UI] '{site.Data.displayName}' kilitli.");
                return;
            }
            currentSite = site;
            if (rootPanel != null) rootPanel.SetActive(true);
            RefreshAll();
        }

        public void Close()
        {
            currentSite = null;
            if (rootPanel != null) rootPanel.SetActive(false);
        }

        public void RefreshAll()
        {
            if (currentSite == null) return;
            var state = GameManager.Instance.State;
            if (state == null) return;

            if (siteIcon != null)
            {
                if (currentSite.Data.resource != null && currentSite.Data.resource.icon != null)
                {
                    siteIcon.sprite = currentSite.Data.resource.icon;
                    siteIcon.enabled = true;
                }
                else siteIcon.enabled = false;
            }

            if (siteNameText != null) siteNameText.text = currentSite.Data.displayName;

            if (resourceInfoText != null)
            {
                string resName = currentSite.Data.resource != null
                    ? currentSite.Data.resource.displayName : "?";
                resourceInfoText.text =
                    $"Üretim: {currentSite.CurrentMiningRate:F2} {resName} / tur\n" +
                    $"ROV: {currentSite.CurrentShipCount}  |  Tur süresi: {currentSite.EffectiveTripTime:F2}s";
            }

            if (miningRateRow != null) miningRateRow.Bind(currentSite, UpgradeType.MiningRate);
            if (shipSpeedRow != null) shipSpeedRow.Bind(currentSite, UpgradeType.ShipSpeed);
            if (shipCountRow != null) shipCountRow.Bind(currentSite, UpgradeType.ShipCount);
        }

        void Update()
        {
            if (currentSite == null) return;
            refreshTimer += Time.unscaledDeltaTime;
            if (refreshTimer >= refreshIntervalSeconds)
            {
                refreshTimer = 0f;
                RefreshAll();
            }
        }
    }
}