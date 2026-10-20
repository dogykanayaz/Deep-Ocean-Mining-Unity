using TMPro;
using DeepOceanMining.Runtime;
using UnityEngine;

namespace DeepOceanMining.UI
{
    /// <summary>
    /// Üst bar ve genel ekran elemanlarını günceller.
    /// </summary>
    public class MainUIController : MonoBehaviour
    {
        [Header("Üst Bar")]
        public TextMeshProUGUI moneyText;

        [Header("Format")]
        public string moneyPrefix = "$";

        // Son gösterilen değer (string allocation'dan kaçınmak için)
        double lastShownMoney = -1;

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State == null) return;
            if (moneyText == null) return;

            double money = GameManager.Instance.State.Money;

            // Değişmediyse güncelleme yapma
            if (System.Math.Abs(money - lastShownMoney) < 0.001) return;

            lastShownMoney = money;
            moneyText.text = moneyPrefix + FormatMoney(money);
        }

        /// <summary>
        /// Büyük sayıları K/M/B/T kısaltmaları ile gösterir.
        /// 1234.56 → 1.23K
        /// </summary>
        string FormatMoney(double value)
        {
            if (value < 0) value = 0;
            if (value < 1000) return value.ToString("F2");
            if (value < 1_000_000) return (value / 1_000.0).ToString("F2") + "K";
            if (value < 1_000_000_000) return (value / 1_000_000.0).ToString("F2") + "M";
            if (value < 1_000_000_000_000) return (value / 1_000_000_000.0).ToString("F2") + "B";
            return (value / 1_000_000_000_000.0).ToString("F2") + "T";
        }
    }
}