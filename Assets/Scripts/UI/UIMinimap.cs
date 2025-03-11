using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMinimap : MonoBehaviour
{
    [SerializeField] Canvas Canvas;
    [SerializeField] RectTransform MinimapRT;
    [SerializeField] Image CameraFrameImage;
    [SerializeField] Image UnitImagePrefab;

    List<Image> _unitImages = new List<Image>();

    void Update()
    {
        if(HCGameManager.Instance.State.Value == HCGameManager.GameState.GAME_ACTIVE)
        {
            UpdateCameraFrameImage();
            PositionCameraFrameImage();
            UpdateUnits();
        }
    }

    void UpdateCameraFrameImage()
    {
        float height = Camera.main.orthographicSize * 2f * 0.8f;
        float width = Camera.main.orthographicSize * Camera.main.aspect * 2f * 0.6f;

        Vector2 minimapSize = new Vector2(MinimapRT.rect.width, MinimapRT.rect.height);
        Vector2 frameSize = new Vector2(width * minimapSize.x / HCMap.SIZE, height * minimapSize.y / HCMap.SIZE);
        CameraFrameImage.rectTransform.sizeDelta = frameSize;
    }

    void PositionCameraFrameImage()
    {
        Vector3 basePos = MinimapRT.transform.position;
        Vector2 minimapSize = new Vector2(MinimapRT.rect.width, MinimapRT.rect.height);

        float posX = Camera.main.transform.position.x * minimapSize.x / HCMap.SIZE * Canvas.scaleFactor;
        float posY = Camera.main.transform.position.y * minimapSize.y / HCMap.SIZE * Canvas.scaleFactor;
        posY += Camera.main.orthographicSize * 0.2f * minimapSize.y / HCMap.SIZE * Canvas.scaleFactor;
        CameraFrameImage.transform.position = basePos + new Vector3(posX, posY, 0f);
    }

    void UpdateUnits()
    {
        _unitImages.ForEach(unit => unit.gameObject.SetActive(false));

        int uic = 0;

        for (int i = 0; i < HCMap.Instance.UnitsSpawned.Count; i++)
        {
            HCUnit unit = HCMap.Instance.UnitsSpawned[i];
            if(!unit.IsDead())
            {
                if(uic >= _unitImages.Count)
                {
                    Image unitImage = Instantiate(UnitImagePrefab);
                    unitImage.transform.SetParent(MinimapRT, false);
                    _unitImages.Add(unitImage);
                    unitImage.gameObject.SetActive(true);
                    UpdateUnitImage(HCMap.Instance.UnitsSpawned[i], unitImage);
                }
                else
                {
                    _unitImages[uic].gameObject.SetActive(true);
                    UpdateUnitImage(unit, _unitImages[uic]);
                }
                uic++;
            }
        }
    }

    void UpdateUnitImage(HCUnit unit, Image unitImage)
    {
        float unitSize = unit.Stats.Radius * 2f;
        float minimapSize = MinimapRT.rect.width;
        float imageSize = minimapSize * unitSize / HCMap.SIZE;
        unitImage.GetComponent<RectTransform>().sizeDelta = new Vector2(imageSize, imageSize);

        Vector3 basePos = MinimapRT.transform.position;
        float posX = unit.transform.position.x * minimapSize / HCMap.SIZE * Canvas.scaleFactor;
        float posY = unit.transform.position.y * minimapSize / HCMap.SIZE * Canvas.scaleFactor;
        unitImage.transform.position = basePos + new Vector3(posX, posY, 0f);

        if(unit.IsEnemy)
        {
            unitImage.color = HCColor.EnemyColor;
        }
        else
        {
            unitImage.color = HCColor.AllyColor;
        }
    }
}
