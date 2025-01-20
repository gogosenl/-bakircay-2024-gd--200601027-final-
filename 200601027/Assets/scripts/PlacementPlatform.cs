using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

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

    private bool isDoubleScoreActive = false;
    private int scoreMultiplier = 10;

    private Dictionary<string, float> buttonCooldowns = new Dictionary<string, float>();
    private float cooldownTime = 5f;

    void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
        guiStyle.fontSize = 30;

        // 2X Skor Butonu
        if (DrawButtonWithCooldown("2X Skor", new Rect(50, 750, 150, 100), guiStyle))
        {
            isDoubleScoreActive = true;
            scoreMultiplier = 20; // 2X Skor Aktif
        }

        // İpucu Butonu
        if (DrawButtonWithCooldown("İpucu", new Rect(220, 750, 150, 100), guiStyle))
        {
            StartCoroutine(JumpOnePair());
        }

        // Büyüt Butonu
        if (DrawButtonWithCooldown("Büyüt", new Rect(50, 900, 150, 100), guiStyle))
        {
            StartCoroutine(ScaleAllItems(1.5f, 2f));
        }

        // Karıştır Butonu
        if (DrawButtonWithCooldown("Nesneleri Topla", new Rect(220, 900, 230, 100), guiStyle))
        {
            ShuffleAllItems();
        }

        if (GUI.Button(new Rect(700, 50, 230, 50), "Yeniden Başlat", guiStyle))
        {
            ResetGame();
        }
    }

    private bool DrawButtonWithCooldown(string buttonName, Rect rect, GUIStyle guiStyle)
    {
        if (!buttonCooldowns.ContainsKey(buttonName))
        {
            buttonCooldowns[buttonName] = 0f;
        }

        if (buttonCooldowns[buttonName] > Time.time)
        {
            float remainingTime = buttonCooldowns[buttonName] - Time.time;
            GUI.Button(rect, $"{buttonName}\n({Mathf.CeilToInt(remainingTime)})", guiStyle);
            return false;
        }

        if (GUI.Button(rect, buttonName, guiStyle))
        {
            buttonCooldowns[buttonName] = Time.time + cooldownTime;
            return true;
        }

        return false;
    }

    private IEnumerator JumpOnePair()
    {
        MovableItem[] allItems = FindObjectsOfType<MovableItem>();
        var groupedByFruitName = allItems.GroupBy(item => item.FruitName);

        foreach (var group in groupedByFruitName)
        {
            var items = group.ToList();
            if (items.Count >= 2)
            {
                items[0].StartCoroutine(items[0].Jump(1f, 1f));
                items[1].StartCoroutine(items[1].Jump(1f, 1f));
                break;
            }
        }

        yield return null;
    }

    private IEnumerator ScaleAllItems(float scaleFactor, float duration)
    {
        MovableItem[] allItems = FindObjectsOfType<MovableItem>();
        Vector3[] originalScales = new Vector3[allItems.Length];

        for (int i = 0; i < allItems.Length; i++)
        {
            originalScales[i] = allItems[i].transform.localScale;
            allItems[i].transform.localScale = originalScales[i] * scaleFactor;
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < allItems.Length; i++)
        {
            if (allItems[i] != null)
            {
                allItems[i].transform.localScale = originalScales[i];
            }
        }
    }

    private void ShuffleAllItems()
    {
        MovableItem[] allItems = FindObjectsOfType<MovableItem>();
        Vector3 center = new Vector3(-2f, 0f, -3f);
        float radius = 1.5f;

        for (int i = 0; i < allItems.Length; i++)
        {
            float angle = i * Mathf.PI * 2 / allItems.Length;
            float x = center.x + Mathf.Cos(angle) * radius;
            float z = center.z + Mathf.Sin(angle) * radius;
            Vector3 newPos = new Vector3(x, allItems[i].transform.position.y, z);

            if (allItems[i].TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            allItems[i].transform.position = newPos;
        }
    }

    public void AddScore()
    {
        Score += 1 * scoreMultiplier;
        ScoreText.text = $"Score: {Score}";

        matchCount++;

        if (matchCount == totalPairs)
        {
            TotalScore = Score;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        scoreMultiplier = 10;
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
