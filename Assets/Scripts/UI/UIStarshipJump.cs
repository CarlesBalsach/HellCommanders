using System;
using System.Collections;
using System.Collections.Generic;
using QFSW.QC.Actions;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIStarshipJump : MonoBehaviour
{
    public event Action OnClose;

    [SerializeField] Image Starship;
    [SerializeField] UIPlanet CurrentPlanet;
    [SerializeField] List<UIPlanet> PlanetChoices;
    [SerializeField] Button JumpButton;
    [SerializeField] TMP_Text WaitingForPlayersText;
    [SerializeField] TMP_Text WaitingHostChoice;
    [SerializeField] Button BackButton;
    [SerializeField] TMP_Text CurrentLayer;
    [SerializeField] TMP_Text NextLayer;
    [SerializeField] UIFloatingText FloatingText;

    Vector3 StarshipOrbit;

    private void Awake()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            PlanetChoices[0].GetComponent<Button>().onClick.AddListener(() => PlanetChosen(0));
            PlanetChoices[1].GetComponent<Button>().onClick.AddListener(() => PlanetChosen(1));
            PlanetChoices[2].GetComponent<Button>().onClick.AddListener(() => PlanetChosen(2));

            WaitingHostChoice.text = "Select a Planet for your next Mission";
        }

        JumpButton.onClick.AddListener(() => {
            HCNetworkManager.Instance.ChangePlayerReadyStatus(true);
        });

        BackButton.onClick.AddListener(() => {
            OnClose?.Invoke();
        });
    }

    void Start()
    {
        StarshipOrbit = Starship.transform.position - CurrentPlanet.transform.position;
    }

    private void OnEnable()
    {
        HCNetworkManager.Instance.OnPlayerDataChanged += OnPlayerDataChanged;
        HCGameManager.Instance.PlanetChoice.OnValueChanged += OnPlanetChoiceChanged;

        CurrentPlanet.OnFocused += OnPlanetFocused;
        CurrentPlanet.OnDefocused += OnPlanetDefocused;
        foreach(UIPlanet planet in PlanetChoices)
        {
            planet.OnFocused += OnPlanetFocused;
            planet.OnDefocused += OnPlanetDefocused;
        }
    }

    private void OnDisable()
    {
        HCNetworkManager.Instance.OnPlayerDataChanged -= OnPlayerDataChanged;
        HCGameManager.Instance.PlanetChoice.OnValueChanged -= OnPlanetChoiceChanged;

        CurrentPlanet.OnFocused -= OnPlanetFocused;
        CurrentPlanet.OnDefocused -= OnPlanetDefocused;
        foreach(UIPlanet planet in PlanetChoices)
        {
            planet.OnFocused -= OnPlanetFocused;
            planet.OnDefocused -= OnPlanetDefocused;
        }
    }

    private void Update()
    {
        float angle = (Time.time * 5) % 360f;
        Vector3 starshipPos = HCUtils.RotateVector2(StarshipOrbit, angle);
        Starship.transform.position = CurrentPlanet.transform.position + starshipPos;

        Starship.transform.rotation = Quaternion.Euler(Vector3.forward * Time.time);
    }

    public void Init()
    {
        CurrentPlanet.Init(HCGameManager.Instance.CurrentPlanet.Value);
        for(int i = 0; i < PlanetChoices.Count; i++)
        {
            PlanetChoices[i].Init(HCGameManager.Instance.PlanetChoices[i]);
        }

        int planetSelected = HCGameManager.Instance.PlanetChoice.Value;
        JumpButton.gameObject.SetActive(planetSelected >= 0);
        WaitingHostChoice.gameObject.SetActive(planetSelected == -1);
        AdjustByReadyState();


        if(planetSelected >= 0)
        {
            SetPlanetSelected(planetSelected);
        }

        int layer = HCGameManager.Instance.Layer.Value;
        CurrentLayer.text = $"Layer {layer}";
        NextLayer.text = $"Layer {layer+1}";

        FloatingText.Hide();
    }

    private void OnPlanetChoiceChanged(int previousValue, int newValue)
    {
        SetPlanetSelected(newValue);
        JumpButton.gameObject.SetActive(newValue >= 0);
        WaitingHostChoice.gameObject.SetActive(HCGameManager.Instance.PlanetChoice.Value == -1);
    }

    private void OnPlayerDataChanged()
    {
        AdjustByReadyState();
    }
    
    void SetPlanetSelected(int planet)
    {
        for(int i = 0; i < PlanetChoices.Count; i++)
        {
            List<Image> imagesInPlanet = HCUtils.FindComponentsRecursively<Image>(PlanetChoices[i].transform);
            foreach(Image image in imagesInPlanet)
            {
                Color c = image.color;
                if(i == planet)
                {
                    c.a = 1f;
                }
                else
                {
                    c.a = 0.3f;
                }
                image.color = c;
            }

            if(i == planet)
            {
                PlanetChoices[i].SetSelected(true);
            }
            else
            {
                PlanetChoices[i].SetSelected(false);
            }
        }
    }

    void PlanetChosen(int planetId) // Host Only
    {
        HCGameManager.Instance.PlanetChoice.Value = planetId;
    }

    void AdjustByReadyState()
    {
        bool playerReady = HCNetworkManager.Instance.GetPlayerData().Ready;
        if(playerReady)
        {
            JumpButton.gameObject.SetActive(false);
            WaitingForPlayersText.gameObject.SetActive(true);
        }
        else
        {
            WaitingForPlayersText.gameObject.SetActive(false);
        }
    }
    
    string GetPlanetInfo(PlanetDataNetwork data)
    {
        string text = data.Name.ToString();
        text += "\n-------------\n";
        for(int i = 0; i < data.Materials.Length; i++)
        {
            if(data.Materials[i])
            {
                MaterialData material = GameData.Instance.GetMaterial(i);
                text += $"{HCEmoji.Get(material.Image)} {material.Name}\n";
            }
        }
        return text.Trim();
    }

    void OnPlanetFocused(Image planet, PlanetDataNetwork planetData)
    {
        string info = GetPlanetInfo(planetData);
        FloatingText.Show(planet.GetComponent<RectTransform>(), info, true, false);
    }

    void OnPlanetDefocused()
    {
        FloatingText.Hide();
    }
}
