//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/Scripts/Inputs/InputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @InputActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @InputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputActions"",
    ""maps"": [
        {
            ""name"": ""Game"",
            ""id"": ""9c56b2fd-dc58-44cb-8673-cca4b1250e57"",
            ""actions"": [
                {
                    ""name"": ""ArrowUp"",
                    ""type"": ""Button"",
                    ""id"": ""06bd9041-b657-4f79-9f93-e5a208b895d5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ArrowDown"",
                    ""type"": ""Button"",
                    ""id"": ""ed5b343b-0011-4dbf-845a-c8ff6caf8166"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ArrowLeft"",
                    ""type"": ""Button"",
                    ""id"": ""ffc485bf-a4a7-412c-a545-3f710654427f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ArrowRight"",
                    ""type"": ""Button"",
                    ""id"": ""8a8b2428-dfa4-43e2-aa70-88a52b23be26"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ResetSequence"",
                    ""type"": ""Button"",
                    ""id"": ""407e6efc-6bd3-4120-9e3a-477ec88e2ca6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""LaunchQuip"",
                    ""type"": ""Button"",
                    ""id"": ""8baa0926-106d-48ba-8b5c-05522df5feb3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""CancelQuip"",
                    ""type"": ""Button"",
                    ""id"": ""db4e9e0e-8b0d-48b7-bdb1-9f1f078133a9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""a3cb0db7-7081-487a-878e-2040af11950f"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ArrowUp"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""75c3d5db-8bef-427f-a5ca-bc345a95f48b"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ArrowDown"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""78f4409f-b8aa-4b35-8a2a-b2b4cdb6a7cd"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ArrowLeft"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""54eb37aa-2658-479b-8876-331030a1e9ce"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ArrowRight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7b9da699-f4ee-4bb0-ba05-fb7d3c7e7e7c"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ResetSequence"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""aa129a5a-48cd-4a9a-ba6f-ad52f5c8fada"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LaunchQuip"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""763b4bf0-0c85-44d1-9fc6-d6fe630619c7"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CancelQuip"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Game
        m_Game = asset.FindActionMap("Game", throwIfNotFound: true);
        m_Game_ArrowUp = m_Game.FindAction("ArrowUp", throwIfNotFound: true);
        m_Game_ArrowDown = m_Game.FindAction("ArrowDown", throwIfNotFound: true);
        m_Game_ArrowLeft = m_Game.FindAction("ArrowLeft", throwIfNotFound: true);
        m_Game_ArrowRight = m_Game.FindAction("ArrowRight", throwIfNotFound: true);
        m_Game_ResetSequence = m_Game.FindAction("ResetSequence", throwIfNotFound: true);
        m_Game_LaunchQuip = m_Game.FindAction("LaunchQuip", throwIfNotFound: true);
        m_Game_CancelQuip = m_Game.FindAction("CancelQuip", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Game
    private readonly InputActionMap m_Game;
    private List<IGameActions> m_GameActionsCallbackInterfaces = new List<IGameActions>();
    private readonly InputAction m_Game_ArrowUp;
    private readonly InputAction m_Game_ArrowDown;
    private readonly InputAction m_Game_ArrowLeft;
    private readonly InputAction m_Game_ArrowRight;
    private readonly InputAction m_Game_ResetSequence;
    private readonly InputAction m_Game_LaunchQuip;
    private readonly InputAction m_Game_CancelQuip;
    public struct GameActions
    {
        private @InputActions m_Wrapper;
        public GameActions(@InputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @ArrowUp => m_Wrapper.m_Game_ArrowUp;
        public InputAction @ArrowDown => m_Wrapper.m_Game_ArrowDown;
        public InputAction @ArrowLeft => m_Wrapper.m_Game_ArrowLeft;
        public InputAction @ArrowRight => m_Wrapper.m_Game_ArrowRight;
        public InputAction @ResetSequence => m_Wrapper.m_Game_ResetSequence;
        public InputAction @LaunchQuip => m_Wrapper.m_Game_LaunchQuip;
        public InputAction @CancelQuip => m_Wrapper.m_Game_CancelQuip;
        public InputActionMap Get() { return m_Wrapper.m_Game; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GameActions set) { return set.Get(); }
        public void AddCallbacks(IGameActions instance)
        {
            if (instance == null || m_Wrapper.m_GameActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_GameActionsCallbackInterfaces.Add(instance);
            @ArrowUp.started += instance.OnArrowUp;
            @ArrowUp.performed += instance.OnArrowUp;
            @ArrowUp.canceled += instance.OnArrowUp;
            @ArrowDown.started += instance.OnArrowDown;
            @ArrowDown.performed += instance.OnArrowDown;
            @ArrowDown.canceled += instance.OnArrowDown;
            @ArrowLeft.started += instance.OnArrowLeft;
            @ArrowLeft.performed += instance.OnArrowLeft;
            @ArrowLeft.canceled += instance.OnArrowLeft;
            @ArrowRight.started += instance.OnArrowRight;
            @ArrowRight.performed += instance.OnArrowRight;
            @ArrowRight.canceled += instance.OnArrowRight;
            @ResetSequence.started += instance.OnResetSequence;
            @ResetSequence.performed += instance.OnResetSequence;
            @ResetSequence.canceled += instance.OnResetSequence;
            @LaunchQuip.started += instance.OnLaunchQuip;
            @LaunchQuip.performed += instance.OnLaunchQuip;
            @LaunchQuip.canceled += instance.OnLaunchQuip;
            @CancelQuip.started += instance.OnCancelQuip;
            @CancelQuip.performed += instance.OnCancelQuip;
            @CancelQuip.canceled += instance.OnCancelQuip;
        }

        private void UnregisterCallbacks(IGameActions instance)
        {
            @ArrowUp.started -= instance.OnArrowUp;
            @ArrowUp.performed -= instance.OnArrowUp;
            @ArrowUp.canceled -= instance.OnArrowUp;
            @ArrowDown.started -= instance.OnArrowDown;
            @ArrowDown.performed -= instance.OnArrowDown;
            @ArrowDown.canceled -= instance.OnArrowDown;
            @ArrowLeft.started -= instance.OnArrowLeft;
            @ArrowLeft.performed -= instance.OnArrowLeft;
            @ArrowLeft.canceled -= instance.OnArrowLeft;
            @ArrowRight.started -= instance.OnArrowRight;
            @ArrowRight.performed -= instance.OnArrowRight;
            @ArrowRight.canceled -= instance.OnArrowRight;
            @ResetSequence.started -= instance.OnResetSequence;
            @ResetSequence.performed -= instance.OnResetSequence;
            @ResetSequence.canceled -= instance.OnResetSequence;
            @LaunchQuip.started -= instance.OnLaunchQuip;
            @LaunchQuip.performed -= instance.OnLaunchQuip;
            @LaunchQuip.canceled -= instance.OnLaunchQuip;
            @CancelQuip.started -= instance.OnCancelQuip;
            @CancelQuip.performed -= instance.OnCancelQuip;
            @CancelQuip.canceled -= instance.OnCancelQuip;
        }

        public void RemoveCallbacks(IGameActions instance)
        {
            if (m_Wrapper.m_GameActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IGameActions instance)
        {
            foreach (var item in m_Wrapper.m_GameActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_GameActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public GameActions @Game => new GameActions(this);
    public interface IGameActions
    {
        void OnArrowUp(InputAction.CallbackContext context);
        void OnArrowDown(InputAction.CallbackContext context);
        void OnArrowLeft(InputAction.CallbackContext context);
        void OnArrowRight(InputAction.CallbackContext context);
        void OnResetSequence(InputAction.CallbackContext context);
        void OnLaunchQuip(InputAction.CallbackContext context);
        void OnCancelQuip(InputAction.CallbackContext context);
    }
}
