using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class PlacementPlatform : MonoBehaviour
{
    public MovableItem CurrentFruit;
    public int Score = 0;
    public TextMeshProUGUI ScoreText;
    public Transform Fruits;
    public GameObject ComplatePanel;
    public static int TotalScore = 0; // Statik değişkenle toplam skor
    private int matchCount = 0;       // Eşleşme sayısı
    public int totalPairs = 7;        // Toplam çift sayısı

    [Header("Particles & Skills")]
    public ParticleSystem matchParticleEffect;

    // 2X Skor değişkenleri
    private bool isDoubleScoreActive = false;
    private int scoreMultiplier = 1; // Başlangıçta normal skor

    void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
        guiStyle.fontSize = 30;

        // 2X Skor Butonu
        if (GUI.Button(new Rect(50, 800, 150, 50), "2X Skor", guiStyle))
        {
            isDoubleScoreActive = true;
            scoreMultiplier = 2; // 2X Skor Aktif
        }

        // [ÖRNEK] Göster Butonu (daha önce eklediğiniz)
        if (GUI.Button(new Rect(220, 800, 150, 50), "Göster", guiStyle))
        {
            StartCoroutine(JumpOnePair()); // Örnek: Sadece 1 çifti zıplatıyor
        }

        // =====================================================
        //  1) BÜYÜT BUTONU - Tüm nesneleri 5sn boyunca 2 katı yap
        // =====================================================
        if (GUI.Button(new Rect(220, 900, 150, 50), "Büyüt", guiStyle))
        {
            // Tüm MovableItem nesneleri 2 katına büyütelim, 5 saniye bekleyip geri alalım
            StartCoroutine(ScaleAllItems(2f, 5f));
        }

        // =====================================================
        //  2) KARIŞTIR BUTONU - Tüm nesnelerin pozisyonunu rastgele dağıt
        // =====================================================
        if (GUI.Button(new Rect(50, 900, 150, 50), "Karıştır", guiStyle))
        {
            ShuffleAllItems();
        }

        if (GUI.Button(new Rect(700, 50, 230, 50), "Yeniden Başlat", guiStyle))
        {
            ResetGame();
        }

    }

    /// <summary>
    /// (Örneğin) Sahnede ilk bulduğu (FruitName'i aynı olan) 2 nesneyi 1 saniye zıplatan eski bir örnek fonksiyon.
    /// Siz kendi projenizde bunun içeriğini değiştirmiş olabilirsiniz.
    /// </summary>
    private IEnumerator JumpOnePair()
    {
        MovableItem[] allItems = FindObjectsOfType<MovableItem>();
        var groupedByFruitName = allItems.GroupBy(item => item.FruitName);

        foreach (var group in groupedByFruitName)
        {
            var items = group.ToList();
            if (items.Count >= 2)
            {
                // Yalnızca ilk 2 taneyi zıplat
                items[0].StartCoroutine(items[0].Jump(1f, 1f));
                items[1].StartCoroutine(items[1].Jump(1f, 1f));
                break;
            }
        }

        yield return null;
    }

    //------------------------------------------------------------------------
    //      1) BÜYÜTME COROUTINE
    //------------------------------------------------------------------------
    private IEnumerator ScaleAllItems(float scaleFactor, float duration)
    {
        // Tüm MovableItem'ları bul
        MovableItem[] allItems = FindObjectsOfType<MovableItem>();

        // Orijinal scale'ları saklamak için dizi
        Vector3[] originalScales = new Vector3[allItems.Length];

        // 1) Hepsinin orijinal scale'ını sakla, sonra scaleFactor ile çarp
        for (int i = 0; i < allItems.Length; i++)
        {
            originalScales[i] = allItems[i].transform.localScale;
            allItems[i].transform.localScale = originalScales[i] * scaleFactor;
        }

        // 2) duration (5sn) bekle
        yield return new WaitForSeconds(duration);

        // 3) Scale'ları geri al
        for (int i = 0; i < allItems.Length; i++)
        {
            if (allItems[i] != null) // Nesne bu arada yok edilmiş olabilir
            {
                allItems[i].transform.localScale = originalScales[i];
            }
        }
    }

    //------------------------------------------------------------------------
    //      2) KARIŞTIR
    //------------------------------------------------------------------------
    private void ShuffleAllItems()
    {
        MovableItem[] allItems = FindObjectsOfType<MovableItem>();

        foreach (MovableItem item in allItems)
        {
            // RB varsa hızını sıfırlayalım (sağa sola kaymasın diye)
            if (item.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // min / max sınırları MovableItem içinden veya sabit bir yerden alabilirsiniz.
            // Örnek olarak, MovableItem içerisindeki minBoundary & maxBoundary değerini kullanabiliriz.
            // DİKKAT: item'in "minBoundary" vb. static değilse her item için farklı olabilir.
            Vector3 randomPos = new Vector3(
                Random.Range(item.minBoundary.x, item.maxBoundary.x),
                item.transform.position.y,
                Random.Range(item.minBoundary.z, item.maxBoundary.z)
            );

            // Yeni rastgele konumu ver
            item.transform.position = randomPos;
        }
    }

    // Mevcut AddScore, ResetGame vs. kısımları aşağıda
    public void AddScore()
    {
        Score += 1 * scoreMultiplier;
        ScoreText.text = $"Score: {Score}";

        matchCount++;

        if (matchCount == totalPairs)
        {
            TotalScore = Score; // Skoru sakla
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Sahneyi yeniden başlat
        }

        scoreMultiplier = 1;
        isDoubleScoreActive = false;
    }

    public void ResetGame()
    {
        Score = 0;
        ScoreText.text = "Score: " + Score;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        if (Fruits.childCount == 0)
        {
            TotalScore = Score;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void Start()
    {
        Score = TotalScore;
        ScoreText.text = $"Score: {Score}";
    }
}
