using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keys_KeySettingPage : NeedToDoOnit
{
    public List<NeedToDoOnit> keytypes;
    public bool isChange = false;

    public override void Onit()
    {
        foreach (var key in keytypes)
        {
            key.GetParent(gameObject);
            key.Onit();
        }
    }
}
