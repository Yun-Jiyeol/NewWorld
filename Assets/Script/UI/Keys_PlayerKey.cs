using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Keys_PlayerKey : NeedToDoOnit
{
    public List<NeedToDoOnit> keys;
    GameObject parent;

    public override void GetParent(GameObject _parent)
    {
        parent = _parent;
    }

    public override void Onit()
    {
        foreach(var key in keys)
        {
            key.GetParent(parent);
            key.Onit();
        }
    }
}
