using UnityEngine;

namespace DeepOceanMining.Data
{
    /// <summary>
    /// Bir maden sahasını tanımlar. Her saha bir kaynak üretir
    /// ve 3 upgrade (Mining Rate, Ship Speed, Ship Count) içerir.
    /// </summary>
    [CreateAssetMenu(fileName = "Site_", menuName = "Deep Ocean Mining/Mining Site Data", order = 1)]
    public class MiningSiteData : ScriptableObject
    {
        [Header("Kimlik")]
        public string siteId = "site_id";
        public string displayName = "Maden Sahası";
        [TextArea(2, 4)] public string description = "";
        public Sprite icon;

        [Header("Üretilen Kaynak")]
        [Tooltip("Bu sahadan çıkan ham kaynağın verisi")]
        public ResourceData resource;

        [Header("Kilit Açma")]
        [Tooltip("Haritadaki sıralama (1, 2, 3...)")]
        public int unlockOrder = 1;

        [Tooltip("Bu sahayı açmak için gereken para")]
        public double unlockCost = 0;

        [Tooltip("Oyun başladığında bu saha açık mı?")]
        public bool unlockedByDefault = false;

        [Header("Mesafe / Yolculuk")]
        [Tooltip("Bir ROV'un sahaya gidip üsse dönmesinin taban süresi (saniye)")]
        public double travelTime = 5.0;

        [Header("Upgrade'ler")]
        public UpgradeDefinition miningRate = new UpgradeDefinition
        {
            displayName = "Mining Rate",
            unitSuffix = "/sec",
            baseValue = 0.5,
            valueMultiplier = 1.1,
            baseCost = 10,
            costMultiplier = 1.15
        };

        public UpgradeDefinition shipSpeed = new UpgradeDefinition
        {
            displayName = "Ship Speed",
            unitSuffix = "mkph",
            baseValue = 1.0,
            valueMultiplier = 1.05,
            baseCost = 15,
            costMultiplier = 1.2
        };

        public UpgradeDefinition shipCount = new UpgradeDefinition
        {
            displayName = "Ship Count",
            unitSuffix = "",
            baseValue = 1,              // Level 1'de 1 ROV
            valueMultiplier = 1.0,      // çarpılmaz
            valueIncrementPerLevel = 1, // her seviyede +1 ROV
            baseCost = 50,
            costMultiplier = 1.5
        };
    }
}