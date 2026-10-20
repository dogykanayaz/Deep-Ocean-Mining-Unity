using System.Collections.Generic;
using DeepOceanMining.Runtime;
using UnityEngine;

namespace DeepOceanMining.UI
{
    /// <summary>
    /// Saha ikonlarını Canvas içine halka şeklinde yerleştirir.
    /// Yeni saha eklendiğinde otomatik yerleşir.
    /// </summary>
    public class MapController : MonoBehaviour
    {
        [Header("Referanslar")]
        public RectTransform mapArea;
        public RectTransform baseView;
        public SiteIconView siteIconPrefab;

        [Header("Yerleşim")]
        [Tooltip("Üssün merkeze göre kaydırılması (üst barı dengelemek için)")]
        public Vector2 centerOffset = new Vector2(0, -70);

        [Tooltip("İlk halkaya kaç saha sığar")]
        public int sitesInFirstRing = 6;

        [Tooltip("İlk halkanın yarıçapı (piksel)")]
        public float firstRingRadius = 320f;

        [Tooltip("Her yeni halkada yarıçap artışı")]
        public float ringRadiusStep = 260f;

        [Tooltip("Her yeni halkada kaç ekstra saha kapasitesi")]
        public int additionalSitesPerRing = 2;

        [Tooltip("İlk sahanın açı konumu (derece). 90 = tam üst")]
        public float startAngleDegrees = 90f;

        [Header("Yenileme")]
        public float refreshIntervalSeconds = 0.5f;

        public event System.Action<SiteRuntime> OnSiteClicked;

        private List<SiteIconView> spawnedIcons = new List<SiteIconView>();
        private float refreshTimer = 0f;

        void Start()
        {
            // GameManager'ın Awake'i bittikten sonra kur
            Invoke(nameof(BuildMap), 0.1f);
        }

        public void BuildMap()
        {
            if (GameManager.Instance == null || GameManager.Instance.State == null)
            {
                Debug.LogWarning("[MapController] GameManager hazır değil, harita kurulamadı.");
                return;
            }

            foreach (var icon in spawnedIcons)
                if (icon != null) Destroy(icon.gameObject);
            spawnedIcons.Clear();

            var sites = GameManager.Instance.State.Sites;
            for (int i = 0; i < sites.Count; i++)
            {
                var site = sites[i];
                if (site == null || site.Data == null) continue;

                var icon = Instantiate(siteIconPrefab, mapArea);
                icon.Bind(site);
                icon.OnClick += HandleSiteClick;

                var rt = icon.GetComponent<RectTransform>();
                rt.anchoredPosition = CalculatePosition(i);
                rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0);

                spawnedIcons.Add(icon);
            }

            if (baseView != null)
                baseView.anchoredPosition = centerOffset;

            Debug.Log($"[MapController] {spawnedIcons.Count} saha ikonu yerleştirildi.");
        }

        Vector2 CalculatePosition(int index)
        {
            int ringIndex = 0;
            int remaining = index;
            int ringCapacity = sitesInFirstRing;

            while (remaining >= ringCapacity)
            {
                remaining -= ringCapacity;
                ringIndex++;
                ringCapacity += additionalSitesPerRing;
            }

            int indexInRing = remaining;
            int countInThisRing = ringCapacity;
            float radius = firstRingRadius + ringIndex * ringRadiusStep;
            float angleStep = 360f / countInThisRing;
            float angle = startAngleDegrees + indexInRing * angleStep;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            return pos + centerOffset;
        }

        void HandleSiteClick(SiteRuntime site)
        {
            Debug.Log($"[Map] Saha tıklandı: {site.Data.displayName}");
            Debug.Log($"[Map] OnSiteClicked subscriber sayısı: {OnSiteClicked?.GetInvocationList().Length ?? 0}");
            OnSiteClicked?.Invoke(site);

        }

        public void RefreshAll()
        {
            foreach (var icon in spawnedIcons)
                if (icon != null) icon.Refresh();
        }

        void Update()
        {
            refreshTimer += Time.unscaledDeltaTime;
            if (refreshTimer >= refreshIntervalSeconds)
            {
                refreshTimer = 0f;
                RefreshAll();
            }
        }
    }
}