using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum StartUIs 
{
    MakeNewWorldUI,
    LoadUI,
    SettingUI
}

public enum CreateState 
{
    Break,
    Setting,
    Making,
    Done,
    Looking
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
    Vector2 DeskAfterPos = new Vector2(0, 145f);

    [Header("TapeInMotion")]
    public GameObject tape;
    GameObject _NowSpawnedtape = null;
    public RectTransform UpdownCover;
    public Transform Spawnpos;
    public Transform ChangeParent;
    public StartPage_NamePage NamePage;
    public Image OverShow;
    public Image AdditionalShow;
    public Image NoSignalText;
    public Image NoSignalSide;
    string NowInTape = null;
    public Transform FirstSpawnTrans;

    [Header("StartMotion")]
    public RectTransform MainPos;
    public RectTransform UpSide;
    public Image NoiseImage;
    public Image OutSideImage;
    bool CanEject = true;

    [Header("StartMotionSound")]
    public SoundData Insert1;
    public SoundData Insert2;
    public SoundData Eject;


    [Header("WorldStatus")]
    public TextMeshProUGUI CreationStatus;
    public List<Image> Reconstruction;
    float lastvalue = 0;

    public List<Image> SignalQuality;
    float timer;
    int frameCount;
    float fps;
    int fpscount = 0;

    public TextMeshProUGUI CurrentBGM;
    public TextMeshProUGUI SystemError;
    public Sprite OnSprite;
    public Sprite OffSprite;

    private void Awake()
    {
        DeskOriginalPos = Desk.sizeDelta;
    }

    private void Start()
    {
        GameManager.Instance.StartAdditionalUI = this;
    }

    private void Update()
    {
        SignalQualityValue();
    }

    void SignalQualityValue()
    {
        timer += Time.unscaledDeltaTime;
        frameCount++;

        if (timer >= 1f)
        {
            fps = frameCount / timer;
            frameCount = 0;
            timer = 0f;

            int nowcount;
            if (fps >= 144) nowcount = 5;
            else if (fps >= 120) nowcount = 4;
            else if (fps >= 90) nowcount = 3;
            else if (fps >= 60) nowcount = 2;
            else if (fps >= 30) nowcount = 1;
            else nowcount = 0;

            if (fpscount == nowcount) return;

            fpscount = nowcount;

            for (int i = 0; i < 5; i++)
            {
                SignalQuality[i].sprite = i < fpscount ? OnSprite : OffSprite;
            }
        }
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

    public void ClickEjectBtn()
    {
        if (!CanEject) return;
        if (NowInTape == null) return;
        if (GameManager.Instance.NowWorldSaveFolder == null) return;

        NowInTape = null;
        GameManager.Instance.NowWorldSaveFolder = null;

        AdditionalShow.DOKill();
        NoSignalText.DOKill();
        UpdownCover.DOKill();
        OverShow.DOKill();

        AdditionalShow.gameObject.SetActive(true);
        AdditionalShow.DOColor(new Color(1, 1, 1, 1), 0f).OnComplete(() =>
        {
            NoSignalText.DOColor(new Color(1, 1, 1, 1), 0f);
        });
        AdditionalShow.DOColor(new Color(1, 1, 1, 0), 0f).SetDelay(0.2f).OnComplete(() =>
        {
            AdditionalShow.gameObject.SetActive(false);
        });

        GameManager.Instance.SoundManager.PlayEffect(Eject);

        if (NowInTape == null)
        {
            NamePage.Onit();
        }

        if (_NowSpawnedtape != null)
        {
            Destroy(_NowSpawnedtape);
        }

        GameManager.Instance.SaveAndLoadManager.CancelLoadWorldFolder();
        GameManager.Instance.WorldManager.DestroyAllWorld();
        ReconstructionValue(0);

        ResetTape();
    }

    void StartMove()
    {
        GameManager.Instance.CursorMover.ChangeCursorState(cursorstate.Off);

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
        OutSideImage.DOColor(new Color(1, 1, 1, 1), 0f);
    }

    public void SpawnLoadUI()
    {
        GameManager.Instance.UIManager.StartSceneUI.AddText("OpenDrawer");
        Desk.DOSizeDelta(DeskOriginalPos + DeskAfterPos, 0.7f);
    }

    public void DestroyLoadUI()
    {
        GameManager.Instance.UIManager.StartSceneUI.AddText("CloseDrawer");
        Desk.DOSizeDelta(DeskOriginalPos, 0.5f);

        TapeInMotion();
    }

    void TapeInMotion()
    {
        if (NowInTape == GameManager.Instance.NowWorldSaveFolder) return;
        NowInTape = GameManager.Instance.NowWorldSaveFolder;

        ResetTape();

        if (NowInTape == null)
        {
            NamePage.Onit();
            return;
        }

        if(_NowSpawnedtape != null)
        {
            Destroy(_NowSpawnedtape);
        }
        CanEject = false;

        _NowSpawnedtape = Instantiate(tape, FirstSpawnTrans);
        RectTransform _tapeRec = _NowSpawnedtape.GetComponent<RectTransform>();
        WorldSaveFile _wsf = _NowSpawnedtape.GetComponent<WorldSaveFile>();

        _wsf.TapeInOnit(NowInTape);                                     //테이프 소환
        _wsf.Shadow.gameObject.SetActive(true);
        _wsf.Shadow.GetComponent<BtnClick>().enabled = false;

        TapeSave _tp = _wsf.tapeSave;
        _wsf.Shadow.transform.SetParent(_tp.gameObject.transform);

        RectTransform rt = _wsf.Shadow.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;

        _tapeRec.anchorMin = Vector3.one * 0.5f;
        _tapeRec.anchorMax = Vector3.one * 0.5f;
        _tapeRec.anchoredPosition = new Vector2(295, -670);
        _tapeRec.sizeDelta = new Vector2(100, 100);
        _tapeRec.localScale = new Vector2(1f, 1f);

        _tapeRec.DOKill();
        _wsf.DOKill();
        AdditionalShow.DOKill();
        NoSignalText.DOKill();

        AdditionalShow.gameObject.SetActive(true);                      //순간 노이즈 생성
        AdditionalShow.DOColor(new Color(1, 1, 1, 1), 0f).SetDelay(0.1f).OnComplete(() =>
        {
            NoSignalText.DOColor(new Color(1, 1, 1, 0), 0f);            //순간 노이즈 치우기
        });
        AdditionalShow.DOColor(new Color(1, 1, 1, 0), 0f).SetDelay(0.4f).OnComplete(() =>
        {
            AdditionalShow.gameObject.SetActive(false);
        });

        OverShow.DOColor(new Color(1, 1, 1, 0), 4f).SetDelay(1.2f).OnComplete(() =>     //노이즈 치우기
        {
            OverShow.color = Color.black;
            OverShow.gameObject.SetActive(false);
        });

        MeshScaleChanger MSC = _tp.Check.GetComponent<MeshScaleChanger>();
        MSC.Invoke(nameof(MeshScaleChanger.TapeInMotion), 0.15f);
        _tp.DownSide.GetComponent<RectTransform>().DOSizeDelta(new Vector2(920f, 50f), 0.35f).SetDelay(0.15f);
        _tp.gameObject.GetComponent<RectTransform>().DOSizeDelta(new Vector2(920f, 170f), 0.4f).SetDelay(0.1f);
        _tapeRec.DOAnchorPos(new Vector2(295, -355), 0.5f);        //테이프 올리기

        _tapeRec.DOScale(new Vector2(0.95f, 0.6f), 0.6f).SetDelay(0.5f).OnStart(() =>
        {
            GameManager.Instance.SoundManager.PlayEffect(Insert1);
        });                //테이프 넣기
        _tp.gameObject.GetComponent<RectTransform>().DOSizeDelta(new Vector2(920f, 120f), 0.6f).SetDelay(0.5f);
        _tapeRec.DOAnchorPos(new Vector2(295, -300), 0.6f).SetDelay(0.5f);

        UpdownCover.DOAnchorPosY(-47.5f, 0.3f).SetDelay(1.5f).OnStart(() =>
        {
            GameManager.Instance.SoundManager.PlayEffect(Insert2);
        }).OnComplete(() =>                 //커버 내리기
        {
            CanEject = true;
        });

        _wsf.Shadow.DOColor(new Color(0,0,0,0.9f), 0.75f).SetDelay(0.6f).OnComplete(() =>
        {
            _NowSpawnedtape.transform.SetParent(ChangeParent);
            _tapeRec.DOAnchorPos(_tapeRec.anchoredPosition - new Vector2(0,60f) , 0.5f);
            NamePage.Onit();
        });
    }

    void ResetTape()
    {
        OverShow.gameObject.SetActive(true);

        OverShow.DOKill();
        UpdownCover.DOKill();

        OverShow.DOColor(new Color(1, 1, 1, 1f), 0.5f);
        UpdownCover.DOAnchorPosY(-115f, 0.3f);

        ChangeCreationState(CreateState.Break);
    }

    public void ReconstructionValue(float percentage)
    {
        int nowcount;
        if (percentage == 1) nowcount = 5;
        else if (percentage >= 0.8f) nowcount = 4;
        else if (percentage >= 0.6f) nowcount = 3;
        else if (percentage >= 0.4f) nowcount = 2;
        else if (percentage >= 0.2f) nowcount = 1;
        else nowcount = 0;

        if (lastvalue == nowcount) return;
        lastvalue = nowcount;

        for (int i = 0; i < 5; i++)
        {
            Reconstruction[i].sprite = i < lastvalue ? OnSprite : OffSprite;
        }
    }

    public void ChangeCreationState(CreateState _changingState)
    {
        CreationStatus.text = _changingState.ToString();
    }

    public void ClickExitGameBtn()
    {
        GameManager.Instance.Exit();
    }
}
