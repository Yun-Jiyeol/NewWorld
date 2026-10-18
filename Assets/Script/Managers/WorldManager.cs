using System;
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

    public void DestroyAllWorld()
    {
        for(int i = Worlds.Count -1; i >= 0; i--)
        {
            Destroy(Worlds[i]);
        }

        Worlds.Clear();
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

    public void LoadAllWorld()
    {
        StartCoroutine(
            LoadAllWorldCoroutine()
        );
    }


    public IEnumerator LoadAllWorldCoroutine(
        Func<bool> shouldCancel = null
    )
    {
        GameObject maincam =
            Camera.main.gameObject;


        SortWorldsByDistance(maincam);


        GameManager.Instance.UIManager.StartSceneUI
            .AddText("TapeRead");


        yield return StartCoroutine(
            LoadAllworld(shouldCancel)
        );
    }

    void SortWorldsByDistance(GameObject go)
    {
        Worlds.Sort((a, b) =>
        {
            float distA = Vector3.Distance(go.transform.position, a.transform.position);
            float distB = Vector3.Distance(go.transform.position, b.transform.position);
            return distA.CompareTo(distB);
        });
    }

    IEnumerator LoadAllworld(
    Func<bool> shouldCancel = null
)
    {
        GameManager.Instance.StartAdditionalUI
            .ReconstructionValue(0);


        for (int i = 0; i < Worlds.Count; i++)
        {
            // =========================
            // 취소 확인
            // =========================

            if (
                shouldCancel != null &&
                shouldCancel()
            )
            {
                yield break;
            }


            ChunkCode chunk =
                Worlds[i].GetComponent<ChunkCode>();


            GameManager.Instance.UIManager.StartSceneUI
                .AddText("WorldCreate");


            // =========================
            // 해당 World 완성까지 기다림
            // =========================

            // 로드에서는 BuildRoutine 끝의 FinishSettingBlock()을 실행하지 않음.
            // 즉, 동기식 PutThisWorld()이 끼어들지 않게 한다.
            yield return StartCoroutine(
                chunk.BuildRoutine(
                    false,
                    shouldCancel
                )
            );


            // BuildRoutine 도중 취소 요청이 들어왔는지 확인
            if (
                shouldCancel != null &&
                shouldCancel()
            )
            {
                yield break;
            }


            // 로드 완료 시 통합 작업도 프레임 분할 버전으로 실행
            yield return StartCoroutine(
                chunk.PutThisWorldCoroutine()
            );


            chunk.LoadState =
                WorldLoadState.Complete;


            GameManager.Instance.StartAdditionalUI
                .ReconstructionValue(
                    (float)(i + 1) / Worlds.Count
                );


            yield return null;
        }


        GameManager.Instance.UIManager.StartAdditionalUI
            .ChangeCreationState(CreateState.Done);


        GameManager.Instance.UIManager.StartSceneUI
            .AddText("Eject");
    }
}   
