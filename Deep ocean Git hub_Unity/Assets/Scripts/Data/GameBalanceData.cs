using UnityEngine;

namespace DeepOceanMining.Data
{
    /// <summary>
    /// Tüm oyunu tek noktadan dengelemek için global çarpanlar.
    /// Test sırasında değerleri değiştirip oyunun tamamını
    /// hızlandırabilir/yavaşlatabilirsin.
    /// </summary>
    [CreateAssetMenu(fileName = "GameBalance", menuName = "Deep Ocean Mining/Game Balance", order = 2)]
    public class GameBalanceData : ScriptableObject
    {
        [Header("Global Çarpanlar (1 = etkisiz)")]
        [Tooltip("Tüm sahaların yolculuk süresine uygulanır. <1 = hızlandırır, >1 = yavaşlatır.")]
        public double globalTravelTimeMultiplier = 1.0;

        [Tooltip("Tüm kaynakların satış fiyatına uygulanır.")]
        public double globalSalePriceMultiplier = 1.0;

        [Tooltip("Tüm upgrade ve kilit açma maliyetlerine uygulanır.")]
        public double globalCostMultiplier = 1.0;
    }
}