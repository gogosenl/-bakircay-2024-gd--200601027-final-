using System;
using TMPro;
using UnityEngine;

public class MovableItem : MonoBehaviour
{
    public string FruitName;
    public float backduration = 3f;
    public PlacementPlatform _sp;
    public float height = 1.5f; // Geri dönüş yay yüksekliği
    public Animator myAnimator;

    [Header("Drag & Throw Settings")]
    public float flingMaxSpeed = 0.5f;       // Bıraktıktan sonraki maksimum hız
    public float backForceMultiplier = 0.3f; // Geri fırlatma gücü çarpanı (mismatch durumunda)

    [Header("Game Area Bounds")]
    public Vector3 minBoundary = new Vector3(-8.26f, -3.80f, -7.20f);
    public Vector3 maxBoundary = new Vector3(6.56f, -3.00f, 3.40f);

    public ParticleSystem dragEffect;

    private Vector3 startposition;
    private Vector3 fallposition;
    private Rigidbody rb;
    private Camera mainCamera;

    private bool isDragging = false;
    private float elapsedTime = 0;
    private bool isBack = false;

    // Sabit bir Y düzleminde sürüklemek için plane
    private Plane dragPlane;
    private float initialY;

    private Vector3 velocityBeforeKinematic = Vector3.zero;

    private void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();

        fallposition = transform.position;
        initialY = transform.position.y;
        startposition = transform.position;

        // Nesneyi sabit bir Y düzleminde sürüklemek için Plane:
        dragPlane = new Plane(Vector3.up, new Vector3(0, initialY, 0));
    }

    private void Update()
    {
        if (isBack)
        {
            // “isBack” durumunda nesneyi bir yay eğrisi ile geri taşıyoruz:
            if (elapsedTime < backduration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / backduration;

                // Sadece x-z ekseninde lineer interpolasyon
                Vector3 horizontalPosition = Vector3.Lerp(transform.position, startposition, t);

                // Y ekseninde küçük bir yay oluştur
                float arc = Mathf.Sin(t * Mathf.PI) * height;

                transform.position = new Vector3(
                    horizontalPosition.x,
                    horizontalPosition.y + arc,
                    horizontalPosition.z
                );
            }
            else
            {
                // Yay hareketi bittiğinde tekrar fizik kontrolü ver
                rb.isKinematic = false;
                // Saklanan velocity'yi geri ver (çarpan uygulanmış hâlini)
                rb.linearVelocity = velocityBeforeKinematic;

                isBack = false;
                elapsedTime = 0;
            }
        }
    }

    private void OnMouseDown()
    {
        isDragging = true;
        rb.isKinematic = true;
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            // Kamera ekranındaki mouse konumundan Ray
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                // Ray'in düzlemle kesiştiği noktayı bulalım
                Vector3 hitPoint = ray.GetPoint(enter);

                // Y sabit olsun (plane zaten y=initialY diyor, ama yine de set edebiliriz)
                hitPoint.y = initialY;

                // Oyun alanı sınırlarını uygula (x,z)
                // İstersen bu "1f, 1f, 2f" offsetlerini kaldırabilir ya da değiştirebilirsin
                Vector3 adjustedMinBoundary = minBoundary + new Vector3(1f, 1f, 2f);
                Vector3 adjustedMaxBoundary = maxBoundary - new Vector3(1f, 1f, 2f);

                hitPoint.x = Mathf.Clamp(hitPoint.x, adjustedMinBoundary.x, adjustedMaxBoundary.x);
                hitPoint.z = Mathf.Clamp(hitPoint.z, adjustedMinBoundary.z, adjustedMaxBoundary.z);

                // Kinematik RB'nin MovePosition’u ile nesneyi taşı
                rb.MovePosition(hitPoint);

                // Sürükleme efekti
                if (dragEffect && !dragEffect.isPlaying)
                {
                    dragEffect.Play();
                }
            }
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        rb.isKinematic = false;

        // Bıraktıktan sonra hızını sınırla
        Vector3 newVelocity = rb.linearVelocity;
        if (newVelocity.magnitude > flingMaxSpeed)
        {
            newVelocity = newVelocity.normalized * flingMaxSpeed;
        }
        rb.linearVelocity = newVelocity;

        // Sürükleme efekti
        if (dragEffect && dragEffect.isPlaying)
        {
            dragEffect.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Placement Area")
        {
            _sp = other.GetComponent<PlacementPlatform>();

            if (_sp.CurrentFruit == null)
            {
                _sp.CurrentFruit = this;
            }
            else if (_sp.CurrentFruit != this && _sp.CurrentFruit.FruitName == this.FruitName)
            {
                // 🟢 Eşleşme Durumu
                if (myAnimator != null)
                    myAnimator.SetTrigger("OnMatch");

                if (_sp.CurrentFruit.myAnimator != null)
                    _sp.CurrentFruit.myAnimator.SetTrigger("OnMatch");

                rb.isKinematic = false;
                gameObject.layer = 6;

                _sp.CurrentFruit.rb.isKinematic = false;
                _sp.CurrentFruit.gameObject.layer = 6;

                _sp.CurrentFruit = null;

                _sp.AddScore();

                // Eşleşme efekti
                if (_sp.matchParticleEffect)
                {
                    ParticleSystem effect = Instantiate(_sp.matchParticleEffect, transform.position, Quaternion.identity);
                    effect.Play();
                    Destroy(effect.gameObject, 2f);
                }

                // Meyveleri yok et
                Destroy(gameObject);
                Destroy(_sp.CurrentFruit.gameObject);
            }
            else if (_sp.CurrentFruit != this && _sp.CurrentFruit.FruitName != this.FruitName)
            {
                // ❌ Eşleşme yoksa geri git
                isBack = true;
                velocityBeforeKinematic = rb.linearVelocity * backForceMultiplier;
                rb.isKinematic = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_sp != null && _sp.CurrentFruit == this)
        {
            _sp.CurrentFruit = null;
        }
    }
}
