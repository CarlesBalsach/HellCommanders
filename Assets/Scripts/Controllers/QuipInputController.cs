using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuipInputController : MonoBehaviour
{
    public const float MISS_PENALTY = 1f;
    public static QuipInputController Instance;

    [SerializeField] QuipActivationVisual QuipVisualPosition;
    [SerializeField] UIGamePlayer GamePlayerUI;

    public event Action<float> OnMissCounterChanged;
    public event Action<List<Arrow>> OnSequenceChanged;
    public event Action<QuipData> OnQuipActivated;
    public event Action<QuipData> OnQuipLaunched;
    public event Action OnQuipActivationCancelled;

    List<Arrow> _sequence = new List<Arrow>();
    List<QuipData> _playerQuips = new List<QuipData>();
    Dictionary<int, float> QuipCooldowns = new Dictionary<int, float>();

    float _missChrono = 0f;
    int _activatingQuip = -1;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    void Start()
    {
        InitPlayerQuips();
    }

    void Update()
    {
        UpdateQuipCooldowns();
        if(_missChrono > 0f)
        {
            _missChrono -= Time.deltaTime;
            OnMissCounterChanged?.Invoke(_missChrono);

            if(_missChrono <= 0f)
            {
                _sequence.Clear();
                OnSequenceChanged?.Invoke(_sequence);
            }
        }
        
        if(_missChrono <= 0f && _activatingQuip == -1)
        {
            CheckSequenceValid();
            CheckQuipActivations();
        }
    }

    void InitPlayerQuips()
    {
        foreach(int quipId in HCPlayer.LocalInstance.Quips)
        {
            _playerQuips.Add(GameData.Instance.GetQuipData(quipId));
        }
        _playerQuips.Add(GameData.Instance.GetQuipData(0));
    }

    void UpdateQuipCooldowns()
    {
        var keys = QuipCooldowns.Keys.ToList();

        foreach(int quipId in keys)
        {
            float cooldown = QuipCooldowns[quipId];
            cooldown -= Time.deltaTime;
            if(cooldown <= 0)
            {
                QuipCooldowns.Remove(quipId);
            }
            else
            {
                QuipCooldowns[quipId] = cooldown;
            }
            GamePlayerUI.UpdateQuipCooldown(quipId, cooldown);
        }
    }

    void CheckSequenceValid()
    {
        bool valid = false;
        foreach(QuipData quip in _playerQuips)
        {
            valid = valid || quip.SequenceWithinCommand(_sequence);

            if(valid)
            {
                break;
            }
        }
        
        if(!valid)
        {
            _missChrono = MISS_PENALTY;
            OnMissCounterChanged?.Invoke(_missChrono);
        }
    }

    void CheckQuipActivations()
    {
        foreach(QuipData quip in _playerQuips)
        {
            if(QuipCooldowns.ContainsKey(quip.Id))
            {
                continue;
            }

            if(quip.SequenceActivatedCommand(_sequence))
            {
                _activatingQuip = quip.Id;
                OnQuipActivated?.Invoke(quip);

                if(quip.Id == 0) // Leave Planet Quip
                {
                    InputLaunchQuip();
                }
            }
        }
    }

    void UserArrowActionRegistered(InputAction.CallbackContext context, Arrow arrow)
    {
        if(context.performed
            && HCGameManager.Instance.State.Value == HCGameManager.GameState.GAME_ACTIVE
            && !HCNetworkManager.Instance.GetPlayerData().Ready
            && _missChrono <= 0f
            && _activatingQuip == -1)
        {
            _sequence.Add(arrow);
            OnSequenceChanged?.Invoke(_sequence);
        }
    }

    public void OnResetSequence(InputAction.CallbackContext context)
    {
        if(context.performed
            && HCGameManager.Instance.State.Value == HCGameManager.GameState.GAME_ACTIVE
            && !HCNetworkManager.Instance.GetPlayerData().Ready
            && _missChrono <= 0f
            && _activatingQuip == -1)
        {
            _sequence.Clear();
            OnSequenceChanged?.Invoke(_sequence);
        }
    }

    public void OnArrowUp(InputAction.CallbackContext context)
    {
        UserArrowActionRegistered(context, Arrow.UP);
    }

    public void OnArrowDown(InputAction.CallbackContext context)
    {
        UserArrowActionRegistered(context, Arrow.DOWN);
    }

    public void OnArrowLeft(InputAction.CallbackContext context)
    {
        UserArrowActionRegistered(context, Arrow.LEFT);
    }

    public void OnArrowRight(InputAction.CallbackContext context)
    {
        UserArrowActionRegistered(context, Arrow.RIGHT);
    }

    //From Player Input Mouse Left Click
    public void InputLaunchQuip()
    {
        if(_activatingQuip >= 0)
        {
            QuipCooldowns[_activatingQuip] = GameData.Instance.GetQuipData(_activatingQuip).Cooldown;
            HCPlayer.LocalInstance.LaunchQuipServerRpc(_activatingQuip, GetQuipLaunchPosition());
            OnQuipLaunched?.Invoke(GameData.Instance.GetQuipData(_activatingQuip));
            _sequence.Clear();
            OnSequenceChanged?.Invoke(_sequence);
            _activatingQuip = -1;
        }
    }

    //From Player Input Mouse Right Click
    public void InputCancelQuipActivation()
    {
        if(_activatingQuip >= 0)
        {
            OnQuipActivationCancelled?.Invoke();
            _activatingQuip = -1;
            _sequence.Clear();
            OnSequenceChanged?.Invoke(_sequence);
        }
    }

    Vector3 GetQuipLaunchPosition()
    {
        return QuipVisualPosition.transform.position;
    }
}
