using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldExUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    ChunkCode ChunkCode;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI xpos;
    public TextMeshProUGUI ypos;
    public TextMeshProUGUI zpos;

    public GameObject OnSelect;
    public GameObject OnMouse;
    public GameObject OnError;

    PlayerPosUI PPUI;

    public void Setting(GameObject _world, PlayerPosUI _ppui)
    {
        PPUI = _ppui;
        ChunkCode = _world.GetComponent<ChunkCode>();
        if(!ChunkCode.isSavedBefore) OnError.SetActive(true);

        Name.text = ChunkCode._name;

        xpos.text = "XÁÂÇ¥ = " + _world.transform.position.x.ToString("F1");
        ypos.text = "YÁÂÇ¥ = " + _world.transform.position.y.ToString("F1");
        zpos.text = "ZÁÂÇ¥ = " + _world.transform.position.z.ToString("F1");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnMouse.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnMouse.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PPUI.TryToLookAt(this);
    }

    public void LooAtThis()
    {
        OnSelect.SetActive(true);
        GameManager.Instance.MainCamera.LookAt(ChunkCode.gameObject.transform);
    }

    public ChunkCode getChunkCode()
    {
        return ChunkCode;
    }
}
