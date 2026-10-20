using UnityEngine;

namespace DeepOceanMining.Data
{
    /// <summary>
    /// Bir ham kaynağı tanımlar (örn: Manganez, Kobalt, İnci).
    /// Her maden sahası bir ResourceData referansı taşır.
    /// </summary>
    [CreateAssetMenu(fileName = "Resource_", menuName = "Deep Ocean Mining/Resource Data", order = 0)]
    public class ResourceData : ScriptableObject
    {
        [Header("Kimlik")]
        public string resourceId = "resource_id";
        public string displayName = "Kaynak";
        public Sprite icon;

        [Header("Ekonomi")]
        [Tooltip("Bu kaynağın 1 biriminin taban satış fiyatı ($)")]
        public double baseSellPrice = 1.0;

        [Header("Kategori (ileride ORES/ALLOYS/ITEMS için)")]
        public string category = "Ores";
    }
}