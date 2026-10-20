using DeepOceanMining.Data;
using DeepOceanMining.Runtime;
using UnityEngine;

namespace DeepOceanMining
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Veri")]
        public GameDatabase database;

        [Header("Başlangıç")]
        public double startingMoney = 100;

        [Header("Test Ayarları")]
        [Range(0.1f, 20f)]
        public float simulationSpeed = 1f;

        [Header("Kayıt Sistemi")]
        [Tooltip("Kaç saniyede bir otomatik kaydedilsin")]
        public float autosaveIntervalSeconds = 30f;

        public GameState State { get; private set; }

        private float autosaveTimer = 0f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (database == null)
            {
                Debug.LogError("GameManager: 'database' alanı boş!");
                return;
            }

            // 1) Taze state oluştur (varsayılan değerlerle)
            State = new GameState(database, startingMoney);

            // 2) Kayıt varsa üzerine yaz
            if (SaveSystem.HasSave())
            {
                var save = SaveSystem.Load();
                if (save != null)
                {
                    State.LoadFromSaveData(save);
                    Debug.Log("[GameManager] Kayıt yüklendi.");
                }
            }
            else
            {
                Debug.Log("[GameManager] Kayıt bulunamadı, yeni oyun başlatıldı.");
            }
        }

        void Update()
        {
            if (State == null) return;
            State.Tick(Time.deltaTime * simulationSpeed);

            // Otomatik kayıt
            autosaveTimer += Time.unscaledDeltaTime;
            if (autosaveTimer >= autosaveIntervalSeconds)
            {
                autosaveTimer = 0f;
                SaveNow();
            }
        }

        // ==== KAYDETME ====

        public void SaveNow()
        {
            if (State == null) return;
            SaveSystem.Save(State.ToSaveData());
        }

        // Uygulama kapanırken (Windows/Mac/Editor)
        void OnApplicationQuit()
        {
            SaveNow();
        }

        // Uygulama arkaya atılınca (mobil)
        void OnApplicationPause(bool paused)
        {
            if (paused) SaveNow();
        }

        // Uygulama odağa girdiğinde kayıt varsa yeniden yükleme opsiyonel — şimdilik yok
        void OnApplicationFocus(bool focused) { }
    }
}