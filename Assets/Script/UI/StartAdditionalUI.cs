using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public enum StartUIs 
{
    MakeNewWorldUI,
    LoadUI,
    SettingUI
}


public class StartAdditionalUI : MonoBehaviour
{
    public GameObject LoadUI;
    public GameObject MakeNewWorldUI;
    public GameObject SettingUI;

    public GameObject NowUI = null;

    [Header("Move")]
    public RectTransform Desk;
    Vector2 DeskOriginalPos;
    Vector2 DeskAfterPos = new Vector2(0, - 150f);

    [Header("TapeInMotion")]
    public GameObject tape;
    GameObject _NowSpawnedtape = null;
    public RectTransform UpdownCover;
    public Transform Spawnpos;
    public Transform ChangeParent;
    public MoveName MoveName;
    public Image OverShow;
    string NowInTape = null;

    [Header("StartMotion")]
    public RectTransform MainPos;
    public RectTransform UpSide;
    public Image NoiseImage;
    public Image OutSideImage;

    private void Awake()
    {
        DeskOriginalPos = Desk.anchoredPosition;
    }

    public void SpawnUI(StartUIs selectUI)
    {
        if(NowUI != null) OffThisUI();

        GameObject _go = MakeNewWorldUI;
        switch (selectUI)
        {
            case StartUIs.MakeNewWorldUI:
                _go = MakeNewWorldUI;
                break;
            case StartUIs.LoadUI:
                _go = LoadUI;
                break;
            case StartUIs.SettingUI:
                _go = SettingUI;
                break;
        }

        GameObject go = Instantiate(_go, transform);
        NowUI = go;
        go.GetComponent<NeedToDoOnit>().GetParent(gameObject);
        go.GetComponent<NeedToDoOnit>().Onit();
    }

    public void OffThisUI()
    {
        if (NowUI == null) return;

        if(NowUI.TryGetComponent<OnEndAction>(out OnEndAction _code))
        {
            _code.OnEnd();
        }
        else 
        {
            Destroy(NowUI);
        }

        NowUI = null;
    }

    public void ClickLoadBtn()
    {
        SpawnUI(StartUIs.LoadUI);
    }

    public void ClickSettingBtn()
    {
        SpawnUI(StartUIs.SettingUI);
    }

    public void ClickStartBtn()
    {
        if(GameManager.Instance.NowWorldSaveFolder == null)
        {
            SpawnUI(StartUIs.LoadUI);
        }
        else
        {
            OffThisUI();
            StartMove();
        }
    }

    void StartMove()
    {
        UpSide.DOScale(new Vector2(2f, 2f), 2f);
        MainPos.DOAnchorPos(new Vector2(0, -1080), 0.5f).OnComplete(() =>
        {
            OutSideImage.DOColor(new Color(1, 1, 1, 0), 1.5f);
            NoiseImage.DOColor(new Color(0, 0, 0, 0), 2f).OnComplete(() =>
            {
                OnStartGame();
            });
        });
    }

    void OnStartGame()
    {
        GameManager.Instance.StartGame();

        UpSide.DOScale(new Vector2(1f, 1f), 0f);
        MainPos.DOAnchorPos(new Vector2(0, 0), 0f);
        NoiseImage.DOColor(new Color(1, 1, 1,1), 0f);
    }

    public void SpawnLoadUI()
    {
        GameManager.Instance.UIManager.StartSceneUI.AddText("서랍을 열어본다.");
        Desk.DOAnchorPos(DeskOriginalPos + DeskAfterPos, 0.7f);
    }

    public void DestroyLoadUI()
    {
        GameManager.Instance.UIManager.StartSceneUI.AddText("서랍을 닫는다.");
        Desk.DOAnchorPos(DeskOriginalPos, 0.5f);

        TapeInMotion();
    }

    void TapeInMotion()
    {
        if (NowInTape == GameManager.Instance.NowWorldSaveFolder) return;
        NowInTape = GameManager.Instance.NowWorldSaveFolder;

        OverShow.gameObject.SetActive(true);

        OverShow.DOKill();
        UpdownCover.DOKill();

        OverShow.DOColor(new Color(1, 1, 1, 1f), 0.5f);
        UpdownCover.DOSizeDelta(new Vector2(945, 100), 0.3f);

        if (NowInTape == null)
        {
            MoveName.Onit();
            return;
        }

        if(_NowSpawnedtape != null)
        {
            Destroy(_NowSpawnedtape);
        }
        _NowSpawnedtape = Instantiate(tape,Spawnpos);
        RectTransform _tapeRec = _NowSpawnedtape.GetComponent<RectTransform>();
        WorldSaveFile _wsf = _NowSpawnedtape.GetComponent<WorldSaveFile>();

        _wsf.Onit(NowInTape);
        _tapeRec.anchorMin = Vector3.one * 0.5f;
        _tapeRec.anchorMax = Vector3.one * 0.5f;
        _tapeRec.anchoredPosition = new Vector2(0, -80);
        _tapeRec.sizeDelta = new Vector2(1100, 200);
        _tapeRec.localScale = new Vector2(0.9f, 0.9f);
        _wsf.Shadow.gameObject.SetActive(true);

        _tapeRec.DOKill();
        _wsf.DOKill();

        UpdownCover.DOSizeDelta(new Vector2(945, 230), 0.5f).SetDelay(1.2f);
        OverShow.DOColor(new Color(1, 1, 1, 0), 4f).SetDelay(1.2f).OnComplete(() =>
        {
            OverShow.color = Color.black;
            OverShow.gameObject.SetActive(false);
        });
        _tapeRec.DOScale(new Vector2(0.85f, 0.6f), 0.8f).SetDelay(0.25f);
        _tapeRec.DOAnchorPos(_tapeRec.anchoredPosition + new Vector2(0, 10f), 0.5f).SetDelay(0.35f);
        _wsf.Shadow.DOColor(new Color(0,0,0,0.9f), 0.75f).SetDelay(0.2f).OnComplete(() =>
        {
            _NowSpawnedtape.transform.SetParent(ChangeParent);
            _tapeRec.DOAnchorPos(_tapeRec.anchoredPosition - new Vector2(0,40f) , 0.5f);
            MoveName.Onit();
        });
    }

    public void ClickExitGameBtn()
    {
        if (GameManager.Instance.UIManager.StartSceneUI.isStart)
        {
            GameManager.Instance.Exit();
        }
        else
        {
            GameManager.Instance.UIManager.StartSceneUI.ClickStart();
        }
    }
}
