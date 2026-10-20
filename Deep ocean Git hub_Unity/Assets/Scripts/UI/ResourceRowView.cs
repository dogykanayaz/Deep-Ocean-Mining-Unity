using TMPro;
using DeepOceanMining.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DeepOceanMining.UI
{
    public class ResourceRowView : MonoBehaviour
    {
        [Header("Referanslar")]
        public Button button;
        public Image background;
        public Image iconImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI amountText;
        public TextMeshProUGUI unitPriceText;

        [Header("Renkler")]
        public Color normalColor = new Color(0.08f, 0.24f, 0.37f, 1f);
        public Color selectedColor = new Color(0.16f, 0.49f, 0.79f, 1f);

        public ResourceData Resource { get; private set; }
        public System.Action<ResourceData> OnClick;

        void Awake()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => { if (Resource != null) OnClick?.Invoke(Resource); });
            }
        }

        public void Bind(ResourceData data)
        {
            Resource = data;
            Refresh();
        }

        public void SetSelected(bool selected)
        {
            if (background != null)
                background.color = selected ? selectedColor : normalColor;
        }

        public void Refresh()
        {
            if (Resource == null) return;
            var state = GameManager.Instance != null ? GameManager.Instance.State : null;
            if (state == null) return;

            if (nameText != null) nameText.text = Resource.displayName;

            if (iconImage != null)
            {
                if (Resource.icon != null) { iconImage.sprite = Resource.icon; iconImage.enabled = true; }
                else iconImage.enabled = false;
            }

            double amount = state.GetResourceAmount(Resource.resourceId);
            if (amountText != null) amountText.text = amount.ToString("F2");

            double unit = state.GetActualSellPrice(Resource);
            if (unitPriceText != null) unitPriceText.text = $"${unit:F2}";
        }
    }
}