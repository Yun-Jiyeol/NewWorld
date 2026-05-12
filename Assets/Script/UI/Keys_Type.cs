using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keys_Type : NeedToDoOnit
{
    public NeedToDoOnit KeyOnit;
    GameObject parent;

    public override void Onit()
    {
        KeyOnit.GetParent(parent);
        KeyOnit.Onit();
    }

    public override void GetParent(GameObject _parent)
    {
        parent = _parent;
    }
}
