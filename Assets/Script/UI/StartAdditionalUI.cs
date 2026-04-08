using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartAdditionalUI : MonoBehaviour
{
    public GameObject NowUI = null;

    public void SpawnUI(GameObject _go)
    {
        if(NowUI != null) Destroy(NowUI);

        GameObject go = Instantiate(_go,transform);
        NowUI = go;

        go.GetComponent<NeedToDoOnit>().Onit();
    }

    public void OffThisUI()
    {
        Destroy(NowUI);
        NowUI = null;
    }
}
