using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class HCBaseCannon : NetworkBehaviour
{
    const float RADIUS = 0.9f;
    const float ATTACK_DISTANCE = 3f;
    const int DPS = 5;
    const float SPARK_SPAWN_RATE = 0.1f;

    [SerializeField] SpriteRenderer CannonSprite;
    [SerializeField] LineRenderer OuterLaser;
    [SerializeField] LineRenderer InnerLaser;
    [SerializeField] SpriteRenderer SparkPrefab;

    HCUnit _target;
    float _attackChrono = 0f;

    bool _attacking;
    NetworkObject _targetNO = null;
    float _sparksChrono = 0f; // From Vector2 Up

    NetworkVariable<float> _angle = new NetworkVariable<float>(0f);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        OuterLaser.gameObject.SetActive(false);
        InnerLaser.gameObject.SetActive(false);
        SparkPrefab.gameObject.SetActive(false);

        ulong ownerId = GetComponent<NetworkObject>().OwnerClientId;
        CannonSprite.color = HCColor.GetPlayerColorDark(HCNetworkManager.Instance.GetPlayerIndexFromClientId(ownerId));

        HCGameManager.Instance.State.OnValueChanged += OnGameStateChanged;
    }

    public override void OnDestroy()
    {
        HCGameManager.Instance.State.OnValueChanged -= OnGameStateChanged;

        base.OnDestroy();
    }

    public void Init(int playerIndex, int maxPlayers)
    {
        float anglePerPlayer = 360f / maxPlayers;
        Vector3 direction = HCUtils.RotateVector2(Vector2.up, anglePerPlayer * playerIndex);
        _angle.Value = Vector2.SignedAngle(Vector2.up, direction);
        transform.position = direction * RADIUS;
    }

    void Update()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            if(HCGameManager.Instance.State.Value == HCGameManager.GameState.GAME_ACTIVE)
            {
                if(_target == null)
                {
                    FindTarget();
                }
                if(_target != null && !_attacking)
                {
                    TargetEnemy(_target);
                }
                if(_attacking)
                {
                    if(_target.IsDead() || !EnemyInRange(_target) || !EnemyInAngle(_target))
                    {
                        StopAttacking();
                    }
                    else
                    {
                        DealDamage();
                    }
                }

                if(_target != null)
                {
                    UpdateAngle();
                }
            }
            else
            {
                _target = null;
                _attacking = false;
            }
        }
        
        if(_targetNO != null)
        {
            SetLasers();
            SpawnSparks();
        }
        UpdatePosition();
    }

    void FindTarget()
    {
        HCUnit closestEnemy = null;
        float minDistance = ATTACK_DISTANCE;
        foreach(HCUnit enemy in HCMap.Instance.EnemyUnits)
        {
            if(!enemy.IsDead() && EnemyInAngle(enemy))
            {
                float distance = enemy.transform.position.magnitude - RADIUS;
                if(distance < minDistance)
                {
                    closestEnemy = enemy;
                    minDistance = distance;
                }
            }
        }

        if(closestEnemy != null)
        {
            _target = closestEnemy;
        }
    }

    bool EnemyInAngle(HCUnit enemy)
    {
        ulong ownerId = GetComponent<NetworkObject>().OwnerClientId;
        int index = HCNetworkManager.Instance.GetPlayerIndexFromClientId(ownerId);
        int maxPlayers = HCNetworkManager.Instance.PlayersConnected();
        float anglePerPlayer = 360f / maxPlayers;
        float min_angle = anglePerPlayer * index;
        float max_angle = anglePerPlayer * (index + 1);

        Vector2 enemyDir = enemy.transform.position.ToV2();
        float angle = Vector2.SignedAngle(Vector2.left, enemyDir) + 180f;
        return angle > min_angle && angle <= max_angle;
    }

    bool EnemyInRange(HCUnit enemy)
    {
        float distance = enemy.transform.position.magnitude - RADIUS;
        return distance <= ATTACK_DISTANCE;
    }

    void TargetEnemy(HCUnit target)
    {
        _attacking = true;
        TargetEnemyClientRpc(target.GetComponent<NetworkObject>());
        _attackChrono = 0f;
    }

    void StopAttacking()
    {
        _attacking = false;
        _target = null;
        StopAttackingClientRpc();
    }

    void DealDamage()
    {
        _attackChrono += Time.deltaTime;
        if(_attackChrono > 1f)
        {
            _attackChrono -= 1f;
            _target.IsHit(DPS);
        }
    }

    void UpdateAngle()
    {
        _angle.Value = Vector2.SignedAngle(Vector2.up, _target.transform.position.ToV2());
    }

    void SetLasers()
    {
        Vector3 dir = _targetNO.transform.position.normalized;
        Vector3 posA = dir * (RADIUS + 0.2f);
        Vector3 posB = _targetNO.transform.position;
        InnerLaser.SetPositions(new Vector3[] {posA, posB});
        OuterLaser.SetPositions(new Vector3[] {posA, posB});
    }

    void SpawnSparks()
    {
        _sparksChrono += Time.deltaTime;
        if(_sparksChrono >= SPARK_SPAWN_RATE)
        {
            _sparksChrono -= SPARK_SPAWN_RATE;
            SpawnSpark();
        }
    }

    void SpawnSpark()
    {
        SpriteRenderer spark = Instantiate(SparkPrefab);
        spark.gameObject.SetActive(true);
        spark.transform.position = _targetNO.transform.position;

        Vector3 direction = HCUtils.RotateVector2(Vector2.up, UnityEngine.Random.Range(0f, 360f));
        spark.transform.DOMove(spark.transform.position + direction, 1f).SetEase(Ease.OutQuad);
        spark.transform.DOScale(0f, 1f).SetEase(Ease.InCubic).OnComplete(()=>Destroy(spark.gameObject));
    }

    void UpdatePosition()
    {
        Vector3 dir = HCUtils.RotateVector2(Vector2.up, -_angle.Value);
        transform.position = dir * RADIUS;
        transform.rotation = Quaternion.Euler(0f, 0f, _angle.Value);
    }

    private void OnGameStateChanged(HCGameManager.GameState previousValue, HCGameManager.GameState newValue)
    {
        if(previousValue == HCGameManager.GameState.GAME_ACTIVE && (newValue == HCGameManager.GameState.GAME_DEPARTING_PLANET || newValue == HCGameManager.GameState.GAME_FAILED))
        {
            gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    void TargetEnemyClientRpc(NetworkObjectReference nor)
    {
        if(nor.TryGet(out NetworkObject no))
        {
            InnerLaser.gameObject.SetActive(true);
            OuterLaser.gameObject.SetActive(true);
            _targetNO = no;
            _sparksChrono = 0f;
        }
        else
        {
            Debug.Log("BaseCannon: Could not find Target Network Object");
        }
    }

    [ClientRpc]
    void StopAttackingClientRpc()
    {
        InnerLaser.gameObject.SetActive(false);
        OuterLaser.gameObject.SetActive(false);
        _targetNO = null;
    }
}
