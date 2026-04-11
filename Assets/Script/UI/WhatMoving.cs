using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CoinType
{
    None,
    World,
    Player
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

    Vector2 PlayerVec = new Vector2(-36f, 0);
    Vector2 WorldVec = new Vector2(36f, 0);

    List<Tween> moveTween = new List<Tween>();
    List<Tween> colorTween = new List<Tween>();

    CoinType NowShowCoinType = CoinType.None;

    private void Awake()
    {
        CoinDict = new Dictionary<CoinType, CoinSave>();

        foreach (CoinSave coinSave in CoinSaves)
        {
            CoinDict.Add(coinSave.coinType, coinSave);
        }
    }

    public void ChangeMoveType(CoinType _nowcoin)
    {
        if(NowShowCoinType == _nowcoin) return;
        NowShowCoinType = _nowcoin;

        switch (_nowcoin)
        {
            case CoinType.None:
                MiddleCross.SetActive(false);

                StopColorTween();
                colorTween.Add(CoinDict[CoinType.World].CoinSprite.DOColor(CoinDict[_nowcoin].CoinColor, 0.5f));
                colorTween.Add(CoinDict[CoinType.Player].CoinSprite.DOColor(CoinDict[_nowcoin].CoinColor, 0.5f));
                break;

            case CoinType.Player:
                MiddleCross.SetActive(true);
                StopAllTween();
                CoinDict[CoinType.Player].CoinSprite.DOColor(CoinDict[_nowcoin].CoinColor, 0.5f);

                Color worldcolor = CoinDict[_nowcoin].CoinColor;
                worldcolor.a = 0f;
                CoinDict[CoinType.World].CoinSprite.DOColor(worldcolor, 0.5f);

                CoinDict[CoinType.World].CoinSprite.gameObject.GetComponent<RectTransform>().DOAnchorPos(PlayerVec, 0.5f);
                CoinDict[CoinType.Player].CoinSprite.gameObject.GetComponent<RectTransform>().DOAnchorPos(PlayerVec, 0.5f);
                break;

            case CoinType.World:
                MiddleCross.SetActive(false);
                StopAllTween();
                CoinDict[CoinType.World].CoinSprite.DOColor(CoinDict[_nowcoin].CoinColor, 0.5f);

                Color playercolor = CoinDict[_nowcoin].CoinColor;
                playercolor.a = 0f;
                CoinDict[CoinType.Player].CoinSprite.DOColor(playercolor, 0.5f);

                CoinDict[CoinType.World].CoinSprite.gameObject.GetComponent<RectTransform>().DOAnchorPos(WorldVec, 0.5f);
                CoinDict[CoinType.Player].CoinSprite.gameObject.GetComponent<RectTransform>().DOAnchorPos(WorldVec, 0.5f);
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
}
