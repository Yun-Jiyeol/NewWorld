using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningToOffGame : MonoBehaviour
{
    public void OKBtn()
    {
        GameManager.Instance.ExitGame();
    }

    public void NOBtn()
    {
        Destroy(gameObject);
    }
}
