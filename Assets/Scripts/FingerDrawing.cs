using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FingerDrawing : MonoBehaviour
{
    [SerializeField] private RawImage displayImage;
    [SerializeField] private HandWritternDigit classifier;
    [SerializeField] private Transform fingerTipTransform;
    [SerializeField] private float delayToSend = 1f;


    private bool hasDrawn = false;
    private float lastDrawTime;
    private Camera mainCamera;
    private Texture2D drawingTexture;
    private Coroutine checkForSend;


    [SerializeField] float distanceToCanvas;
    [SerializeField] TextMeshProUGUI TMP_Result;

    private void OnEnable()
    {
        HandWritternDigit.OnResult += OnResultHandle;
    }

    private void OnDestroy()
    {
        HandWritternDigit.OnResult -= OnResultHandle;
    }

    private void Start()
    {
        drawingTexture = new Texture2D(28, 28, TextureFormat.RGBA32, false);
        displayImage.texture = drawingTexture;
        mainCamera = Camera.main;
        ClearTexture();
    }

    public void ClearTexture()
    {
        Color[] clearColor = new Color[drawingTexture.width * drawingTexture.height];
        for (int i = 0; i < clearColor.Length; i++)
        {
            clearColor[i] = Color.black;
        }
        drawingTexture.SetPixels(clearColor);
        drawingTexture.Apply();
    }

    private void Update()
    {
        bool isDrawing = Vector3.Distance(fingerTipTransform.position, displayImage.transform.position) < distanceToCanvas;
        Debug.Log("isDrawing: "+isDrawing);
        if(isDrawing)
        {
            if(checkForSend != null)
            {
                StopCoroutine(checkForSend);
                checkForSend = null;
            }
            Draw(fingerTipTransform.position);
            hasDrawn = true;
            lastDrawTime = Time.time;
        }
        else if(hasDrawn && Time.time - lastDrawTime > delayToSend && checkForSend == null)
        {
            Debug.Log("ExecuteModel");
            checkForSend = StartCoroutine(CheckForSend());
        }
    }

    private void Draw(Vector3 fingerTipPos)
    {
        Debug.Log("Draw");
        Vector2 screenPoint = mainCamera.WorldToScreenPoint(fingerTipPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(displayImage.rectTransform, screenPoint, mainCamera, out Vector2 localPoint);
        Vector2 normalizePoint = Rect.PointToNormalized(displayImage.rectTransform.rect, localPoint);
        AddPixels(normalizePoint);
    }

    private void AddPixels(Vector2 normalizePoint)
    {
        int texX = (int)(normalizePoint.x * drawingTexture.width);
        int texY = (int)(normalizePoint.y * drawingTexture.height);


      if (texX >= 0 && texX < drawingTexture.width && texY >= 0 && texY < drawingTexture.height)
      {
            drawingTexture.SetPixel(texX, texY, Color.white);
            drawingTexture.Apply();

       }
    }

    private IEnumerator CheckForSend()
    {
        yield return new WaitForSeconds(delayToSend);
        classifier.ExecuteModel(drawingTexture);
        hasDrawn = false;
        checkForSend = null;
    }

    private void OnResultHandle(int obj)
    {
        TMP_Result.text = $"You write: <color=green>{obj}</color>";
    }
}   

