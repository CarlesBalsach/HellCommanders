using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class HCGameCamera : MonoBehaviour
{
    public static HCGameCamera Instance = null;

    public const float MOUSE_TO_WORLD = 0.1f;
    const float SCROLL_TO_ORTHOGRAPHIC = 0.1f;
    const float MIN_ORTHOGRAPHIC = 3f;
    const float DEFAULT_ORTHOGRAPHIC = 6f;
    const float MAX_ORTHOGRAPHIC = 15f;
    Camera _camera;
    bool _firstMove = true;
    Transform _followTransform = null;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    void Start()
    {
        _camera = GetComponent<Camera>();

        SetCameraDefaultPositionAndSize();
    }

    void Update()
    {
        if(HCGameManager.Instance.State.Value == HCGameManager.GameState.GAME_ACTIVE)
        {
            UpdateCameraPositionAndSize();
        }
        else
        {
            SetCameraDefaultPositionAndSize();
        }
    }

    void UpdateCameraPositionAndSize()
    {
        CheckScrollInput();

        if(_followTransform == null)
        {
            CheckMouseDelta();
        }
        else
        {
            FollowTransform();
        }
    }

    void CheckScrollInput()
    {
        float scroll = Mouse.current.scroll.value.y;
        float deltaOrtho = -scroll * SCROLL_TO_ORTHOGRAPHIC;
        float ortho = Mathf.Clamp(_camera.orthographicSize + deltaOrtho, MIN_ORTHOGRAPHIC, MAX_ORTHOGRAPHIC);
        _camera.orthographicSize = ortho;
    }

    void CheckMouseDelta()
    {
        Vector2 delta = Mouse.current.delta.value;
        if(delta != Vector2.zero)
        {
            if(_firstMove)
            {
                _firstMove = false;
                return;
            }

            delta *= MOUSE_TO_WORLD;
            Vector3 pos = transform.position;
            pos.x += delta.x;
            pos.y += delta.y;
            SetCameraPosition(pos);
        }
    }

    void FollowTransform()
    {
        Vector3 pos = _followTransform.position;
        pos.z = transform.position.z;
        pos.y -= Camera.main.orthographicSize * 2f * 0.1f;
        SetCameraPosition(pos);
    }

    void SetCameraPosition(Vector3 pos)
    {
        float height = _camera.orthographicSize;
        float width = height * _camera.aspect;

        float MIN_X = -HCMap.SIZE / 2f + width - 1f - width * 0.4f;
        float MAX_X = HCMap.SIZE / 2f - width + 1f + width * 0.4f;
        pos.x = Mathf.Clamp(pos.x, MIN_X, MAX_X);

        float MIN_Y = -HCMap.SIZE / 2f + height - 1f - height * 0.4f;
        float MAX_Y = HCMap.SIZE / 2f - height + 1f;
        pos.y = Mathf.Clamp(pos.y, MIN_Y, MAX_Y);

        transform.position = pos;
    }

    void SetCameraDefaultPositionAndSize()
    {
        _camera.orthographicSize = DEFAULT_ORTHOGRAPHIC;
        Vector3 pos = transform.position;
        pos.x = 0f;
        pos.y = 0f;

        if(HCGameManager.Instance.State.Value == HCGameManager.GameState.GAME_LANDING_PLANET)
        {
            pos.y = -_camera.orthographicSize * 2f * 0.1f;
        }

        transform.position = pos;
    }

    public void SetTransformToFollow(Transform t)
    {
        _followTransform = t;
    }
}
