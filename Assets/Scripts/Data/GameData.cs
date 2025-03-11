using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    public List<CharacterData> Characters = new List<CharacterData>();
    public List<QuipData> Quips = new List<QuipData>();
    public List<EnemyData> Enemies = new List<EnemyData>();
    public List<MaterialData> Materials = new List<MaterialData>();
    public Dictionary<int, SpawnRatesData> SpawnWavesPower = new Dictionary<int, SpawnRatesData>();

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ParseMaterials();
            ParseQuips();
            ValidateQuips();
            ParseCharacters();
            ParseEnemies();
            PraseSpawnRates();
        }
    }

    void ParseCharacters()
    {
        List<Dictionary<string, object> > dataList = CSVReaderGPT.Read("Data/characters");
        foreach (var data in dataList)
        {
            int id = (int)data["id"];
            string name = (string)data["name"];
            string description = (string)data["description"];
            CharacterData character = new CharacterData(id, name, description);

            int quip0 = (int)data["quip_0"];
            character.AddQuip(GetQuipData(quip0));

            if(data.ContainsKey("quip_1"))
            {
                int quip1 = (int)data["quip_1"];
                character.AddQuip(GetQuipData(quip1));
            }

            if(data.ContainsKey("quip_2"))
            {
                int quip1 = (int)data["quip_2"];
                character.AddQuip(GetQuipData(quip1));
            }

            Characters.Add(character);
        }
    }

    void ParseQuips()
    {
        List<Dictionary<string, object> > dataList = CSVReaderGPT.Read("Data/quips");
        foreach (var data in dataList)
        {
            int id = (int)data["id"];
            string name = (string)data["name"];
            string command = (string)data["command"];
            int activationTime = (int)data["activation_time"];
            int cooldown = (int)data["cooldown"];
            float mainRadius = (float)data["main_radius"];
            float secondRadius = (float)data["second_radius"];
            float aoe = (float)data["area_of_effect"];
            int damage = (int)data["damage"];
            float size = (float)data["size"];
            float range = (float)data["range"];
            float minRange = (float)data["min_range"];
            int rounds = (int)data["rounds"];
            string image = (string)data["image"];
            string description = (string)data["description"];
            string quipPrefab = (string)data["quip_prefab"];
            string unitPrefab = string.Empty;
            if(data.ContainsKey("unit_prefab"))
                unitPrefab = (string)data["unit_prefab"];

            QuipData quip = new QuipData(id, name, ParseCommand(command), activationTime, cooldown, mainRadius, secondRadius,
                                            aoe, damage, size, range, minRange, rounds, image, description, quipPrefab, unitPrefab);

            foreach(MaterialData mat in Materials)
            {
                if(data.ContainsKey(mat.Name))
                {
                    int price = (int)data[mat.Name];
                    quip.AddMaterialPrice(mat, price);
                }
            }
            
            Quips.Add(quip);
        }
    }

    void ValidateQuips()
    {
        foreach(QuipData quipA in Quips)
        {
            foreach(QuipData quipB in Quips)
            {
                if(quipA.Id != quipB.Id)
                {
                    if(quipA.SequenceWithinCommand(quipB.Command))
                    {
                        Debug.LogWarning($"{quipB.Name} Command is within {quipA} Command");
                    }
                }
            }
        }
    }

    void ParseEnemies()
    {
        List<Dictionary<string, object> > dataList = CSVReaderGPT.Read("Data/enemies");
        foreach (var data in dataList)
        {
            int id = (int)data["id"];
            string prefab_name = (string)data["prefab_name"];
            int power = (int)data["power"];
            int min_time = (int)data["min_time"];
            float radius = (float)data["radius"];
            
            EnemyData enemy = new EnemyData(id, prefab_name, power, min_time, radius);
            Enemies.Add(enemy);
        }
    }

    void ParseMaterials()
    {
        List<Dictionary<string, object> > dataList = CSVReaderGPT.Read("Data/materials");
        foreach (var data in dataList)
        {
            int id = (int)data["id"];
            string name = (string)data["name"];
            int price = (int)data["price"];
            string image = (string)data["image"];
            string description = (string)data["description"];

            MaterialData material = new MaterialData(id, name, price, image, description);
            Materials.Add(material);
        }
    }

    void PraseSpawnRates()
    {
        List<Dictionary<string, object> > dataList = CSVReaderGPT.Read("Data/spawn_rates");
        foreach (var data in dataList)
        {
            int players = (int)data["players"];
            SpawnRatesData srd = new SpawnRatesData();

            for (int i = 0; i <= 30; i++)
            {
                int power = (int)data[i.ToString()];
                srd.AddWavePower(i, power);
            }
            
            SpawnWavesPower[players] = srd;
        }
    }

    public CharacterData GetCharacterData(int id)
    {
        foreach(CharacterData cd in Characters)
        {
            if(cd.Id == id)
            {
                return cd;
            }
        }
        Debug.LogError($"Could not find character with Id: {id}");
        return null;
    }

    public QuipData GetQuipData(int id)
    {
        foreach (QuipData data in Quips)
        {
            if(data.Id == id)
            {
                return data;
            }
        }
        Debug.LogError($"Could not find Quip by Id: {id}");
        return null;
    }

    public EnemyData GetEnemyData(int id)
    {
        foreach(EnemyData enemy in Enemies)
        {
            if(enemy.Id == id)
            {
                return enemy;
            }
        }
        Debug.LogError($"Could not find Enemy by Id: {id}");
        return null;
    }

    public MaterialData GetMaterial(int id)
    {
        foreach(MaterialData material in Materials)
        {
            if(material.Id == id)
            {
                return material;
            }
        }
        Debug.LogError($"Could not find Material by Id: {id}");
        return null;
    }

    public MaterialData GetMaterial(string name)
    {
        foreach(MaterialData material in Materials)
        {
            if(material.Name == name)
            {
                return material;
            }
        }
        Debug.LogError($"Could not find Material by Name: {name}");
        return null;
    }

    List<Arrow> ParseCommand(string commandString)
    {
        List<Arrow> command = new List<Arrow>();
        foreach(char c in commandString)
        {
            if(c == 'W')
            {
                command.Add(Arrow.UP);
            }
            if(c == 'S')
            {
                command.Add(Arrow.DOWN);
            }
            if(c == 'A')
            {
                command.Add(Arrow.LEFT);
            }
            if(c == 'D')
            {
                command.Add(Arrow.RIGHT);
            }
        }
        return command;
    }
}
