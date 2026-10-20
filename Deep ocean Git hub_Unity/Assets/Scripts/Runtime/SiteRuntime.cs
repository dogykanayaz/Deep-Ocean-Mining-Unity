using System.Collections.Generic;
using DeepOceanMining.Data;
using UnityEngine;

namespace DeepOceanMining.Runtime
{
    /// <summary>
    /// Bir maden sahasının OYUN İÇİ (runtime) durumu.
    /// Upgrade metodları burada.
    /// </summary>
    public class SiteRuntime
    {
        public MiningSiteData Data;
        public bool IsUnlocked;

        // Upgrade seviyeleri
        public int MiningRateLevel;
        public int ShipSpeedLevel;
        public int ShipCountLevel;

        // Her ROV'un yolculuk ilerlemesi (0..1). 1'e ulaşınca üsse dönmüş sayılır.
        public List<float> RovTripProgress = new List<float>();

        public SiteRuntime(MiningSiteData data, bool unlocked)
        {
            Data = data;
            IsUnlocked = unlocked;
            MiningRateLevel = data.miningRate.startingLevel;
            ShipSpeedLevel = data.shipSpeed.startingLevel;
            ShipCountLevel = data.shipCount.startingLevel;
            SyncRovCount();
        }

        // --- Hesaplanan değerler ---
        public double CurrentMiningRate => Data.miningRate.GetValueAtLevel(MiningRateLevel);
        public double CurrentShipSpeed => Data.shipSpeed.GetValueAtLevel(ShipSpeedLevel);
        public int CurrentShipCount => Mathf.RoundToInt((float)Data.shipCount.GetValueAtLevel(ShipCountLevel));

        public double EffectiveTripTime
        {
            get
            {
                double speed = System.Math.Max(0.01, CurrentShipSpeed);
                return Data.travelTime / speed;
            }
        }

        public void SyncRovCount()
        {
            int target = CurrentShipCount;
            while (RovTripProgress.Count < target) RovTripProgress.Add(0f);
            while (RovTripProgress.Count > target) RovTripProgress.RemoveAt(RovTripProgress.Count - 1);
        }

        public double Tick(double deltaTime, double globalTravelMultiplier)
        {
            if (!IsUnlocked) return 0;
            SyncRovCount();

            double tripTime = EffectiveTripTime * globalTravelMultiplier;
            if (tripTime < 0.001) tripTime = 0.001;

            double produced = 0;
            double progressPerSec = 1.0 / tripTime;

            for (int i = 0; i < RovTripProgress.Count; i++)
            {
                RovTripProgress[i] += (float)(progressPerSec * deltaTime);
                while (RovTripProgress[i] >= 1f)
                {
                    RovTripProgress[i] -= 1f;
                    produced += CurrentMiningRate;
                }
            }
            return produced;
        }

        // ==== UPGRADE SİSTEMİ ====

        public UpgradeDefinition GetDefinition(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.MiningRate: return Data.miningRate;
                case UpgradeType.ShipSpeed: return Data.shipSpeed;
                case UpgradeType.ShipCount: return Data.shipCount;
            }
            return null;
        }

        public int GetLevel(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.MiningRate: return MiningRateLevel;
                case UpgradeType.ShipSpeed: return ShipSpeedLevel;
                case UpgradeType.ShipCount: return ShipCountLevel;
            }
            return 0;
        }

        public void SetLevel(UpgradeType type, int level)
        {
            switch (type)
            {
                case UpgradeType.MiningRate: MiningRateLevel = level; break;
                case UpgradeType.ShipSpeed: ShipSpeedLevel = level; break;
                case UpgradeType.ShipCount: ShipCountLevel = level; break;
            }
            // Gemi sayısı değişirse ROV listesini hemen senkronize et
            if (type == UpgradeType.ShipCount) SyncRovCount();
        }

        /// <summary>Bir sonraki seviyeye çıkmanın HAM maliyeti (global çarpan hariç).</summary>
        public double GetUpgradeCost(UpgradeType type)
        {
            var def = GetDefinition(type);
            if (def == null) return double.MaxValue;
            return def.GetCostForNextLevel(GetLevel(type));
        }

        public bool CanUpgrade(UpgradeType type)
        {
            var def = GetDefinition(type);
            if (def == null) return false;
            if (!IsUnlocked) return false;
            return !def.IsMaxLevel(GetLevel(type));
        }

        /// <summary>Parayı harcamadan seviyeyi +1 yapar. Para kontrolü GameState'te.</summary>
        public bool ApplyUpgrade(UpgradeType type)
        {
            if (!CanUpgrade(type)) return false;
            SetLevel(type, GetLevel(type) + 1);
            return true;
        }
    }
}