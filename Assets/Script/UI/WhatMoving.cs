using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CoinType
{
    None,
    World,
    Player,
    Light
}

[System.Serializable]
public class CoinSave 
{
    public CoinType coinType;
    public Image CoinSprite;
    public Color CoinColor;
}


public class WhatMoving : MonoBehaviour
{
    public GameObject MiddleCross;

    public List<CoinSave> CoinSaves;
    Dictionary<CoinType, CoinSave> CoinDict;

    Vector2 PlayerVec = new Vector2(-25f, 0);
    Vector2 WorldVec = new Vector2(25f, 0);

    List<Tween> moveTween = new List<Tween>();
    List<Tween> colorTween = new List<Tween>();

    CoinType NowShowCoinType = CoinType.None;

    bool isLock = false;
    public LockUI lockUI;

    private void Awake()
    {
        CoinDict = new Dictionary<CoinType, CoinSave>();

        foreach (CoinSave coinSave in CoinSaves)
        {
            CoinDict.Add(coinSave.coinType, coinSave);
        }
    }

    public void LockMotion(bool _isLock)
    {
        if (isLock == _isLock) return;
        isLock = _isLock;

        if (isLock)
        {
            lockUI.OnLock();
        }
        else
        {
            lockUI.OffLock();
        }
    }

    public void ChangeMoveType(CoinType _nowcoin)
    {
        if (NowShowCoinType == _nowcoin) return;
        NowShowCoinType = _nowcoin;

        switch (_nowcoin)
        {
            case CoinType.None:
                MiddleCross.SetActive(false);
                GameManager.Instance.CursorMover.ChangeCursorState(cursorstate.Follow);

                StopColorTween();
                colorTween.Add(CoinDict[CoinType.World].CoinSprite.DOColor(CoinDict[_nowcoin].CoinColor, 0.5f));
                colorTween.Add(CoinDict[CoinType.Player].CoinSprite.DOColor(CoinDict[_nowcoin].CoinColor, 0.5f));
                colorTween.Add(CoinDict[CoinType.Light].CoinSprite.DOColor(CoinDict[_nowcoin].CoinColor, 0.5f));
                break;

            case CoinType.Player:
                MiddleCross.SetActive(true);
                GameManager.Instance.CursorMover.ChangeCursorState(cursorstate.Off);
                StopAllTween();
                CoinDict[CoinType.Player].CoinSprite.DOColor(CoinDict[_nowcoin].CoinColor, 0.5f);

                Color worldcolor = CoinDict[_nowcoin].CoinColor;
                worldcolor.a = 0f;
                CoinDict[CoinType.World].CoinSprite.DOColor(worldcolor, 0.5f);
                CoinDict[CoinType.Light].CoinSprite.DOColor(worldcolor, 0.5f);

                CoinDict[CoinType.World].CoinSprite.gameObject.GetComponent<RectTransform>().DOAnchorPos(PlayerVec, 0.5f);
                CoinDict[CoinType.Player].CoinSprite.gameObject.GetComponent<RectTransform>().DOAnchorPos(PlayerVec, 0.5f);
                CoinDict[CoinType.Light].CoinSprite.gameObject.GetComponent<RectTransform>().DOAnchorPos(PlayerVec, 0.5f);
                break;

            case CoinType.World:
                MiddleCross.SetActive(false);
                GameManager.Instance.CursorMover.ChangeCursorState(cursorstate.Follow);
                StopAllTween();
                CoinDict[CoinType.World].CoinSprite.DOColor(CoinDict[_nowcoin].CoinColor, 0.5f);

                Color playercolor = CoinDict[_nowcoin].CoinColor;
                playercolor.a = 0f;
                CoinDict[CoinType.Player].CoinSprite.DOColor(playercolor, 0.5f);
                CoinDict[CoinType.Light].CoinSprite.DOColor(playercolor, 0.5f);

                CoinDict[CoinType.World].CoinSprite.gameObject.GetComponent<RectTransform>().DOAnchorPos(WorldVec, 0.5f);
                CoinDict[CoinType.Player].CoinSprite.gameObject.GetComponent<RectTransform>().DOAnchorPos(WorldVec, 0.5f);
                CoinDict[CoinType.Light].CoinSprite.gameObject.GetComponent<RectTransform>().DOAnchorPos(WorldVec, 0.5f);
                break;

            case CoinType.Light:
                MiddleCross.SetActive(false);
                GameManager.Instance.CursorMover.ChangeCursorState(cursorstate.Follow);
                StopAllTween();
                CoinDict[CoinType.Light].CoinSprite.DOColor(CoinDict[_nowcoin].CoinColor, 0.5f);

                Color lightcolor = CoinDict[_nowcoin].CoinColor;
                lightcolor.a = 0f;
                CoinDict[CoinType.Player].CoinSprite.DOColor(lightcolor, 0.5f);
                CoinDict[CoinType.World].CoinSprite.DOColor(lightcolor, 0.5f);

                CoinDict[CoinType.World].CoinSprite.gameObject.GetComponent<RectTransform>().DOAnchorPos(WorldVec, 0.5f);
                CoinDict[CoinType.Player].CoinSprite.gameObject.GetComponent<RectTransform>().DOAnchorPos(WorldVec, 0.5f);
                CoinDict[CoinType.Light].CoinSprite.gameObject.GetComponent<RectTransform>().DOAnchorPos(WorldVec, 0.5f);
                break;
        }
    }

    void StopAllTween()
    {
        StopColorTween();
        StopMoveTween();
    }

    void StopColorTween()
    {
        if (colorTween.Count == 0) return;

        foreach (Tween tw in colorTween)
        {
            tw.Kill();
        }

        colorTween.Clear();
    }

    void StopMoveTween()
    {
        if (moveTween.Count == 0) return;

        foreach (Tween tw in moveTween)
        {
            tw.Kill();
        }

        moveTween.Clear();
    }

    [Header("Turning UI")]
    public GameObject Turnning;
    public Image Sphere;
    public Image XRing;
    public Image YRing;
    public Image ZRing;

    Color NoneSeen = new Color(1, 1, 1, 0);

    public void OnAndOffTurnning(bool active)
    {
        if(Turnning.activeSelf == active) return;

        if (active)
        {
            Turnning.SetActive(true);
            DoKillAll();

            Sphere.color = NoneSeen;
            XRing.color = NoneSeen;
            YRing.color = NoneSeen;
            ZRing.color = NoneSeen;

            Sphere.DOColor(Color.white, 0.5f);
        }
        else
        {
            Sphere.DOColor(NoneSeen, 0.5f).OnComplete(() =>
            {
                Turnning.SetActive(false);
            });
        }
    }

    void DoKillAll()
    {
        Sphere.DOKill();
        XRing.DOKill();
        YRing.DOKill();
        ZRing.DOKill();
    }
}
