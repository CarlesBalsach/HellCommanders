using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HCBullet : NetworkBehaviour
{
    const float ALIVE_TIME = 10f;
    const float SPEED = 5f;

    [SerializeField] SpriteRenderer BulletSprite;
    Transform _owner;
    Vector2 _speed;
    bool _initialized = false;
    Rigidbody2D _rb;
    int _damage = 0;
    float _spawnTime = 0f;
    bool _hit = false;
    bool _destroying = false;

    public void Init(Vector2 direction, int damage, Transform owner) // Server
    {
        _owner = owner;
        _speed = direction * SPEED;
        _damage = damage;
        _initialized = true;
        _rb = GetComponent<Rigidbody2D>();

        _spawnTime = Time.time;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        InitializeClientRpc(transform.position, _speed, angle);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(!NetworkManager.Singleton.IsServer)
        {
            BulletSprite.gameObject.SetActive(false);
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private void Update()
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if(!_destroying && (_hit || Time.time - _spawnTime > ALIVE_TIME))
        {
            _destroying =  true;
            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    private void FixedUpdate()
    {
        if(_initialized && !_hit)
        {
            _rb.MovePosition(transform.position.ToV2() + _speed * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!_initialized || _hit)
        {
            return;
        }

        if(other.transform != _owner)
        {
            HCUnit unit = other.GetComponent<HCUnit>();
            if(unit != null)
            {
                unit.IsHit(_damage);
                _hit = true;
            }
        }
    }

    [ClientRpc]
    void InitializeClientRpc(Vector2 position, Vector2 speed, float angle)
    {
        if(!NetworkManager.Singleton.IsHost)
        {
            BulletSprite.gameObject.SetActive(true);
            transform.position = position;
            _speed = speed;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            _rb = GetComponent<Rigidbody2D>();
            _initialized = true;
        }
    }
}
