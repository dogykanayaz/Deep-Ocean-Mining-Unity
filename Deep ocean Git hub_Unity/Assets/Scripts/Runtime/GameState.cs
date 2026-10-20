using System.Collections.Generic;
using DeepOceanMining.Data;
using UnityEngine;

namespace DeepOceanMining.Runtime
{
    public class GameState
    {
        public GameDatabase Database;
        public double Money;
        public Dictionary<string, double> Inventory = new Dictionary<string, double>();
        public List<SiteRuntime> Sites = new List<SiteRuntime>();

        public GameState(GameDatabase db, double startingMoney)
        {
            Database = db;
            Money = startingMoney;
            Inventory.Clear();
            Sites.Clear();

            if (db == null)
            {
                Debug.LogError("GameState: Database null!");
                return;
            }

            foreach (var siteData in db.sites)
            {
                if (siteData == null) continue;
                Sites.Add(new SiteRuntime(siteData, siteData.unlockedByDefault));
            }
        }

        public void Tick(double deltaTime)
        {
            double travelMult = (Database.balance != null)
                ? Database.balance.globalTravelTimeMultiplier
                : 1.0;

            foreach (var site in Sites)
            {
                double produced = site.Tick(deltaTime, travelMult);
                if (produced > 0 && site.Data.resource != null)
                {
                    string id = site.Data.resource.resourceId;
                    if (!Inventory.ContainsKey(id)) Inventory[id] = 0;
                    Inventory[id] += produced;
                }
            }
        }

        public double GetResourceAmount(string resourceId)
        {
            return Inventory.TryGetValue(resourceId, out var v) ? v : 0;
        }

        // ==== UPGRADE ====

        public double GetActualUpgradeCost(SiteRuntime site, UpgradeType type)
        {
            double raw = site.GetUpgradeCost(type);
            double mult = (Database != null && Database.balance != null)
                ? Database.balance.globalCostMultiplier
                : 1.0;
            return raw * mult;
        }

        public bool TryUpgrade(SiteRuntime site, UpgradeType type, out string failReason)
        {
            failReason = null;

            if (site == null) { failReason = "Saha bulunamadı"; return false; }
            if (!site.IsUnlocked) { failReason = "Saha kilitli"; return false; }
            if (!site.CanUpgrade(type)) { failReason = "Maksimum seviyeye ulaşıldı"; return false; }

            double cost = GetActualUpgradeCost(site, type);
            if (Money < cost)
            {
                failReason = $"Yetersiz para (gereken: ${cost:F2})";
                return false;
            }

            Money -= cost;
            site.ApplyUpgrade(type);
            return true;
        }

        // ==== SATIŞ ====

        public double GetActualSellPrice(ResourceData resource)
        {
            if (resource == null) return 0;
            double mult = (Database != null && Database.balance != null)
                ? Database.balance.globalSalePriceMultiplier
                : 1.0;
            return resource.baseSellPrice * mult;
        }

        public bool SellResource(string resourceId, double amount, out double earned, out string failReason)
        {
            earned = 0;
            failReason = null;

            if (string.IsNullOrEmpty(resourceId))
            {
                failReason = "Geçersiz kaynak";
                return false;
            }

            double have = GetResourceAmount(resourceId);
            if (have <= 0) { failReason = "Bu kaynaktan envanterde yok"; return false; }
            if (amount <= 0) { failReason = "Satılacak miktar 0"; return false; }

            if (amount > have) amount = have;

            ResourceData res = (Database != null) ? Database.GetResourceById(resourceId) : null;
            if (res == null) { failReason = "Kaynak verisi bulunamadı"; return false; }

            double unitPrice = GetActualSellPrice(res);
            earned = unitPrice * amount;

            Inventory[resourceId] = have - amount;
            if (Inventory[resourceId] < 0) Inventory[resourceId] = 0;

            Money += earned;
            return true;
        }

        // ==== KİLİT AÇMA ====

        public double GetActualUnlockCost(SiteRuntime site)
        {
            if (site == null || site.Data == null) return 0;
            double mult = (Database != null && Database.balance != null)
                ? Database.balance.globalCostMultiplier
                : 1.0;
            return site.Data.unlockCost * mult;
        }

        public bool CanUnlockSite(SiteRuntime site, out string failReason)
        {
            failReason = null;

            if (site == null) { failReason = "Saha yok"; return false; }
            if (site.Data == null) { failReason = "Saha verisi yok"; return false; }
            if (site.IsUnlocked) { failReason = "Zaten açık"; return false; }

            foreach (var other in Sites)
            {
                if (other == null || other.Data == null) continue;
                if (other == site) continue;
                if (other.Data.unlockOrder < site.Data.unlockOrder && !other.IsUnlocked)
                {
                    failReason = $"Önce '{other.Data.displayName}' açılmalı";
                    return false;
                }
            }

            double cost = GetActualUnlockCost(site);
            if (Money < cost)
            {
                failReason = $"Yetersiz para (gereken: ${cost:F2})";
                return false;
            }

            return true;
        }

        public bool TryUnlockSite(SiteRuntime site, out string failReason)
        {
            if (!CanUnlockSite(site, out failReason)) return false;

            double cost = GetActualUnlockCost(site);
            Money -= cost;
            site.IsUnlocked = true;
            site.SyncRovCount();
            return true;
        }

        // ==== KAYIT / YÜKLEME ====

        public SaveData ToSaveData()
        {
            var data = new SaveData();
            data.money = Money;

            data.inventory.Clear();
            foreach (var kv in Inventory)
            {
                data.inventory.Add(new ResourceEntry
                {
                    resourceId = kv.Key,
                    amount = kv.Value
                });
            }

            data.sites.Clear();
            foreach (var site in Sites)
            {
                if (site == null || site.Data == null) continue;
                data.sites.Add(new SiteEntry
                {
                    siteId = site.Data.siteId,
                    isUnlocked = site.IsUnlocked,
                    miningRateLevel = site.MiningRateLevel,
                    shipSpeedLevel = site.ShipSpeedLevel,
                    shipCountLevel = site.ShipCountLevel
                });
            }
            return data;
        }

        /// <summary>
        /// Kayıt verisini mevcut state'in üzerine uygular.
        /// Database'de artık olmayan sahalar/kaynaklar sessizce atlanır.
        /// </summary>
        public void LoadFromSaveData(SaveData data)
        {
            if (data == null) return;

            Money = data.money;

            Inventory.Clear();
            if (data.inventory != null)
            {
                foreach (var e in data.inventory)
                {
                    if (e == null || string.IsNullOrEmpty(e.resourceId)) continue;
                    Inventory[e.resourceId] = e.amount;
                }
            }

            if (data.sites != null)
            {
                foreach (var saved in data.sites)
                {
                    if (saved == null) continue;
                    var site = Sites.Find(s => s.Data != null && s.Data.siteId == saved.siteId);
                    if (site == null)
                    {
                        Debug.LogWarning($"[Save] '{saved.siteId}' artık Database'de yok, atlanıyor.");
                        continue;
                    }
                    site.IsUnlocked = saved.isUnlocked;
                    site.SetLevel(UpgradeType.MiningRate, saved.miningRateLevel);
                    site.SetLevel(UpgradeType.ShipSpeed, saved.shipSpeedLevel);
                    site.SetLevel(UpgradeType.ShipCount, saved.shipCountLevel);
                }
            }

            Debug.Log($"[Save] Kayıt uygulandı. Para: ${Money:F2}, envanter kalemi: {Inventory.Count}, saha: {Sites.Count}");
        }
    }
}