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
    public TextMeshProUGUI pos;

    Image thisImage;
    PlayerPosUI PPUI;

    private void Start()
    {
        thisImage = gameObject.GetComponent<Image>();
    }

    public void Setting(GameObject _world, PlayerPosUI _ppui)
    {
        PPUI = _ppui;
        ChunkCode = _world.GetComponent<ChunkCode>();
        Name.text = ChunkCode._name;
        pos.text = "x = " + _world.transform.position.x.ToString("F1") + "y = " + _world.transform.position.y.ToString("F1") + "z = " + _world.transform.position.z.ToString("F1");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        thisImage.color = new Color(0, 0, 0, 1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        thisImage.color = new Color(0, 0, 0, 0);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PPUI.TryToLookAt(this);
    }

    public void LooAtThis()
    {
        GameManager.Instance.MainCamera.LookAt(ChunkCode.gameObject.transform.position);
    }
}
