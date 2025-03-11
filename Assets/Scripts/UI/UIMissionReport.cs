using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Text.RegularExpressions;
using UnityEngine.InputSystem;

public class UIMissionReport : MonoBehaviour
{
    const float SUCCESS_WAIT = 4f;
    const float FAIL_WAIT = 4f;

    [SerializeField] Image LetterFrame;
    [SerializeField] TMP_Text SuccessText;
    [SerializeField] TMP_Text FailText;
    [SerializeField] Button AcceptButton;
    [SerializeField] TMP_Text WaitingText;
    [SerializeField] Image StampImage;
    [SerializeField] TMP_Text Score;

    string _successText;
    string _failText;
    bool _success;
    string _report;
    bool _writingReport = false;
    bool _skip =  false;


    private readonly Regex spriteRegex = new Regex(@"<sprite name=""[^""]+"">");


    void Awake()
    {
        _successText = SuccessText.text;
        _failText = FailText.text;

        SuccessText.text = string.Empty;
        FailText.text = string.Empty;

        AcceptButton.onClick.AddListener(() => AcceptButtonPressed());
    }

    void Start()
    {
        LetterFrame.enabled = false;
        SuccessText.gameObject.SetActive(false);
        FailText.gameObject.SetActive(false);
        AcceptButton.gameObject.SetActive(false);
        WaitingText.gameObject.SetActive(false);
        StampImage.gameObject.SetActive(false);
        HCGameManager.Instance.State.OnValueChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        HCGameManager.Instance.State.OnValueChanged -= OnGameStateChanged;
    }

    void Update()
    {
        if(_writingReport && (Keyboard.current.spaceKey.isPressed || Mouse.current.leftButton.isPressed))
        {
            _skip = true;
        }
    }

    private void OnGameStateChanged(HCGameManager.GameState previousValue, HCGameManager.GameState newValue)
    {
        if(newValue == HCGameManager.GameState.GAME_DEPARTING_PLANET)
        {
            _success = true;
            StartCoroutine(WaitAndShow());
        }
        else if(newValue == HCGameManager.GameState.GAME_FAILED)
        {
            _success = false;
            StartCoroutine(WaitAndShow());
        }
    }

    IEnumerator WaitAndShow()
    {
        if(_success)
        {
            yield return new WaitForSeconds(SUCCESS_WAIT);
        }
        else
        {
            yield return new WaitForSeconds(FAIL_WAIT);
        }

        ShowReport();
    }

    void ShowReport()
    {
        LetterFrame.enabled = true;
        if(_success)
        {
            FailText.gameObject.SetActive(false);
            SuccessText.gameObject.SetActive(true);
            _report = GetSuccessReport();
        }
        else
        {
            FailText.gameObject.SetActive(true);
            SuccessText.gameObject.SetActive(false);
            _report = GetFailReport();
        }
        StartCoroutine(WriteReport());
    }

    string GetSuccessReport()
    {
        string scommanders = string.Empty;
        int numPlayers = HCNetworkManager.Instance.PlayersConnected();
        int i = 1;
        foreach(PlayerDataNetwork pdn in HCNetworkManager.Instance.GetPlayersData())
        {
            if(scommanders.Length > 0)
            {
                if(i == numPlayers)
                {
                    scommanders += " and ";
                }
                else
                {
                    scommanders += ", ";
                }
            }
            scommanders += pdn.Name;
            i++;
        }

        string planetName = HCGameManager.Instance.CurrentPlanet.Name.ToString();

        int time = (int)HCGameManager.Instance.StageTime.Value;
        int min = time / 60;
        int sec = time % 60;
        string stime = $"{min}m {sec}s";

        string senemies = HCGameManager.Instance.EnemiesKilled.Value.ToString();

        string sresources = string.Empty;
        for(i = 0; i < HCBase.Instance.MaterialsLooted.Count; i++)
        {
            if(HCBase.Instance.MaterialsLooted[i] > 0)
            {
                MaterialData material = GameData.Instance.GetMaterial(i);
                string name = material.Name;
                string emoji = HCEmoji.Get(material.Image);
                sresources += $"{name} {emoji} x{HCBase.Instance.MaterialsLooted[i]}\n";
            }
        }

        return _successText.Replace("COMMANDERS_LIST", scommanders)
                .Replace("PLANET_NAME", planetName)
                .Replace("MISSION_TIME", stime)
                .Replace("ENEMIES_KILLED", senemies)
                .Replace("RESOURCE_LIST", sresources.Trim());
    }

    string GetFailReport()
    {
        string playerList = string.Empty;
        foreach(PlayerDataNetwork pdn in HCNetworkManager.Instance.GetPlayersData())
        {
            playerList += $"{pdn.Name}\n";
        }
        playerList = playerList.Trim();

        string planetName = HCGameManager.Instance.CurrentPlanet.Name.ToString();

        return _failText.Replace("PLAYER_LIST", playerList)
                .Replace("PLANET_NAME", planetName).Trim();
    }
    
    IEnumerator WriteReport()
    {
        _writingReport = true;
        int i = 0;
        while (i < _report.Length)
        {
            if(_skip)
            {
                if (_success)
                {
                    SuccessText.text = _report;
                }
                else
                {
                    FailText.text = _report;
                }
                break;
            }
            // Check for special sprite sequence
            Match match = spriteRegex.Match(_report, i);
            if (match.Success && match.Index == i)
            {
                string spriteTag = match.Value;
                if (_success)
                {
                    SuccessText.text += spriteTag;
                }
                else
                {
                    FailText.text += spriteTag;
                }
                i += spriteTag.Length;
            }
            else
            {
                char c = _report[i];
                if (_success)
                {
                    SuccessText.text += c;
                }
                else
                {
                    FailText.text += c;
                }

                if (c == '\n')
                {
                    yield return new WaitForSeconds(0.3f);
                }
                else
                {
                    yield return new WaitForSeconds(0.01f);
                }
                i++;
            }
        }
        
        _writingReport = false;
        Score.text = ScoreText();
        Stamp();
    }

    string ScoreText()
    {
        if(!_success)
        {
            return "F-";
        }
        else
        {
            int time = (int)HCGameManager.Instance.StageTime.Value;
            if(time < 30)
            {
                return "C-";
            }
            if(time < 60)
            {
                return "C";
            }
            if(time < 90)
            {
                return "C+";
            }
            if(time < 120)
            {
                return "B-";
            }
            if(time < 150)
            {
                return "B";
            }
            if(time < 180)
            {
                return "B+";
            }
            if(time < 210)
            {
                return "A-";
            }
            if(time < 240)
            {
                return "A";
            }
            if(time < 270)
            {
                return "A+";
            }
            return "S";
        }
    }

    void Stamp()
    {
        StampImage.gameObject.SetActive(true);
        StampImage.transform.localScale = Vector3.one * 10f;
        StampImage.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-50f, 50f));

        StampImage.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutQuad).OnComplete(ShowAcceptButton);
    }
    
    void ShowAcceptButton()
    {
        AcceptButton.gameObject.SetActive(true);
    }

    void AcceptButtonPressed()
    {
        if(_success)
        {
            AcceptButton.gameObject.SetActive(false);
            WaitingText.gameObject.SetActive(true);
            HCNetworkManager.Instance.ChangePlayerReadyStatus(true);
        }
        else
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainScene);
        }
    }
}
