using UnityEngine;

namespace DeepOceanMining.Data
{
    /// <summary>
    /// Tek bir upgrade'in (Mining Rate, Ship Speed, Ship Count) veri tanımı.
    /// MiningSiteData içinde gömülü olarak durur (ayrı asset değil).
    /// </summary>
    [System.Serializable]
    public class UpgradeDefinition
    {
        [Header("Kimlik")]
        public string displayName = "Mining Rate";
        [Tooltip("Buton üzerinde gösterilecek birim (örn: \"/sec\", \"mkph\", \"\")")]
        public string unitSuffix = "";

        [Header("Seviye")]
        [Tooltip("Oyun başladığında kaçıncı seviyeden başlar")]
        public int startingLevel = 1;

        [Tooltip("Maksimum seviye (0 = sınırsız)")]
        public int maxLevel = 0;

        [Header("Değer Eğrisi")]
        [Tooltip("StartingLevel'daki değer")]
        public double baseValue = 1.0;

        [Tooltip("Her seviyede değerin ÇARPILDIĞI katsayı (1 = büyümez)")]
        public double valueMultiplier = 1.1;

        [Tooltip("Her seviyede değere EKLENEN sabit miktar (0 = eklenmez)")]
        public double valueIncrementPerLevel = 0;

        [Header("Maliyet Eğrisi")]
        [Tooltip("Başlangıçtan 1 üst seviyeye çıkmanın maliyeti")]
        public double baseCost = 10.0;

        [Tooltip("Her seviyede maliyetin ÇARPILDIĞI katsayı")]
        public double costMultiplier = 1.15;

        // --- Yardımcı metodlar (kod tarafında kullanılacak) ---

        /// <summary>Verilen seviyedeki değeri döner.</summary>
        public double GetValueAtLevel(int level)
        {
            int steps = Mathf.Max(0, level - startingLevel);
            return baseValue * System.Math.Pow(valueMultiplier, steps)
                   + valueIncrementPerLevel * steps;
        }

        /// <summary>currentLevel'dan bir üst seviyeye geçmenin maliyetini döner.</summary>
        public double GetCostForNextLevel(int currentLevel)
        {
            int exponent = Mathf.Max(0, currentLevel - startingLevel);
            return baseCost * System.Math.Pow(costMultiplier, exponent);
        }

        /// <summary>Bu seviye maksimum mu?</summary>
        public bool IsMaxLevel(int level)
        {
            return maxLevel > 0 && level >= maxLevel;
        }
    }
}