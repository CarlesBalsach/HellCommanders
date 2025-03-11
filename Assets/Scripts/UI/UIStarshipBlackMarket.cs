using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIStarshipBlackMarket : MonoBehaviour
{
    public event Action OnClose;

    [SerializeField] UIMaterialBlackMarket MaterialBMPrefab;
    [SerializeField] Button BackButton;

    void Start()
    {
        MaterialBMPrefab.gameObject.SetActive(false);
        foreach(MaterialData material in GameData.Instance.Materials)
        {
            UIMaterialBlackMarket mbm = Instantiate(MaterialBMPrefab, MaterialBMPrefab.transform.parent);
            mbm.gameObject.SetActive(true);
            mbm.Init(material);
        }

        BackButton.onClick.AddListener(() => {
            OnClose?.Invoke();
        });
    }
}
