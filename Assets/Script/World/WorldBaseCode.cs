using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

[System.Flags]
public enum BlockFace
{
    None = 0,
    Up = 1 << 0,
    Down = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3,
    Front = 1 << 4,
    Back = 1 << 5,

    All = Up | Down | Left | Right | Front | Back
}

public class BlockData
{
    public GameObject go;
    public bool IsTransparent;
}

public class WorldBaseCode : MonoBehaviour
{
    static readonly Vector3Int[] CheckDirs =
    {
    Vector3Int.up,
    Vector3Int.down,
    Vector3Int.left,
    Vector3Int.right,
    new Vector3Int(0, 0, 1),   // forward
    new Vector3Int(0, 0, -1),  // back
    };

    public Dictionary<Vector3Int, BlockData> Blocks;
    GameObject LightBlock;
    public int Num;

    float Hiestest_X = 0;
    float Lowest_X = 0;
    float Hiestest_Y = 0;
    float Lowest_Y = 0;
    float Hiestest_Z = 0;
    float Lowest_Z = 0;

    BoxCollider boxCollider;

    public int LightPower = 0;

    public void MakeNewWorld(int _x, int _y, int _z, int _num = 0)
    {
        if(_num != 0) Num = _num;

        if (Blocks == null) Blocks = new Dictionary<Vector3Int, BlockData>();
        if (boxCollider == null) boxCollider = gameObject.AddComponent<BoxCollider>();

        List<Vector3Int> CheckBlocks = new List<Vector3Int>(Blocks.Keys);

        for (int x = 0; x < _x; x++)
        {
            for (int y = 0; y < _y; y++)
            {
                for (int z = 0; z < _z; z++)
                {
                    Vector3Int v3 = new Vector3Int(x, y, z);
                    if (Blocks.ContainsKey(v3))
                    {
                        Checkblockpos(v3);
                    }
                    else
                    {
                        MakeNewBlocks(GameManager.Instance.BuildManager.BasicBlock, new Vector3(x, y, z));
                    }

                    if(CheckBlocks.Contains(v3)) CheckBlocks.Remove(v3);
                }
            }
        }

        foreach(Vector3Int vec in CheckBlocks)
        {
            DestroyBlock(Blocks[vec].go);
        }

        LightPower = 0;
        StartChangeEveryThingSmooth(0.2f, 1f / (Mathf.Max(_x, _y, _z)), transform.position, transform.rotation, true);
    }

    public void DestroyAllBlocks(bool destroyWorld = false)
    {
        foreach (BlockData block in Blocks.Values)
        {
            block.go.SetActive(false);
        }
        DestroyNoneSeenBlocks(destroyWorld);
        Blocks.Clear();
    }

    public void DestroyNoneSeenBlocks(bool isDestroy)
    {
        var removeList = new List<Vector3Int>();

        foreach (var pair in Blocks)
        {
            if (!pair.Value.go.activeSelf)
                removeList.Add(pair.Key);
        }

        foreach (var key in removeList)
        {
            DestroyBlock(Blocks[key].go, isDestroy);
        }
    }

    void CheckSize(Vector3 _position)
    {
        if (Hiestest_X < _position.x) //x위치 조정
        {
            Hiestest_X = _position.x;
        }
        else if (Lowest_X > _position.x)
        {
            Lowest_X = _position.x;
        }

        if (Hiestest_Y < _position.y) //y위치 조정
        {
            Hiestest_Y = _position.y;
        }
        else if (Lowest_Y > _position.y)
        {
            Lowest_Y = _position.y;
        }

        if (Hiestest_Z < _position.z) //z위치 조정
        {
            Hiestest_Z = _position.z;
        }
        else if (Lowest_Z > _position.z)
        {
            Lowest_Z = _position.z;
        }

        SettingMiddle();
    }

    public void MakeNewBlocks(GameObject _block, Vector3 _position)
    {
        if (Blocks == null) Blocks = new Dictionary<Vector3Int, BlockData>();
        if (boxCollider == null) boxCollider = gameObject.AddComponent<BoxCollider>();

        Vector3Int gridPos = Vector3Int.RoundToInt(_position);

        GameObject block = Instantiate(_block, transform);
        block.AddComponent<BaseBlockCode>()
             .SettingPosition(_position.x, _position.y, _position.z);
        block.layer = 6;

        BlockData newData = new BlockData
        {
            go = block,
            IsTransparent = false, // 일단 기본값
        };
        
        Blocks.Add(gridPos, newData);

        OnBlockPlaced(gridPos);

        UpdateBlockMesh(gridPos);
        foreach (var dir in CheckDirs) UpdateBlockMesh(gridPos + dir);

        CheckSize(_position);
    }

    void Checkblockpos(Vector3Int pos)
    {
        Vector3Int gridPos = Vector3Int.RoundToInt(pos);

        OnBlockPlaced(gridPos);

        UpdateBlockMesh(gridPos);
        foreach (var dir in CheckDirs) UpdateBlockMesh(gridPos + dir);

        CheckSize(pos);
    }

    public void MakeNewBlocks(GameObject _block, Vector3 _position, Color _color, float _alfar)
    {
        if (Blocks == null) Blocks = new Dictionary<Vector3Int, BlockData>();
        if (boxCollider == null) boxCollider = gameObject.AddComponent<BoxCollider>();

        Vector3Int gridPos = Vector3Int.RoundToInt(_position);

        GameObject block = Instantiate(_block, transform);
        block.AddComponent<BaseBlockCode>()
             .SettingPosition(_position.x, _position.y, _position.z);
        block.layer = 6;

        block.GetComponent<BaseBlockCode>().SetColor(_color);
        block.GetComponent<BaseBlockCode>().SetColor(_alfar);

        BlockData newData = new BlockData
        {
            go = block,
            IsTransparent = false, // 일단 기본값
        };
        if(_alfar != 1)
        {
            newData.IsTransparent = true;
        }

        Blocks.Add(gridPos, newData);

        OnBlockPlaced(gridPos);

        UpdateBlockMesh(gridPos);
        foreach (var dir in CheckDirs) UpdateBlockMesh(gridPos + dir);

        CheckSize(_position);
    }

    public void ChooseThisWorld()
    {
        boxCollider.enabled = false;

        foreach (BlockData block in Blocks.Values)
        {
            block.go.GetComponent<BoxCollider>().enabled = true;
        }
    }

    public void PutThisWorld()
    {
        boxCollider.enabled = true;

        foreach (BlockData block in Blocks.Values)
        {
            block.go.GetComponent<BoxCollider>().enabled = false;
        }
    }

    void SettingMiddle()
    {
        boxCollider.center = new Vector3((Hiestest_X + Lowest_X) / 2f, (Hiestest_Y + Lowest_Y) / 2f, (Hiestest_Z + Lowest_Z) / 2f);
        boxCollider.size = new Vector3(Hiestest_X - Lowest_X + 1, Hiestest_Y - Lowest_Y + 1, Hiestest_Z - Lowest_Z + 1);
    }

    public void StartChangeEveryThingSmooth(float duration, float _scale, Vector3 _position, Quaternion _rotation, bool isFollow = false)
    {
        StartCoroutine(ChangeEveryThingSmooth(duration, _scale, _position, _rotation, isFollow));
    }

    IEnumerator ChangeEveryThingSmooth(float duration, float _scale, Vector3 _position, Quaternion _rotation, bool isFollow)
    {
        float biggestsize = Mathf.Max(
            Hiestest_X - Lowest_X + 1,
            Hiestest_Y - Lowest_Y + 1,
            Hiestest_Z - Lowest_Z + 1
        );

        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one * _scale;
        if (!isFollow) endScale = endScale / biggestsize;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = _position;

        Quaternion startRotation = transform.localRotation;
        Quaternion endRotation = _rotation;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            transform.position = Vector3.Lerp(startPosition, endPosition, time / duration); 
            transform.localRotation = Quaternion.Lerp(startRotation, endRotation, time / duration);
            yield return null;
        }

        transform.localScale = endScale;

        GameManager.Instance.SaveAndLoadManager.SaveWorldData(this);

        if (!isFollow) GameManager.Instance.ChangeFlow(GameFlow.PutWorldInSpace);
    }

    public void DestroyBlock(GameObject _destroyedblock, bool isdelete = true)
    {
        Vector3Int keyToRemove = Vector3Int.zero;

        foreach (var pair in Blocks)
        {
            if (pair.Value.go == _destroyedblock)
            {
                keyToRemove = pair.Key;
                break;
            }
        }

        if (Blocks.ContainsKey(keyToRemove)) Blocks.Remove(keyToRemove);
        Destroy(_destroyedblock);

        OnBlockDestroyed(keyToRemove);

        foreach (var dir in CheckDirs) UpdateBlockMesh(keyToRemove + dir);

        if (Blocks.Count == 0 && isdelete)
        {
            GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
            GameManager.Instance.WorldManager.DestroyWorld(gameObject);
        }
    }

    public GameObject MakePreview(int _num)
    {
        GameObject PreviewObject = new GameObject();
        PreviewObject.transform.SetParent(Camera.main.transform, false);
        PreviewObject.transform.localRotation = gameObject.transform.localRotation;
        PreviewObject.transform.localScale = gameObject.transform.localScale;

        foreach (BlockData _block in Blocks.Values)
        {
            GameObject block = _block.go;
            GameObject _go = new GameObject();
            _go.transform.SetParent(PreviewObject.transform, false);

            _go.transform.localPosition = block.transform.localPosition;
            _go.transform.localRotation = block.transform.localRotation;
            _go.transform.localScale = block.transform.localScale;

            _go.layer = _num;

            MeshFilter srcMF = block.GetComponent<MeshFilter>();
            if (srcMF != null)
            {
                MeshFilter mf = _go.AddComponent<MeshFilter>();
                
                mf.sharedMesh = srcMF.sharedMesh;
            }

            MeshRenderer srcMR = block.GetComponent<MeshRenderer>();
            if (srcMR != null)
            {
                MeshRenderer mr = _go.AddComponent<MeshRenderer>();
                mr.sharedMaterials = srcMR.sharedMaterials;

                mr.material = GameManager.Instance.MaterialManager.PreviewMaterial;
            }
        }

        GameManager.Instance.ChangeFlow(GameFlow.PutWorldInSpace);
        return PreviewObject;
    }

    bool IsFullyCovered(Vector3Int pos)
    {
        foreach (var dir in CheckDirs)
        {
            Vector3Int nPos = pos + dir;

            if (!Blocks.TryGetValue(nPos, out BlockData n))
                return false;

            if (n.IsTransparent)
                return false;
        }
        return true;
    }

    public void OnBlockPlaced(Vector3Int pos)
    {
        UpdateSelfVisibility(pos);

        UpdateAroundVisibility(pos);
    }

    void UpdateSelfVisibility(Vector3Int pos)
    {
        if (!Blocks.TryGetValue(pos, out BlockData data))
            return;

        bool covered = IsFullyCovered(pos);
        data.go.SetActive(!covered);
    }

    void UpdateAroundVisibility(Vector3Int pos)
    {
        foreach (var dir in CheckDirs)
        {
            Vector3Int nPos = pos + dir;

            if (!Blocks.TryGetValue(nPos, out BlockData neighbor))
                continue;

            bool covered = IsFullyCovered(nPos);
            neighbor.go.SetActive(!covered);
        }
    }

    public void OnBlockDestroyed(Vector3Int pos)
    {
        foreach (var dir in CheckDirs)
        {
            Vector3Int nPos = pos + dir;

            if (!Blocks.TryGetValue(nPos, out BlockData neighbor))
                continue;

            neighbor.go.SetActive(true);
        }
    }
    BlockFace GetVisibleFaces(Vector3Int pos, BlockData data)
    {
        BlockFace faces = BlockFace.All;

        foreach (var dir in CheckDirs)
        {
            Vector3Int nPos = pos + dir;

            if (!Blocks.TryGetValue(nPos, out BlockData n))
                continue;

            if (n.IsTransparent != data.IsTransparent)
                continue;

            if (dir == Vector3Int.up) faces &= ~BlockFace.Up;
            if (dir == Vector3Int.down) faces &= ~BlockFace.Down;
            if (dir == Vector3Int.left) faces &= ~BlockFace.Left;
            if (dir == Vector3Int.right) faces &= ~BlockFace.Right;
            if (dir == new Vector3Int(0, 0, 1)) faces &= ~BlockFace.Front;
            if (dir == new Vector3Int(0, 0, -1)) faces &= ~BlockFace.Back;
        }

        return faces;
    }

    void UpdateBlockMesh(Vector3Int pos)
    {
        if (!Blocks.TryGetValue(pos, out BlockData data))
            return;

        var meshGen = data.go.GetComponent<BlockMeshGenerator>();
        meshGen.Build(GetVisibleFaces(pos, data));
    }

    public Light ChangeLightPower()
    {
        if (LightBlock == null) LightBlock = Instantiate(new GameObject(), gameObject.transform);

        Light light;
        LightBlock.TryGetComponent<Light>(out light);
        if (light == null)
        {
            light = LightBlock.AddComponent<Light>();
            light.type = UnityEngine.LightType.Point;
            light.range = 0;
            light.intensity = 0;
            LightBlock.transform.localPosition = new Vector3(0, 0, 0);
        }

        return light;
    }


    //빛은 제거하자 굳이? world단위 빛만 만들자
    /* void UpdateBlockLightOnSpawn(Vector3Int lightPos, Color lightColor, int range)
    {
        // 범위 내 블럭 수집 (정확도 + 성능 위해 HashSet)
        HashSet<Vector3Int> targets = new HashSet<Vector3Int>();

        for (int x = lightPos.x - range; x <= lightPos.x + range; x++)
        {
            for (int y = lightPos.y - range; y <= lightPos.y + range; y++)
            {
                for (int z = lightPos.z - range; z <= lightPos.z + range; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);

                    if (!Blocks.TryGetValue(pos, out BlockData data))
                        continue;

                    if (!data.go.activeSelf)
                        continue;

                    float dist = Vector3Int.Distance(lightPos, pos);
                    if (dist > range)
                        continue;

                    targets.Add(pos);
                }
            }
        }

        if (targets.Count == 0) return;

        // 각 블럭에 대해 빛 계산
        foreach (Vector3Int targetPos in targets)
        {
            if (targetPos == lightPos)
                continue;

            Vector3 dir = ((Vector3)(targetPos - lightPos)).normalized;
            float distance = Vector3Int.Distance(lightPos, targetPos);

            Color finalLight = lightColor;
            bool blocked = false;

            Vector3Int prevPos = lightPos;

            // 광선 추적 (step 방식)
            float step = 0.25f;
            for (float t = step; t < distance; t += step)
            {
                Vector3 sample = lightPos + dir * t;
                Vector3Int samplePos = Vector3Int.RoundToInt(sample);

                if (samplePos == prevPos || samplePos == lightPos || samplePos == targetPos)
                    continue;

                prevPos = samplePos;

                if (!Blocks.TryGetValue(samplePos, out BlockData midData))
                    continue;

                if (!midData.go.activeSelf)
                    continue;

                BaseBlockCode midBlock = midData.go.GetComponent<BaseBlockCode>();
                Color midColor = midBlock.OriginalColor;

                // 완전 불투명 → 차단
                if (midColor.a >= 1f)
                {
                    blocked = true;
                    break;
                }

                // 반투명 → 색 필터링
                if (midColor.a > 0f)
                {
                    float attenuate = 1f - (Vector3Int.Distance(lightPos, samplePos) / range);
                    finalLight += midColor * midColor.a * attenuate;
                }
            }

            if (blocked)
                continue;

            float attenuation = Mathf.Pow(1f - (distance / range), 3f);
            float intensity = Mathf.Clamp01(attenuation);
            Color applied = finalLight * intensity;
            applied.a = 1f;

            BaseBlockCode targetBlock =
                Blocks[targetPos].go.GetComponent<BaseBlockCode>();

            targetBlock.AddLight(applied);
        }
    }*/
}
