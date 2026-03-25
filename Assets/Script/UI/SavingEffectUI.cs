using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavingEffectUI : MonoBehaviour
{
    public GameObject SavingEffect;
    GameObject OnEffectGo;

    public void StartSaving()
    {
        if (OnEffectGo != null) return;

        OnEffectGo = Instantiate(SavingEffect, gameObject.transform);
    }

    public void EndSaving()
    {
        if (OnEffectGo == null) return;

        Destroy(OnEffectGo);
        OnEffectGo = null;
    }
}
