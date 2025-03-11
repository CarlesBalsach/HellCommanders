using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;
using System;

public class UIStarshipQuips : MonoBehaviour
{
    public event Action OnClose;

    [SerializeField] UIQuip OwnedQuipPrefab;
    [SerializeField] UIQuipStore StoreQuipPrefab;
    [SerializeField] UIQuipInfo QuipInfo;
    [SerializeField] TMP_Text OnwedQuipsText;
    [SerializeField] Button BackButton;

    List<UIQuip> _ownedQuips = new List<UIQuip>();
    List<UIQuipStore> _storeQuips = new List<UIQuipStore>();

    private void Awake()
    {
        BackButton.onClick.AddListener(()=>{
            OnClose?.Invoke();
        });
    }

    void Start()
    {
        OwnedQuipPrefab.gameObject.SetActive(false);
        StoreQuipPrefab.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        HCPlayer.LocalInstance.Quips.OnListChanged += OnQuipsChanged;
        RefreshQuips();
    }

    void OnDisable()
    {
        HCPlayer.LocalInstance.Quips.OnListChanged -= OnQuipsChanged;
    }

    private void OnQuipsChanged(NetworkListEvent<int> changeEvent)
    {
        RefreshQuips();
    }

    void RefreshQuips()
    {
        for(int i = _ownedQuips.Count - 1; i >= 0; i--)
        {
            Destroy(_ownedQuips[i].gameObject);
            _ownedQuips.RemoveAt(i);
        }

        for(int i = _storeQuips.Count - 1; i >= 0; i--)
        {
            Destroy(_storeQuips[i].gameObject);
            _storeQuips.RemoveAt(i);
        }

        foreach(int quipId in HCPlayer.LocalInstance.Quips)
        {
            UIQuip quip = Instantiate(OwnedQuipPrefab, OwnedQuipPrefab.transform.parent);
            quip.gameObject.SetActive(true);
            quip.Init(GameData.Instance.GetQuipData(quipId));
            quip.OnQuipClicked += OwnedQuipClicked;
            _ownedQuips.Add(quip);
        }

        foreach(QuipData quip in GameData.Instance.Quips)
        {
            bool owned = HCPlayer.LocalInstance.Quips.Contains(quip.Id);
            if(quip.Id > 0 && !owned)
            {
                UIQuipStore quipStore = Instantiate(StoreQuipPrefab, StoreQuipPrefab.transform.parent);
                quipStore.gameObject.SetActive(true);
                bool affordable = HCPlayer.LocalInstance.CanPayMaterialPrice(quip.Price);
                quipStore.Init(quip, affordable);
                quipStore.OnQuipClicked += StoreQuipClicked;
                _storeQuips.Add(quipStore);
            }
        }

        OnwedQuipsText.text = $"Installed Quips\n{HCPlayer.LocalInstance.Quips.Count} / 12";
        QuipInfo.gameObject.SetActive(false);
    }

    void OwnedQuipClicked(QuipData quip)
    {
        QuipInfo.gameObject.SetActive(true);
        QuipInfo.Init(quip, true);
    }

    void StoreQuipClicked(QuipData quip)
    {
        QuipInfo.gameObject.SetActive(true);
        QuipInfo.Init(quip, false);
    }
}
