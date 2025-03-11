using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBackground : MonoBehaviour
{
    const float BORDER_LINE_WIDTH = 0.1f;

    [SerializeField] SpriteRenderer MapBackgroundSprite;
    [SerializeField] LineRenderer MapBorderLines;
    [SerializeField] MapTile MapTilePrefab;

    void Start()
    {
        MapBackgroundSprite.size = new Vector2(HCMap.SIZE, HCMap.SIZE);
        MapBorderLines.startWidth = BORDER_LINE_WIDTH;
        MapBorderLines.endWidth = BORDER_LINE_WIDTH;
        MapBorderLines.SetPositions(new Vector3[] {
            new Vector3(-HCMap.SIZE/2f - BORDER_LINE_WIDTH/2f, HCMap.SIZE/2f + BORDER_LINE_WIDTH/2f, 0f),
            new Vector3(HCMap.SIZE/2f + BORDER_LINE_WIDTH/2f, HCMap.SIZE/2f + BORDER_LINE_WIDTH/2f, 0f),
            new Vector3(HCMap.SIZE/2f + BORDER_LINE_WIDTH/2f, -HCMap.SIZE/2f - BORDER_LINE_WIDTH/2f, 0f),
            new Vector3(-HCMap.SIZE/2f - BORDER_LINE_WIDTH/2f, -HCMap.SIZE/2f - BORDER_LINE_WIDTH/2f, 0f),
            new Vector3(-HCMap.SIZE/2f - BORDER_LINE_WIDTH/2f, HCMap.SIZE/2f + BORDER_LINE_WIDTH, 0f),
        });

        for(int i = 0; i < HCMap.SIZE; i++)
        {
            for(int j = 0; j < HCMap.SIZE; j++)
            {
                MapTile tile = Instantiate(MapTilePrefab, transform);
                tile.gameObject.SetActive(true);
                tile.transform.position = new Vector3(-HCMap.SIZE / 2f + i + 0.5f, -HCMap.SIZE / 2f + j + 0.5f, 0f);
            }
        }
    }

}
