using System.Collections.Generic;

namespace DeepOceanMining.Runtime
{
    /// <summary>
    /// Diske yazılacak tüm oyun verisi.
    /// JsonUtility Dictionary desteklemediği için List kullanıyoruz.
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        public int version = 1;
        public double money;
        public long lastSaveUnixSeconds;   // offline ilerleme için ileride kullanılacak
        public List<ResourceEntry> inventory = new List<ResourceEntry>();
        public List<SiteEntry> sites = new List<SiteEntry>();
    }

    [System.Serializable]
    public class ResourceEntry
    {
        public string resourceId;
        public double amount;
    }

    [System.Serializable]
    public class SiteEntry
    {
        public string siteId;
        public bool isUnlocked;
        public int miningRateLevel;
        public int shipSpeedLevel;
        public int shipCountLevel;
    }
}