using System.Collections.Generic;
using UnityEngine;

namespace DeepOceanMining.Data
{
    /// <summary>
    /// Tüm oyun verisinin merkezi referansı.
    /// Oyun kodun sahadaki tüm verilere buradan ulaşır.
    /// </summary>
    [CreateAssetMenu(fileName = "GameDatabase", menuName = "Deep Ocean Mining/Game Database", order = 3)]
    public class GameDatabase : ScriptableObject
    {
        [Header("Referanslar")]
        public GameBalanceData balance;

        [Header("Kaynaklar")]
        public List<ResourceData> resources = new List<ResourceData>();

        [Header("Maden Sahaları (unlockOrder'a göre sıralı tutun)")]
        public List<MiningSiteData> sites = new List<MiningSiteData>();

        // --- Yardımcı metodlar ---
        public MiningSiteData GetSiteById(string siteId)
        {
            foreach (var s in sites)
                if (s != null && s.siteId == siteId) return s;
            return null;
        }

        public ResourceData GetResourceById(string resourceId)
        {
            foreach (var r in resources)
                if (r != null && r.resourceId == resourceId) return r;
            return null;
        }
    }
}