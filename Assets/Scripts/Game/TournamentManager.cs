using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class TournamentManager : MonoBehaviour
{
    public static TournamentManager Instance;

    [SerializeField] private List<string> turnuvaSahneListesi;
    private int aktifSahneIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TurnuvayaBasla()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("⛔ Sadece sunucu turnuvayı başlatabilir.");
            return;
        }

        aktifSahneIndex = 0;
        SahneyiYukle(aktifSahneIndex);
    }

    public void SonrakiSahneyeGec()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        aktifSahneIndex++;

        if (aktifSahneIndex < turnuvaSahneListesi.Count)
        {
            SahneyiYukle(aktifSahneIndex);
        }
        else
        {
            Debug.Log("🏁 Turnuva bitti!");
            // Ana menüye dön veya bitiş sahnesi
            // SceneManager.LoadScene("AnaMenu");
        }
    }

    private void SahneyiYukle(int index)
    {
        string sahneAdi = turnuvaSahneListesi[index];
        Debug.Log("📦 Yükleniyor: " + sahneAdi);
        NetworkManager.Singleton.SceneManager.LoadScene(sahneAdi, LoadSceneMode.Single);
    }

    // Bu metod oyun sonunda çağrılmalı
    public void OyunBitti()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        Debug.Log("✅ Oyun bitti. 35 saniye sonra sonraki sahneye geçilecek...");
        StartCoroutine(SahneyeGecikmeliGecis());
    }

    private IEnumerator SahneyeGecikmeliGecis()
    {
        yield return new WaitForSeconds(35f);
        SonrakiSahneyeGec();
    }
}
