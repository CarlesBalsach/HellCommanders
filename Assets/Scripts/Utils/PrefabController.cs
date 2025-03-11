using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class PrefabController : MonoBehaviour
{
    public static PrefabController Instance = null;

    public Sprite BaseSprite;
    public Sprite GoodConnectionSprite;
    public Sprite OkConnectionSprite;
    public Sprite BadConnectionSprite;
    public SpriteRenderer SmokeParticlePrefab;
    public SpriteRenderer BloodStainPrefab;
    public BombDrop BombDropPrefab;
    public UnitDrop UnitDropPrefab;
    public ExplosionAoE ExplisionPrefab;
    public HCBaseCannon BaseCannonPrefab;
    public SpriteRenderer SquareEffectSprite;

    Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    Sprite GetCachedSprite(string path)
    {
        if(SpriteCache.ContainsKey(path))
        {
            return SpriteCache[path];
        }

        Sprite sprite = Resources.Load<Sprite>(path);
        SpriteCache[path] = sprite;
        return sprite;
    }

    public Sprite GetCharacterSprite(CharacterData character)
    {
        return GetCachedSprite($"Characters/character_{character.Id}");
    }

    public Sprite GetQuipSprite(QuipData quip)
    {
        return GetCachedSprite($"Quips/Images/{quip.Image}");
    }

    public SpriteRenderer SpawnSmokeParticle(Transform parent)
    {
        SpriteRenderer smoke = Instantiate(SmokeParticlePrefab, parent);
        return smoke;
    }

    public SpriteRenderer SpawnBloodStainPrefab(Vector3 position, float scale, Color color)
    {
        SpriteRenderer blood = Instantiate(BloodStainPrefab);
        blood.transform.position = position;
        blood.transform.localScale = Vector3.one * scale;
        blood.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        color.a = 0f;
        blood.color = color;

        Sequence seq = DOTween.Sequence();
        seq.Append(blood.DOFade(1f, 1f).SetEase(Ease.Linear))
            .AppendInterval(300f)
            .Append(blood.DOFade(0f, 3f).SetEase(Ease.Linear))
            .AppendCallback(()=>Destroy(blood.gameObject));

        return blood;
    }

    public BombDrop SpawnBombDrop(Vector3 position)
    {
        BombDrop bomb = Instantiate(BombDropPrefab);
        bomb.transform.position = position;
        return bomb;
    }

    public UnitDrop SpawnUnitDrop(Vector3 position)
    {
        UnitDrop unit = Instantiate(UnitDropPrefab);
        unit.transform.position = position;
        return unit;
    }

    public ExplosionAoE SpawnExplosionAoE(Vector3 position)
    {
        ExplosionAoE explosion = Instantiate(ExplisionPrefab);
        explosion.transform.position = position;
        return explosion;
    }

    public void SpawnBurningParticle(Vector3 position, float radius, Transform parent)
    {
        SpriteRenderer burn = Instantiate(SquareEffectSprite, parent);
        burn.transform.position = HCUtils.GetPositionInArea(position, radius);
        burn.transform.localScale = Vector3.one * radius * 0.5f;
        burn.color = HCColor.Fire;
        burn.transform.DOMove(position + Vector3.up, 1f).SetEase(Ease.Linear);
        burn.transform.DOLocalRotate(Vector3.forward * 360f * HCUtils.Random1m1(), 1f).SetEase(Ease.OutQuad);
        burn.transform.DOScale(0f, 1f).SetEase(Ease.InQuad).OnComplete(()=>Destroy(burn));
    }

    public SpriteRenderer SpawnSpark(Vector3 position, float size)
    {
        SpriteRenderer spark = Instantiate(SquareEffectSprite);
        spark.transform.position = position;
        spark.transform.localScale = Vector3.one * size;
        return spark;
    }

    public void NetworkSpawnBaseCannon(int playerIndex, int maxPlayers, ulong ownerId)
    {
        HCBaseCannon bc = Instantiate(BaseCannonPrefab);
        bc.Init(playerIndex, maxPlayers);
        bc.GetComponent<NetworkObject>().SpawnWithOwnership(ownerId, true);
    }

    public void NetworkSpawnQuip(QuipData quip, Transform parent, Vector3 position, ulong ownerId)
    {
        string path = $"Quips/Prefabs/{quip.QuipPrefab}";
        GameObject quipGO = Instantiate(Resources.Load(path), parent) as GameObject;
        quipGO.transform.position = position;
        HCQuip hcQuip = quipGO.GetComponent<HCQuip>();
        hcQuip.Init(quip);
        quipGO.GetComponent<NetworkObject>().SpawnWithOwnership(ownerId, true);
    }

    public NetworkBehaviour NetworkSpawnUtility(string prefabName, Vector3 position)
    {
        string path = $"Network/Utilities/{prefabName}";
        GameObject unit = Instantiate(Resources.Load(path)) as GameObject;
        unit.transform.position = position;
        unit.GetComponent<NetworkObject>().Spawn(true);

        Debug.Log($"Spawning Utility {prefabName} at {position}");

        NetworkBehaviour utility = unit.GetComponent<NetworkBehaviour>();
        return utility;
    }

    public NetworkBehaviour NetworkSpawnEffect(string prefabName, Vector3 position)
    {
        string path = $"Network/Effects/{prefabName}";
        GameObject unit = Instantiate(Resources.Load(path)) as GameObject;
        unit.transform.position = position;
        unit.GetComponent<NetworkObject>().Spawn(true);

        Debug.Log($"Spawning Effect {prefabName} at {position}");

        NetworkBehaviour effect = unit.GetComponent<NetworkBehaviour>();
        return effect;
    }

    public HCUnit NetworkSpawnAlly(string prefabName, Vector3 position)
    {
        string path = $"Network/Allies/{prefabName}";
        GameObject unit = Instantiate(Resources.Load(path)) as GameObject;
        unit.transform.position = position;
        unit.GetComponent<NetworkObject>().Spawn(true);

        Debug.Log($"Spawning Ally {prefabName} at {position}");

        HCUnit ally = unit.GetComponent<HCUnit>();
        return ally;
    }

    public HCUnit NetworkSpawnEnemy(string prefabName, Vector3 position)
    {
        string path = $"Network/Enemies/{prefabName}";
        GameObject unit = Instantiate(Resources.Load(path)) as GameObject;
        unit.transform.position = position;
        unit.GetComponent<NetworkObject>().Spawn(true);

        // Debug.Log($"Spawning Enemy {prefabName} at {position}");

        HCUnit enemy = unit.GetComponent<HCUnit>();
        return enemy;
    }

    public NetworkBehaviour NetworkSpawnProjectile(string prefabName, Vector3 position)
    {
        string path = $"Network/Projectiles/{prefabName}";
        GameObject unit = Instantiate(Resources.Load(path)) as GameObject;
        unit.transform.position = position;
        unit.GetComponent<NetworkObject>().Spawn(true);

        // Debug.Log($"Spawning Enemy {prefabName} at {position}");

        NetworkBehaviour projectile = unit.GetComponent<NetworkBehaviour>();
        return projectile;
    }
}
