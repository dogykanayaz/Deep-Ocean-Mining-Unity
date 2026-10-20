using System.Collections.Generic;
using TMPro;
using DeepOceanMining.Data;
using DeepOceanMining.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace DeepOceanMining.UI
{
    public class InventoryController : MonoBehaviour
    {
        [Header("Kategori Sekmeleri")]
        public Button oresTab;
        public Button alloysTab;
        public Button itemsTab;

        [Header("Liste")]
        public RectTransform contentParent;
        public ResourceRowView rowPrefab;

        [Header("Satış Paneli")]
        public TextMeshProUGUI selectedText;
        public TextMeshProUGUI unitPriceText;
        public Slider amountSlider;
        public TextMeshProUGUI amountText;
        public Button maxButton;
        public Button sellButton;
        public TextMeshProUGUI earnText;

        [Header("Renkler")]
        public Color activeTabColor = new Color(0.16f, 0.49f, 0.79f, 1f); // #2A7DC8
        public Color inactiveTabColor = new Color(0.08f, 0.24f, 0.37f, 1f); // #153E5E

        private string currentCategory = "Ores";
        private ResourceData selectedResource;
        private List<ResourceRowView> rows = new List<ResourceRowView>();
        private float refreshTimer = 0f;
        private const float RefreshInterval = 0.25f;

        void Awake()
        {
            if (oresTab != null) oresTab.onClick.AddListener(() => SetCategory("Ores"));
            if (alloysTab != null) alloysTab.onClick.AddListener(() => SetCategory("Alloys"));
            if (itemsTab != null) itemsTab.onClick.AddListener(() => SetCategory("Items"));

            if (maxButton != null) maxButton.onClick.AddListener(OnMaxClicked);
            if (sellButton != null) sellButton.onClick.AddListener(OnSellClicked);

            if (amountSlider != null)
            {
                amountSlider.minValue = 0f;
                amountSlider.maxValue = 1f;
                amountSlider.onValueChanged.AddListener(OnSliderChanged);
            }
        }

        void OnEnable()
        {
            RebuildList();
            RefreshSellPanel();
        }

        void Update()
        {
            refreshTimer += Time.unscaledDeltaTime;
            if (refreshTimer >= RefreshInterval)
            {
                refreshTimer = 0f;
                RefreshAllRows();
                RefreshSellPanel();
            }
        }

        // ==== SEKME ====

        public void SetCategory(string category)
        {
            currentCategory = category;
            UpdateTabColors();
            RebuildList();
        }

        void UpdateTabColors()
        {
            SetTabColor(oresTab, currentCategory == "Ores");
            SetTabColor(alloysTab, currentCategory == "Alloys");
            SetTabColor(itemsTab, currentCategory == "Items");
        }

        void SetTabColor(Button btn, bool isActive)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = isActive ? activeTabColor : inactiveTabColor;
        }

        // ==== LİSTE ====

        void RebuildList()
        {
            foreach (var r in rows) if (r != null) Destroy(r.gameObject);
            rows.Clear();

            var state = GameManager.Instance != null ? GameManager.Instance.State : null;
            if (state == null || state.Database == null) return;

            foreach (var res in state.Database.resources)
            {
                if (res == null) continue;
                if (!string.Equals(res.category, currentCategory, System.StringComparison.OrdinalIgnoreCase)) continue;

                var row = Instantiate(rowPrefab, contentParent);
                row.Bind(res);
                row.OnClick += OnResourceClicked;
                rows.Add(row);
            }
            UpdateTabColors();
            UpdateRowSelection();
        }

        void RefreshAllRows()
        {
            foreach (var r in rows) if (r != null) r.Refresh();
        }

        void UpdateRowSelection()
        {
            foreach (var r in rows)
            {
                if (r == null) continue;
                bool selected = selectedResource != null && r.Resource == selectedResource;
                r.SetSelected(selected);
            }
        }

        // ==== SEÇİM / SATIŞ ====

        void OnResourceClicked(ResourceData data)
        {
            selectedResource = data;
            UpdateRowSelection();
            RefreshSellPanel();
            if (amountSlider != null) amountSlider.value = 0f;
        }

        void RefreshSellPanel()
        {
            var state = GameManager.Instance != null ? GameManager.Instance.State : null;
            if (state == null) return;

            if (selectedResource == null)
            {
                if (selectedText != null) selectedText.text = "Kaynak seç...";
                if (unitPriceText != null) unitPriceText.text = "";
                if (amountText != null) amountText.text = "0 / 0";
                if (earnText != null) earnText.text = "Kazanç: $0.00";
                if (amountSlider != null) { amountSlider.minValue = 0; amountSlider.maxValue = 1; amountSlider.value = 0; }
                if (sellButton != null) sellButton.interactable = false;
                if (maxButton != null) maxButton.interactable = false;
                return;
            }

            double have = state.GetResourceAmount(selectedResource.resourceId);
            double unit = state.GetActualSellPrice(selectedResource);

            if (selectedText != null) selectedText.text = selectedResource.displayName;
            if (unitPriceText != null) unitPriceText.text = $"@ ${unit:F2} / birim";

            if (amountSlider != null)
            {
                amountSlider.minValue = 0f;
                amountSlider.maxValue = (float)System.Math.Max(0.0, have);
                if (amountSlider.value > (float)have) amountSlider.value = (float)have;
            }

            double selectedAmount = amountSlider != null ? amountSlider.value : 0;
            if (amountText != null) amountText.text = $"{selectedAmount:F2} / {have:F2}";
            if (earnText != null) earnText.text = $"Kazanç: ${(selectedAmount * unit):F2}";

            bool canSell = have > 0 && selectedAmount > 0;
            if (sellButton != null) sellButton.interactable = canSell;
            if (maxButton != null) maxButton.interactable = have > 0;
        }

        void OnSliderChanged(float value)
        {
            var state = GameManager.Instance != null ? GameManager.Instance.State : null;
            if (state == null || selectedResource == null) return;

            double have = state.GetResourceAmount(selectedResource.resourceId);
            double unit = state.GetActualSellPrice(selectedResource);

            if (amountText != null) amountText.text = $"{value:F2} / {have:F2}";
            if (earnText != null) earnText.text = $"Kazanç: ${(value * unit):F2}";
            if (sellButton != null) sellButton.interactable = have > 0 && value > 0;
        }

        void OnMaxClicked()
        {
            if (amountSlider == null) return;
            amountSlider.value = amountSlider.maxValue;
        }

        void OnSellClicked()
        {
            var state = GameManager.Instance != null ? GameManager.Instance.State : null;
            if (state == null || selectedResource == null || amountSlider == null) return;

            double amount = amountSlider.value;
            double earned; string reason;
            if (state.SellResource(selectedResource.resourceId, amount, out earned, out reason))
            {
                Debug.Log($"[Inventory] {amount:F2} {selectedResource.displayName} satıldı → +${earned:F2}");
                amountSlider.value = 0f;
                RefreshAllRows();
                RefreshSellPanel();
            }
            else Debug.Log($"[Inventory] Satış başarısız: {reason}");
        }
    }
}