using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuipActivationVisual : MonoBehaviour
{
    [SerializeField] SpriteRenderer MainRadius;
    [SerializeField] SpriteRenderer SecondaryRadius;
    [SerializeField] GameObject AoE;
    [SerializeField] SpriteRenderer BuildingSize;
    [SerializeField] LineRenderer LinePrefab;

    bool _activatingQuip = false;
    QuipData _quip;

    void Start()
    {
        QuipInputController.Instance.OnQuipActivated += OnQuipActivated;
        QuipInputController.Instance.OnQuipLaunched += OnQuipLaunched;
        QuipInputController.Instance.OnQuipActivationCancelled += OnQuipActivationCancelled;
    }

    void OnDestroy()
    {
        if(QuipInputController.Instance != null)
        {
            QuipInputController.Instance.OnQuipActivated -= OnQuipActivated;
            QuipInputController.Instance.OnQuipLaunched -= OnQuipLaunched;
            QuipInputController.Instance.OnQuipActivationCancelled -= OnQuipActivationCancelled;
        }
    }

    private void Update()
    {
        if(_activatingQuip)
        {
            CheckMouseDelta();
        }
    }

    void CheckMouseDelta()
    {
        Vector2 delta = Mouse.current.delta.value;
        if(delta != Vector2.zero)
        {
            delta *= HCGameCamera.MOUSE_TO_WORLD;
            Vector3 pos = transform.position;
            pos.x += delta.x;
            pos.y += delta.y;
            pos.z = 0;
            SetQuipPosition(pos);
        }
    }

    void SetDefaultPosition()
    {
        float deltaY = Camera.main.orthographicSize * 2f * 0.1f;
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y + deltaY, 0f);
    }

    void SetQuipPosition(Vector3 pos)
    {
        float quipSize = Mathf.Max(_quip.MainRadius, _quip.SecondaryRadius, _quip.AreaOfEffect, _quip.Size);
        pos.x = Mathf.Clamp(pos.x, -HCMap.SIZE /2f + quipSize, HCMap.SIZE/2f - quipSize);
        pos.y = Mathf.Clamp(pos.y, -HCMap.SIZE /2f + quipSize, HCMap.SIZE/2f - quipSize);
        transform.position = pos;
    }

    private void OnQuipActivated(QuipData quip)
    {
        if(quip.Id != 0) // If Leaving Planet, Leave Immidieatly
        {
            _activatingQuip = true;
            SetDefaultPosition();
            HCGameCamera.Instance.SetTransformToFollow(transform);
            SetQuipVisials(quip);
        }
    }

    void OnQuipLaunched(QuipData quip)
    {
        _activatingQuip = false;
        HCGameCamera.Instance.SetTransformToFollow(null);
        ResetVisuals();
    }

    void OnQuipActivationCancelled()
    {
        _activatingQuip = false;
        HCGameCamera.Instance.SetTransformToFollow(null);
        ResetVisuals();
    }

    void SetQuipVisials(QuipData quip)
    {
        ResetVisuals();
        _quip = quip;
        
        if(quip.MainRadius > 0f)
        {
            MainRadius.gameObject.SetActive(true);
            MainRadius.transform.localScale = Vector3.one * quip.MainRadius * 2f;
        }

        if(quip.SecondaryRadius > 0f)
        {
            SecondaryRadius.gameObject.SetActive(true);
            SecondaryRadius.transform.localScale = Vector3.one * quip.SecondaryRadius * 2f;
        }

        if(quip.AreaOfEffect > 0f)
        {
            CreateDottedCircle(quip.AreaOfEffect);
        }

        if(quip.Size > 0f)
        {
            BuildingSize.gameObject.SetActive(true);
            BuildingSize.transform.localScale = Vector3.one * quip.Size;
        }

        if(quip.Range > 0f)
        {
            CreateDottedCircle(quip.Range);
        }
        if(quip.MinRange > 0f)
        {
            CreateDottedCircle(quip.MinRange);
        }
    }

    void CreateDottedCircle(float radius, float segmentLength = 0.5f)
    {
        int segmentCount = Mathf.RoundToInt(2 * Mathf.PI * radius / segmentLength);
        float angleIncrement = 360f / segmentCount;
        bool segmentWithLine = true;

        for (int i = 0; i < segmentCount; i++)
        {
            if(segmentWithLine)
            {
                // Calculate start and end points for each segment
                float angleRadians = (angleIncrement * i) * Mathf.Deg2Rad;
                Vector3 startPoint = new Vector3(radius * Mathf.Cos(angleRadians), radius * Mathf.Sin(angleRadians), 0f);
                angleRadians = (angleIncrement * (i + 1)) * Mathf.Deg2Rad;
                Vector3 endPoint = new Vector3(radius * Mathf.Cos(angleRadians), radius * Mathf.Sin(angleRadians), 0f);

                // Instantiate the line segment prefab
                GameObject lineSegment = Instantiate(LinePrefab.gameObject, AoE.transform.position, Quaternion.identity, AoE.transform) as GameObject;
                lineSegment.gameObject.SetActive(true);
                LineRenderer lr = lineSegment.GetComponent<LineRenderer>();

                // Adjust LineRenderer to draw the curved segment
                lr.positionCount = 2;
                lr.SetPosition(0, startPoint);
                lr.SetPosition(1, endPoint);
            }
            segmentWithLine = !segmentWithLine;
        }
    }

    void ResetVisuals()
    {
        _quip = null;
        MainRadius.gameObject.SetActive(false);
        SecondaryRadius.gameObject.SetActive(false);
        HCUtils.DestroyAllChildren(AoE.transform);
        BuildingSize.gameObject.SetActive(false);
        
        for(int i = AoE.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(AoE.transform.GetChild(i).gameObject);
        }
    }
}
