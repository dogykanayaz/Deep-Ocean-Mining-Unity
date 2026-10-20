using UnityEngine;
using UnityEngine.UI;

namespace DeepOceanMining.UI
{
    public class TabController : MonoBehaviour
    {
        [Header("Ekranlar")]
        public GameObject mapScreen;
        public GameObject inventoryScreen;
        public GameObject settingsScreen;

        [Header("Butonlar")]
        public Button mapButton;
        public Button inventoryButton;
        public Button settingsButton;

        [Header("Paneller")]
        public SiteDetailPanel siteDetailPanel;

        [Header("Renkler")]
        public Color activeColor = new Color(0.16f, 0.49f, 0.79f, 1f);
        public Color inactiveColor = new Color(0.08f, 0.24f, 0.37f, 1f);

        void Awake()
        {
            if (mapButton != null) mapButton.onClick.AddListener(() => ShowMap());
            if (inventoryButton != null) inventoryButton.onClick.AddListener(() => ShowInventory());
            if (settingsButton != null) settingsButton.onClick.AddListener(() => ShowSettings());
        }

        void Start()
        {
            ShowMap();
        }

        public void ShowMap()
        {
            ClosePanels();
            SetActive(mapScreen, true);
            SetActive(inventoryScreen, false);
            SetActive(settingsScreen, false);
            UpdateColors(mapButton);
        }

        public void ShowInventory()
        {
            ClosePanels();
            SetActive(mapScreen, false);
            SetActive(inventoryScreen, true);
            SetActive(settingsScreen, false);
            UpdateColors(inventoryButton);
        }

        public void ShowSettings()
        {
            ClosePanels();
            SetActive(mapScreen, false);
            SetActive(inventoryScreen, false);
            SetActive(settingsScreen, true);
            UpdateColors(settingsButton);
        }

        void ClosePanels()
        {
            if (siteDetailPanel != null) siteDetailPanel.Close();
        }

        void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }

        void UpdateColors(Button activeBtn)
        {
            SetButtonColor(mapButton, mapButton == activeBtn);
            SetButtonColor(inventoryButton, inventoryButton == activeBtn);
            SetButtonColor(settingsButton, settingsButton == activeBtn);
        }

        void SetButtonColor(Button btn, bool isActive)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = isActive ? activeColor : inactiveColor;
        }
    }
}