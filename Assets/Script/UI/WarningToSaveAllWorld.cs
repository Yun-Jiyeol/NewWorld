using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningToSaveAllWorld : MonoBehaviour
{
    public void ClickOkBtn()
    {
        GameManager.Instance.SaveAndLoadManager.SaveAllWorld();
        Destroy(gameObject);
    }

    public void ClickNoBtn()
    {
        Destroy(gameObject);
    }
}
