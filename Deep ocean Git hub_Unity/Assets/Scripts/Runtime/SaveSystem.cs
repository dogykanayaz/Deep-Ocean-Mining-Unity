using System;
using System.IO;
using UnityEngine;

namespace DeepOceanMining.Runtime
{
    /// <summary>
    /// Diske kayıt/yükleme işlemlerini yönetir.
    /// MonoBehaviour değil, statik yardımcı sınıf.
    /// </summary>
    public static class SaveSystem
    {
        private const string FileName = "deep_ocean_save.json";

        public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool HasSave()
        {
            return File.Exists(SavePath);
        }

        public static void Save(SaveData data)
        {
            try
            {
                data.lastSaveUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(SavePath, json);
                Debug.Log($"[SaveSystem] Kayıt yazıldı: {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Kayıt başarısız: {e.Message}");
            }
        }

        public static SaveData Load()
        {
            try
            {
                if (!File.Exists(SavePath)) return null;
                string json = File.ReadAllText(SavePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[SaveSystem] Kayıt okundu: {SavePath}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Yükleme başarısız: {e.Message}");
                return null;
            }
        }

        public static void Delete()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                    Debug.Log("[SaveSystem] Kayıt silindi.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Silme başarısız: {e.Message}");
            }
        }
    }
}