using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField] Sprite[] MapTiles;

    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = MapTiles[Random.Range(0, MapTiles.Length)];
        int rotation = Random.Range(0,4);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0f, 90f * rotation));
    }
}
