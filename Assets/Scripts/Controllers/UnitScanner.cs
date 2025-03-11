using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScanner : MonoBehaviour
{
    const int CREATE_TEXTURE_SIZE = 256;

    public static UnitScanner Instance = null;

    [SerializeField] SpriteRenderer FocusSprite;

    HCUnit _focusUnit;

    public event Action<HCUnit, Sprite> OnFocusUnit;
    public event Action OnDefocusUnit;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    void Update()
    {
        if(HCGameManager.Instance.State.Value == HCGameManager.GameState.GAME_ACTIVE)
        {
            FindUnitToScan();
            SetScanFocus();
        }
        else
        {
            FocusSprite.gameObject.SetActive(false);
        }
    }

    void FindUnitToScan()
    {
        Vector3 focusPoint = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0f);
        focusPoint.y += Camera.main.orthographicSize * 2f * 0.1f;

        float max_distance = Camera.main.orthographicSize * 2f * 0.1f;
        HCUnit focusUnit = null;
        float min_distance = max_distance;
        for(int i = 0; i < HCMap.Instance.UnitsSpawned.Count; i++)
        {
            HCUnit unit = HCMap.Instance.UnitsSpawned[i];
            if(!unit.IsDead())
            {
                float distance = Vector2.Distance(focusPoint, unit.transform.position);
                if(distance <= max_distance)
                {
                    if(distance < min_distance)
                    {
                        focusUnit = unit;
                        min_distance = distance;
                    }
                }
            }
        }

        if(focusUnit != _focusUnit)
        {
            ScanUnit(focusUnit);
        }
    }

    void ScanUnit(HCUnit unit)
    {
        if(unit != null)
        {
            _focusUnit = unit;
            OnFocusUnit?.Invoke(unit, GetUnitSprite(unit));
        }
        if(unit == null && _focusUnit != null)
        {
            _focusUnit = null;
            OnDefocusUnit?.Invoke();
        }
    }

    void SetScanFocus()
    {
        if(_focusUnit == null)
        {
            FocusSprite.gameObject.SetActive(false);
        }
        else
        {
            FocusSprite.gameObject.SetActive(true);
            FocusSprite.transform.position = _focusUnit.transform.position;
            float size = _focusUnit.Stats.Radius * 2f + 0.1f;
            FocusSprite.size = new Vector2(size, size);
        }
    }

    public Sprite GetUnitSprite(HCUnit unit)
    {
        if(unit.Stats.Name == "Base")
        {
            return PrefabController.Instance.BaseSprite;
        }
        return CaptureSpriteTexture(unit.GetUnitSprite());
    }

    public Sprite CaptureSpriteTexture(SpriteRenderer spriteRenderer)
    {
        // Step 1: Clone the GameObject
        GameObject spriteObject = Instantiate(spriteRenderer.gameObject);
        spriteObject.layer = LayerMask.NameToLayer("RenderOnly");
        SetLayerRecursively(spriteObject, LayerMask.NameToLayer("RenderOnly"));

        // Step 2: Calculate bounding box
        Bounds bounds = GetBounds(spriteObject);
        float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y);

        // Optional Step: Apply Rotation
        spriteObject.transform.rotation = spriteRenderer.transform.rotation;

        // Step 3: Create a RenderTexture
        RenderTexture renderTexture = new RenderTexture(CREATE_TEXTURE_SIZE, CREATE_TEXTURE_SIZE, 24);

        // Step 4: Configure and position the camera
        GameObject cameraGameObject = new GameObject("TempCamera");
        Camera tempCamera = cameraGameObject.AddComponent<Camera>();
        tempCamera.backgroundColor = Color.clear;
        tempCamera.clearFlags = CameraClearFlags.SolidColor;
        tempCamera.orthographic = true;
        tempCamera.orthographicSize = maxDimension / 2;
        tempCamera.aspect = 1f;
        tempCamera.cullingMask = 1 << LayerMask.NameToLayer("RenderOnly");
        tempCamera.targetTexture = renderTexture;
        tempCamera.transform.position = bounds.center + Vector3.back * 10;

        // Step 5: Render the sprite to the RenderTexture
        tempCamera.Render();

        // Step 6: Convert RenderTexture to Texture2D
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        // Cleanup
        RenderTexture.active = null;
        Destroy(tempCamera.gameObject);
        Destroy(spriteObject);

        return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private Bounds GetBounds(GameObject obj)
    {
        Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
