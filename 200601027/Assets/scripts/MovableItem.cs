using System;
using TMPro;
using UnityEngine;

public class MovableItem : MonoBehaviour
{
    public string FruitName;
    public float backduration = 3f;
    public PlacementPlatform _sp;
    public float height = 1.5f;
    public Animator myAnimator;

    [Header("Drag & Throw Settings")]
    public float flingMaxSpeed = 0.5f;
    public float backForceMultiplier = 0.3f;

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

        dragPlane = new Plane(Vector3.up, new Vector3(0, initialY, 0));
    }

    private void Update()
    {
        if (isBack)
        {
            if (elapsedTime < backduration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / backduration;
                Vector3 horizontalPosition = Vector3.Lerp(transform.position, startposition, t);
                float arc = Mathf.Sin(t * Mathf.PI) * height;
                transform.position = new Vector3(
                    horizontalPosition.x,
                    horizontalPosition.y + arc,
                    horizontalPosition.z
                );
            }
            else
            {
                rb.isKinematic = false;
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
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                hitPoint.y = initialY;

                Vector3 adjustedMinBoundary = minBoundary + new Vector3(1f, 1f, 2f);
                Vector3 adjustedMaxBoundary = maxBoundary - new Vector3(1f, 1f, 2f);

                hitPoint.x = Mathf.Clamp(hitPoint.x, adjustedMinBoundary.x, adjustedMaxBoundary.x);
                hitPoint.z = Mathf.Clamp(hitPoint.z, adjustedMinBoundary.z, adjustedMaxBoundary.z);

                rb.MovePosition(hitPoint);

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

        Vector3 newVelocity = rb.linearVelocity;
        if (newVelocity.magnitude > flingMaxSpeed)
        {
            newVelocity = newVelocity.normalized * flingMaxSpeed;
        }
        rb.linearVelocity = newVelocity;

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
                // Eşleşme
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

                if (_sp.matchParticleEffect)
                {
                    ParticleSystem effect = Instantiate(_sp.matchParticleEffect, transform.position, Quaternion.identity);
                    effect.Play();
                    Destroy(effect.gameObject, 2f);
                }

                // Meyveleri yok et
                Destroy(gameObject);
            }
            else if (_sp.CurrentFruit != this && _sp.CurrentFruit.FruitName != this.FruitName)
            {
                // Eşleşme yoksa geri git
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

    /// <summary>
    /// Nesneyi sin dalgası kullanarak kısa bir zıplama hareketi yaptırır (örnek).
    /// </summary>
    public System.Collections.IEnumerator Jump(float jumpHeight, float totalTime)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / totalTime;

            float arc = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            transform.position = new Vector3(
                startPos.x,
                startPos.y + arc,
                startPos.z
            );

            yield return null;
        }

        transform.position = startPos;
    }
}
