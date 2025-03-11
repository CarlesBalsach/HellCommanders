using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

public class HCMap : MonoBehaviour
{
    const string BASE_PREFAB_NAME = "HCAllyBase";
    const float SPAWN_TIME = 10f;
    const int CLUSTERS_PER_WAVE = 2;
    const float EPSILON = 0.001f;

    public static HCMap Instance;
    public const int SIZE = 50;

    [SerializeField] ShipAnimations ShipAnimation;

    public List<HCUnit> AllyUnits = new List<HCUnit>(); // Server only
    public List<HCUnit> EnemyUnits = new List<HCUnit>(); // Server only
    public List<HCUnit> UnitsSpawned = new List<HCUnit>();

    float _spawnChrono = 0f;
    int _numWaves = 0;

    private void Awake()
    {
        HCGameManager.Instance.State.OnValueChanged += OnGameStateChanged;
        ShipAnimation.gameObject.SetActive(false);
        Instance = this;
    }

    private void OnDestroy()
    {
        HCGameManager.Instance.State.OnValueChanged -= OnGameStateChanged;
        Instance = null;
        DOTween.KillAll();
    }

    private void Update()
    {
        if(NetworkManager.Singleton.IsServer && HCGameManager.Instance.State.Value == HCGameManager.GameState.GAME_ACTIVE)
        {
            SpawnPlanetEnemiesLogic();
        }
    }

    private void OnGameStateChanged(HCGameManager.GameState previousValue, HCGameManager.GameState newValue)
    {
        if(newValue == HCGameManager.GameState.GAME_LANDING_PLANET)
        {
            ShipAnimation.gameObject.SetActive(true);
            ShipAnimation.StartShipLandAnimation();
        }
        if(newValue == HCGameManager.GameState.GAME_ACTIVE && NetworkManager.Singleton.IsServer)
        {
            SpawnAlly(BASE_PREFAB_NAME, Vector3.zero);
            SpawnBaseCannons();
            _spawnChrono = -SPAWN_TIME;
            _numWaves = 0;
        }
        if(newValue == HCGameManager.GameState.GAME_FAILED)
        {
            ShipAnimation.StartShipDestroyedAnimation();
        }
        if(newValue == HCGameManager.GameState.GAME_DEPARTING_PLANET)
        {
            ShipAnimation.StartShipDepartingAniation();
        }
    }

    void SpawnBaseCannons()
    {
        int players = HCNetworkManager.Instance.PlayersConnected();
        for(int i = 0; i < players; i++)
        {
            PlayerDataNetwork pdata = HCNetworkManager.Instance.GetPlayerDataFromPlayerIndex(i);
            PrefabController.Instance.NetworkSpawnBaseCannon(i, players, pdata.ClientId);
        }
    }

    public void SpawnAlly(string prefabName, Vector3 position) // Server
    {
        HCUnit ally = PrefabController.Instance.NetworkSpawnAlly(prefabName, position);

        for (int i = 0; i < AllyUnits.Count; i++)
        {
            HCUnit unit = AllyUnits[i];
            if(HCUtils.InDistance2D(ally.transform.position, unit.transform.position, ally.Stats.Radius + unit.Stats.Radius))
            {
                unit.Die();
            }
        }

        AllyUnits.Add(ally);
    }

    HCUnit SpawnEnemy(string prefabName, Vector3 position) // Server
    {
        HCUnit enemy = PrefabController.Instance.NetworkSpawnEnemy(prefabName, position);
        EnemyUnits.Add(enemy);
        return enemy;
    }

    public void SpawnFire(Vector3 position, float radius) // Server Only
    {
        if(NetworkManager.Singleton.IsServer)
        {
            NetworkBehaviour fireAreaNB = PrefabController.Instance.NetworkSpawnEffect("FireArea", position);
            fireAreaNB.GetComponent<HCFireArea>().Init(radius);
        }
        else
        {
            Debug.LogError("Trying to Spawn Fire when not the server");
        }
    }

    void SpawnPlanetEnemiesLogic() // Server Only
    {
        float time_elapsed = HCGameManager.Instance.StageTime.Value;
        if(time_elapsed - _spawnChrono >= SPAWN_TIME)
        {
            _spawnChrono += SPAWN_TIME;
            int numPlayers = HCNetworkManager.Instance.PlayersConnected();
            int wavesToSpawn = CLUSTERS_PER_WAVE * numPlayers;
            int wavePower = GetWavePower(numPlayers);
            for (int i = 0; i < wavesToSpawn; i++)
            {
                SpawnWave(wavePower);
            }
            _numWaves++;
        }
    }

    int GetWavePower(int numPlayers)
    {
        int playerLayer = HCGameManager.Instance.CurrentPlanet.Value.Layer;
        int planetInitialPower = (playerLayer - 1) * 10;
        float planetMultiplier = 1f + (playerLayer - 1) * 0.1f;
        float playersMultiplier = 1f + (numPlayers - 1) * 0.1f;

        SpawnRatesData srd = GameData.Instance.SpawnWavesPower[numPlayers];
        int wavePower = srd.GetWavePower(_numWaves);

        return (int)((planetInitialPower + wavePower) * planetMultiplier * playersMultiplier);
    }

    void SpawnWave(int power)
    {
        int time = 10 * (HCGameManager.Instance.CurrentPlanet.Value.Layer - 1) + (int)HCGameManager.Instance.StageTime.Value;
        List<EnemyData> enemiesEligible = new List<EnemyData>(0);
        foreach(EnemyData enemy in GameData.Instance.Enemies)
        {
            if(enemy.Power <= power && enemy.MinTime <= time)
            {
                enemiesEligible.Add(enemy);
            }
        }
        enemiesEligible.Sort((a, b)=> b.Power.CompareTo(a.Power));
        List<EnemyData> chosenEnemies = new List<EnemyData>();
        while(chosenEnemies.Count < 10) // Max of 10 enemies mostly for performance reasons
        {
            while(enemiesEligible.Count > 0 && enemiesEligible[0].Power > power)
            {
                enemiesEligible.RemoveAt(0);
            }
            if(enemiesEligible.Count == 0)
            {
                break;
            }

            for(int i = 0; i < enemiesEligible.Count; i++)
            {
                int rand = UnityEngine.Random.Range(0, 2);
                if(rand == 0 || i == enemiesEligible.Count - 1)
                {
                    EnemyData enemy = enemiesEligible[i];
                    chosenEnemies.Add(enemy);
                    power -= enemy.Power;
                    break;
                }
            }
        }

        SpawnEnemies(chosenEnemies);
    }

    void SpawnEnemies(List<EnemyData> enemies)
    {
        Vector3 center = GetWaveSpawnPosition();
        enemies.Sort((a, b) => b.Radius.CompareTo(a.Radius));

        List<HCUnit> enemiesPlaced = new List<HCUnit>();

        for (int i = 0; i < enemies.Count; i++)
        {
            if(i == 0)
            {
                enemiesPlaced.Add(SpawnEnemy(enemies[i].PrefabName, center));
            }
            else if(i == 1)
            {
                Vector2 dir = HCUtils.RotateVector2(Vector2.up, UnityEngine.Random.Range(0, 360f));
                Vector3 pos = center + dir.ToV3() * (enemies[0].Radius + enemies[1].Radius);
                enemiesPlaced.Add(SpawnEnemy(enemies[i].PrefabName, pos));
            }
            else
            {
                Vector3 pos = FindPositionForEnemy(enemiesPlaced, enemies[i]);
                enemiesPlaced.Add(SpawnEnemy(enemies[i].PrefabName, pos));
            }
        }
    }

    Vector3 FindPositionForEnemy(List<HCUnit> enemies, EnemyData enemy)
    {
        List<Circle> circles = new List<Circle>();
        foreach(HCUnit unit in enemies)
        {
            circles.Add(new Circle(unit.transform.position, unit.Stats.Radius + enemy.Radius));
        }

        List<Vector2> intersections = new List<Vector2>();
        for (int i = 0; i < circles.Count; i++)
        {
            for (int j = i + 1; j < circles.Count; j++)
            {
                intersections.AddRange(circles[i].IntersectionPoints(circles[j]));
            }
        }

        for(int i = intersections.Count - 1; i >= 0; i--)
        {
            bool valid = true;
            foreach(HCUnit unit in enemies)
            {
                float distance = (unit.transform.position.ToV2() - intersections[i]).magnitude;
                if(distance < unit.Stats.Radius + enemy.Radius - EPSILON)
                {
                    valid = false;
                    break;
                }
            }
            if(!valid)
            {
                intersections.RemoveAt(i);
            }
        }

        Vector2 pos = intersections[0];
        float min_distance = (enemies[0].transform.position.ToV2() - pos).magnitude;
        for(int i = 1; i < intersections.Count; i++)
        {
            float distance = (enemies[0].transform.position.ToV2() - intersections[i]).magnitude;
            if(distance < min_distance)
            {
                pos = intersections[i];
                min_distance = distance;
            }
        }

        return pos;
    }

    Vector3 GetWaveSpawnPosition()
    {
        int rand = UnityEngine.Random.Range(0, 4);
        Vector3 spawnPos = Vector3.zero;
        Vector2 dir = Vector2.zero;
        switch(rand) {
            case 0:
                spawnPos = new Vector3(-SIZE/2f, SIZE / 2f, 0f);
                dir = Vector2.right;
                break;
            case 1:
                spawnPos = new Vector3(SIZE/2f, SIZE / 2f, 0f);
                dir = Vector2.down;
                break;
            case 2:
                spawnPos = new Vector3(SIZE/2f, -SIZE / 2f, 0f);
                dir = Vector2.left;
                break;
            case 3:
                spawnPos = new Vector3(-SIZE/2f, -SIZE / 2f, 0f);
                dir = Vector2.up;
                break;
        }

        float randF = UnityEngine.Random.Range(0f, SIZE);
        spawnPos += dir.ToV3() * randF;
        return spawnPos;
    }

    public void RemoveUnit(HCUnit unit)
    {
        for (int i = 0; i < AllyUnits.Count; i++)
        {
            if(AllyUnits[i].UId == unit.UId)
            {
                AllyUnits.RemoveAt(i);
                break;
            }
        }
        for (int i = 0; i < EnemyUnits.Count; i++)
        {
            if(EnemyUnits[i].UId == unit.UId)
            {
                EnemyUnits.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; i < UnitsSpawned.Count; i++)
        {
            if(UnitsSpawned[i].UId == unit.UId)
            {
                UnitsSpawned.RemoveAt(i);
                break;
            }
        }
    }

    public void UnitSpawned(HCUnit unit)
    {
        UnitsSpawned.Add(unit);
    }
}
