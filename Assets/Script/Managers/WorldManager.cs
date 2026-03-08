using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public List<GameObject> Worlds;

    public Transform SpawnNewWorldPosition;
    public Transform SpawnedWorlds;

    public int HowManyYouMakeWorld = 0;

    public GameObject MakeNewWorld(int _x, int _y, int _z)
    {
        HowManyYouMakeWorld++;

        GameObject newWorld = new GameObject();
        newWorld.transform.parent = SpawnedWorlds;
        newWorld.transform.tag = "World";
        newWorld.layer = 7;
        newWorld.transform.position = SpawnNewWorldPosition.position;
        newWorld.AddComponent<MeshCollider>();
        newWorld.AddComponent<ChunkCode>().MakeNewWorld(_x, _y, _z);
        newWorld.GetComponent<ChunkCode>().SetName();
        Worlds.Add(newWorld);

        return newWorld;
    }

    public void DestroyWorld(GameObject _world)
    {
        if(!Worlds.Contains(_world)) Debug.Log("It Doesn;t Save");

        GameManager.Instance.SaveAndLoadManager.DeleteData(_world.GetComponent<ChunkCode>());
        GameManager.Instance.InputManager.UnChooseWorld();

        Worlds.Remove(_world);
        Destroy(_world);
    }

    public GameObject MakeWorld()
    {
        GameObject newWorld = new GameObject();
        newWorld.transform.parent = SpawnedWorlds;
        newWorld.transform.tag = "World";
        newWorld.layer = 6;
        newWorld.AddComponent<ChunkCode>();
        Worlds.Add(newWorld);

        return newWorld;
    }
}
