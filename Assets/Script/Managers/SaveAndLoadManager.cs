using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class WorldSaveData
{
    //번호
    public int Num;

    //좌표
    public float x;
    public float y;
    public float z;

    //방향
    public float xrot;
    public float yrot;
    public float zrot;
    public float wrot;

    //크기
    public float size;

    public List<BlockSaveData> blocks;

    public bool isLightBlock = false;
    public LightBlockSaveData lightblock;
}

[System.Serializable]
public class BlockSaveData
{
    //좌표
    public float x;
    public float y;
    public float z;

    //색
    public float r;
    public float g;
    public float b;
    public float a;
}

[System.Serializable]
public class LightBlockSaveData
{
    //좌표
    public float x;
    public float y;
    public float z;

    //방향
    public float xrot;
    public float yrot;
    public float zrot;
    public float wrot;

    //빛 관련 저장
    public float r;
    public float g;
    public float b;

    public int type; //1 -> Spot, 2 -> point
    public float SpotAngle;
    public float Range;
    public float Intensive;
}

[System.Serializable]
public class SaveAllData
{
    public int NumberOfWorld;

    public ColorSaveData maincolor;
    public List<ColorSaveData> precolor;
    public List<ColorSaveData> savecolor;
}

[System.Serializable]
public class ColorSaveData
{
    public float r;
    public float g;
    public float b;
    public float a;
}

public class SaveAndLoadManager : MonoBehaviour
{
    public static string RootPath => Application.persistentDataPath;
    public static string SaveFolder => System.IO.Path.Combine(RootPath, "Saves");

    public static void EnsureSaveFolder()
    {
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
            Debug.Log($"[Save] 폴더 생성: {SaveFolder}");
        }
    }

    private void Start()
    {
        EnsureSaveFolder();
        Debug.Log(Application.persistentDataPath);
        string[] files = Directory.GetFiles(SaveFolder);

        List<string> worldfile = new List<string>();

        if (files.Length > 0)
        {
            foreach (string file in files)
            {
                string fileName = System.IO.Path.GetFileName(file);

                if (fileName.StartsWith("Data_"))
                {
                    LoadData(file);
                }
                if (fileName.StartsWith("World_"))
                {
                    worldfile.Add(file);
                }
            }
        }

        if(worldfile.Count > 0)
        {
            foreach(string file in worldfile)
            {
                LoadWorldData(file);
            }
        }
    }

    public void SaveWorldData(WorldBaseCode _world)
    {
        EnsureSaveFolder();

        string fileName = $"World_{_world.Num.ToString()}.json";

        string path = System.IO.Path.Combine(SaveFolder, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[Save] 월드 삭제됨: " + _world.Num);
        }

        WorldSaveData worldsave = new WorldSaveData() 
        {
            Num = _world.Num,

            x = _world.gameObject.transform.position.x,
            y = _world.gameObject.transform.position.y,
            z = _world.gameObject.transform.position.z,

            xrot = _world.gameObject.transform.rotation.x,
            yrot = _world.gameObject.transform.rotation.y,
            zrot = _world.gameObject.transform.rotation.z,
            wrot = _world.gameObject.transform.rotation.w,

            size = _world.gameObject.transform.localScale.x,

            blocks = new List<BlockSaveData>()
        };

        foreach(Vector3Int blockposition in _world.Blocks.Keys)
        {
            BlockSaveData blocksave = new BlockSaveData()
            {
                x= blockposition.x,
                y= blockposition.y,
                z= blockposition.z,

                r = _world.Blocks[blockposition].go.GetComponent<MeshRenderer>().material.color.r,
                g = _world.Blocks[blockposition].go.GetComponent<MeshRenderer>().material.color.g,
                b = _world.Blocks[blockposition].go.GetComponent<MeshRenderer>().material.color.b,
                a = _world.Blocks[blockposition].go.GetComponent<MeshRenderer>().material.color.a
            };

            worldsave.blocks.Add(blocksave);
        }

        if(_world.LightBlock != null)
        {
            worldsave.isLightBlock = true;

            GameObject LB = _world.LightBlock.gameObject;
            Light L = LB.GetComponent<Light>();
            int _type = 0;
            if (L.type == LightType.Spot)
            {
                _type = 1;
            }
            if (L.type == LightType.Point)
            {
                _type = 2;
            }


            worldsave.lightblock = new LightBlockSaveData()
            {
                x= LB.transform.position.x,
                y= LB.transform.position.y,
                z= LB.transform.position.z,

                xrot = LB.transform.rotation.x,
                yrot = LB.transform.rotation.y,
                zrot = LB.transform.rotation.z,
                wrot = LB.transform.rotation.w,

                r = L.color.r,
                g = L.color.g,
                b = L.color.b,

                type = _type,
                SpotAngle = L.spotAngle,
                Range = L.range,
                Intensive = L.intensity,
            };
        }

        string json = JsonUtility.ToJson(worldsave, true);
        File.WriteAllText(path, json);
    }

    public void DeleteData(WorldBaseCode _world)
    {
        EnsureSaveFolder();

        string fileName = $"World_{_world.Num.ToString()}.json";

        string path = System.IO.Path.Combine(SaveFolder, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[Save] 월드 삭제됨: " + _world.Num);
        }
    }

    public void LoadWorldData(string path)
    {
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        WorldSaveData worldsave = JsonUtility.FromJson<WorldSaveData>(json);

        GameObject world = GameManager.Instance.WorldManager.MakeWorld();

        world.transform.position = new Vector3(worldsave.x, worldsave.y, worldsave.z);
        world.transform.rotation = new Quaternion(
            worldsave.xrot,
            worldsave.yrot,
            worldsave.zrot,
            worldsave.wrot
        );
        world.transform.localScale = Vector3.one * worldsave.size;

        WorldBaseCode worldBaseCode = world.GetComponent<WorldBaseCode>();

        worldBaseCode.Num = worldsave.Num;
        foreach (BlockSaveData block in worldsave.blocks)
        {

           /* GameManager.Instance.BuildManager.RequestBuild
                (new WorldAndBlock
                {
                    world = worldBaseCode,
                    blockposition = new Vector3(block.x, block.y, block.z),
                    blockcolor = new Color(block.r, block.g, block.b, block.a),
                    collider = false
                });*/
        }

        if(worldsave.isLightBlock)
        {
            Light light = worldBaseCode.ChangeLightPower();
            light.gameObject.transform.position = new Vector3(worldsave.lightblock.x, worldsave.lightblock.y, worldsave.lightblock.z);
            light.gameObject.transform.rotation = new Quaternion(worldsave.lightblock.xrot, worldsave.lightblock.yrot, worldsave.lightblock.zrot, worldsave.lightblock.wrot);
            light.color = new Color(worldsave.lightblock.r, worldsave.lightblock.g, worldsave.lightblock.b);
            switch (worldsave.lightblock.type)
            {
                case 1:
                    light.type = LightType.Spot;
                    break;
                case 2:
                    light.type = LightType.Point;
                    break;
            }
            light.spotAngle = worldsave.lightblock.SpotAngle;
            light.range = worldsave.lightblock.Range;
            light.intensity = worldsave.lightblock.Intensive;
        }
    }

    public void SaveData()
    {
        string fileName = $"Data_personal.json";

        string path = System.IO.Path.Combine(SaveFolder, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        BuildManager BM = GameManager.Instance.BuildManager;

        SaveAllData savedata = new SaveAllData()
        {
            NumberOfWorld = GameManager.Instance.WorldManager.HowManyYouMakeWorld,

            maincolor = new ColorSaveData()
            {
                r = BM.MainColor.color.r,
                g = BM.MainColor.color.g,
                b = BM.MainColor.color.b,
                a = BM.alfar
            },

            precolor = new List<ColorSaveData>(),
            savecolor = new List<ColorSaveData>()
        };

        if (BM.PreColors.Count > 0)
        {
            foreach (Color color in BM.PreColors)
            {
                savedata.precolor.Add(new ColorSaveData()
                {
                    r = color.r,
                    g = color.g,
                    b = color.b,
                    a = color.a
                });
            }
        }

        if (BM.SaveColors.Count > 0)
        {
            foreach (Color color in BM.SaveColors)
            {
                savedata.savecolor.Add(new ColorSaveData()
                {
                    r = color.r,
                    g = color.g,
                    b = color.b,
                    a = color.a
                });
            }
        }

        string json = JsonUtility.ToJson(savedata, true);
        File.WriteAllText(path, json);
    }

    public void LoadData(string path)
    {
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        SaveAllData worldsave = JsonUtility.FromJson<SaveAllData>(json);

        GameManager.Instance.WorldManager.HowManyYouMakeWorld = worldsave.NumberOfWorld;

        BuildManager BM = GameManager.Instance.BuildManager;

        BM.MainColor.color =  new Color(worldsave.maincolor.r, worldsave.maincolor.g, worldsave.maincolor.b, worldsave.maincolor.a);

        if (worldsave.precolor.Count > 0)
        {
            foreach (ColorSaveData color in worldsave.precolor)
            {
                BM.AddPreColor(new Color(color.r, color.g, color.b, color.a));
            }
        }
        if (worldsave.savecolor.Count > 0)
        {
            for(int i = 0; i < worldsave.savecolor.Count; i++)
            {
                BM.AddSaveColor(new Color(worldsave.savecolor[i].r, worldsave.savecolor[i].g, worldsave.savecolor[i].b, worldsave.savecolor[i].a), i);
            }
        }
    }

    public void SaveAllWorld()
    {
        foreach(GameObject wb in GameManager.Instance.WorldManager.Worlds)
        {
            //SaveWorldData(wb.GetComponent<WorldBaseCode>());
        }
    }
}
