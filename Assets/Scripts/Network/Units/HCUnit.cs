using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using DG.Tweening;


[Serializable]
public struct UnitStats
{
    public string Name;
    public string Description;
    public int HP;
    public int Damage;
    public int Armor;
    public float Speed;
    public float AttackSpeed;
    public float AttackDistance;
    public float MinDistance;
    public int Rounds;
    public float Radius;
    public bool IsConstruct;
}

public class HCUnit : NetworkBehaviour
{
    const float DEAD_FADE_TIME = 1f;
    const float BURN_TICK = 1f;
    const float BURN_DURATION = 3f;
    const int FIRE_DAMAGE = 5;
    const float READY_TIME = 1f;

    public static int UIdCounter = 0;

    public UnitStats Stats;
    public bool IsEnemy = false;

    [SerializeField] protected SpriteRenderer UnitSprite;
    [SerializeField] protected CircleCollider2D Collider;
    [SerializeField] protected Rigidbody2D Rigidbody;
    
    [HideInInspector] public int UId = -1;
    [HideInInspector] public NetworkVariable<int> HP = new NetworkVariable<int>(1);
    [HideInInspector] private NetworkVariable<bool> Burning = new NetworkVariable<bool>(false);
    [HideInInspector] protected NetworkVariable<int> _roundsLeft = new NetworkVariable<int>(0);
    
    protected HCUnit _target;
    protected bool _dead = false;

    float _timeBurning = 0f;
    float _burnChrono = 0f;

    protected float _attackCooldown = 0f;
    protected bool _ready = false;

    protected virtual void Awake()
    {
        UId = UIdCounter++;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        HP.OnValueChanged += OnHPValueChanged;

        if(!NetworkManager.Singleton.IsServer)
        {
            Collider.enabled = false;
        }
    }

    protected virtual void Start()
    {
        HCMap.Instance.UnitSpawned(this);
        if(NetworkManager.Singleton.IsServer)
        {
            HP.Value = Stats.HP;
            _roundsLeft.Value = Stats.Rounds;
            if(IsEnemy)
            {
                _ready = true;
            }
            else
            {
                StartCoroutine(GetReadyIn(READY_TIME));
            }
        }
    }

    protected virtual void Update()
    {
        if(Burning.Value)
        {
            SpawnBurningParticles();
        }

        Stats.Rounds = _roundsLeft.Value;

        if(!NetworkManager.Singleton.IsServer || HCGameManager.Instance.State.Value != HCGameManager.GameState.GAME_ACTIVE || _dead)
        {
            return;
        }

        if(Burning.Value)
        {
            _timeBurning += Time.deltaTime;
            if(_timeBurning > BURN_TICK)
            {
                _timeBurning -= BURN_TICK;
                IsHit(FIRE_DAMAGE, true);
            }
            _burnChrono -= Time.deltaTime;
            if(_burnChrono <= 0f || IsDead())
            {
                _timeBurning = 0f;
                Burning.Value = false;
            }
        }

        if(Stats.IsConstruct && _roundsLeft.Value <= 0)
        {
            _dead = true;
            HCMap.Instance.RemoveUnit(this);
            Collider.enabled = false;
            DisappearClientRpc();
        }

        if(!_dead && IsDead())
        {
            _dead = true;

            HCMap.Instance.RemoveUnit(this);

            Collider.enabled = false;
        }

        if(_ready)
        {
            if(!_dead)
            {
                FindTarget();
            }
            if(_attackCooldown > 0f)
            {
                _attackCooldown -= Time.deltaTime;
            }
            if(TargetInSight() && _attackCooldown <= 0f)
            {
                AttackTarget();
                _attackCooldown = Stats.AttackSpeed;
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        if(!NetworkManager.Singleton.IsServer || HCGameManager.Instance.State.Value != HCGameManager.GameState.GAME_ACTIVE || IsDead())
        {
            return;
        }

        if(_target != null && !TargetInSight())
        {
            OrientateTowardsTarget();
            MoveTowardsTarget();
        }
        else
        {
            OrientateTowardsTarget();
            if(Rigidbody.bodyType != RigidbodyType2D.Static)
            {
                Rigidbody.velocity = Vector2.zero;
            }
        }
    }

    protected virtual void FindTarget()
    {
        _target = null;

        List<HCUnit> targets = HCMap.Instance.EnemyUnits;
        if(IsEnemy)
        {
            targets = HCMap.Instance.AllyUnits;
        }

        foreach(HCUnit unit in targets)
        {
            if(!unit.IsDead())
            {
                if(_target == null)
                {
                    _target = unit;
                }
                else
                {
                    float targetDistanceSqr = (_target.transform.position - transform.position).sqrMagnitude;
                    float unitDistanceSqr = (unit.transform.position - transform.position).sqrMagnitude;
                    if(unitDistanceSqr < targetDistanceSqr)
                    {
                        _target = unit;
                    }
                }
            }
        }
    }
    
    protected virtual bool TargetInSight()
    {
        if(_target == null)
        {
            return false;
        }

        return TargetDistanceGap() <= Stats.AttackDistance;
    }

    protected virtual float TargetDistanceGap()
    {
        if(_target == null)
        {
            return Mathf.Infinity;
        }

        float distance = (transform.position - _target.transform.position).magnitude;
        return distance - Stats.Radius - _target.Stats.Radius;
    }

    protected virtual void OrientateTowardsTarget()
    {
        if(_target != null)
        {
            Vector2 direction = _target.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Rigidbody.MoveRotation(angle);
        }
    }

    protected virtual void MoveTowardsTarget()
    {
        if(_target != null)
        {
            Vector2 direction = _target.transform.position - transform.position;
            direction.Normalize();
            Rigidbody.velocity = direction * Stats.Speed;
        }
    }

    protected virtual void AttackTarget()
    {
        
    }

    public bool IsDead()
    {
        return HP.Value <= 0;
    }

    public virtual void Die()
    {
        HP.Value = 0;
    }

    public virtual void IsHit(int damage, bool ignoreArmor = false) // Server
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Unit being hit in a non-server environment");
            return;
        }

        if(!ignoreArmor)
        {
            damage = Mathf.Max(1, damage - Stats.Armor);
        }

        HP.Value = Mathf.Max(0, HP.Value - damage);

        if(IsEnemy)
        {
            SpawnBloodStainsClientRpc((float)damage / Stats.HP, HP.Value <= 0);
        }
    }

    public virtual void Burn() // Server
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Unit being hit in a non-server environment");
            return;
        }

        if(!Stats.IsConstruct)
        {
            Burning.Value = true;
            _burnChrono = BURN_DURATION;
        }
    }

    protected virtual void SpawnBurningParticles()
    {
        float rand = UnityEngine.Random.Range(0f, 1f);
        if(rand < 0.1f)
        {
            PrefabController.Instance.SpawnBurningParticle(transform.position, Stats.Radius, transform);
        }
    }

    private void OnHPValueChanged(int previousValue, int newValue)
    {
        if(newValue == 0 && previousValue > 0)
        {
            HCMap.Instance.RemoveUnit(this);
            DeadAnimation();
        }
    }

    protected virtual void DeadAnimation()
    {
        List<SpriteRenderer> sprites = HCUtils.FindComponentsRecursively<SpriteRenderer>(UnitSprite.transform);
        foreach(SpriteRenderer sprite in sprites)
        {
            sprite.DOFade(0f, DEAD_FADE_TIME).SetEase(Ease.InQuad);
        }
        StartCoroutine(DeactivateAfter(DEAD_FADE_TIME));
    }

    protected virtual void DisappearAnimation()
    {
        List<SpriteRenderer> sprites = HCUtils.FindComponentsRecursively<SpriteRenderer>(UnitSprite.transform);
        foreach(SpriteRenderer sprite in sprites)
        {
            sprite.DOFade(0f, DEAD_FADE_TIME).SetEase(Ease.InQuad);
        }
        StartCoroutine(DeactivateAfter(DEAD_FADE_TIME));
    }

    protected IEnumerator DeactivateAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
    }

    public SpriteRenderer GetUnitSprite()
    {
        return UnitSprite;
    }

    IEnumerator GetReadyIn(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _ready = true;
    }

    [ClientRpc]
    void DisappearClientRpc()
    {
        DisappearAnimation();
        HCMap.Instance.RemoveUnit(this);
    }

    [ClientRpc]
    protected virtual void SpawnBloodStainsClientRpc(float hpLoss, bool dead) //hpLoss from 0 to 1
    {
        if(dead)
        {
            PrefabController.Instance.SpawnBloodStainPrefab(transform.position, Stats.Radius * 2f * 1.33f, HCColor.EnemyBlood);
        }
        else
        {
            Vector3 pos = transform.position + HCUtils.RotateVector2(Vector2.up * Stats.Radius, UnityEngine.Random.Range(0f, 360f)).ToV3();
            float scale = Mathf.Max(0.25f, Stats.Radius * 2f * hpLoss * 1.33f);
            PrefabController.Instance.SpawnBloodStainPrefab(pos, scale, HCColor.EnemyBlood);
        }
    }
}
