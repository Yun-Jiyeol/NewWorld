using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSaveFile : MonoBehaviour
{
    public TapeSave tapeSave;
    string Name;

    public Image Shadow;

    public bool isCheck = false;
    public bool IsInitialized => tapeSave != null;

    public void Onit(string _name)
    {
        Name = _name;

        string saveName;
        string saveTime;
        int iconIndex;
        int tapeIndex;

        if (!TryReadSaveInfo(
            Name,
            false,
            out saveName,
            out saveTime,
            out iconIndex,
            out tapeIndex))
        {
            gameObject.SetActive(false);
            return;
        }

        GameObject _tape =
            Instantiate(
                GameManager.Instance.MaterialManager.Tapes[tapeIndex],
                gameObject.transform
            );

        tapeSave =
            _tape.GetComponent<TapeSave>();

        if (tapeSave == null)
        {
            Destroy(_tape);
            return;
        }

        SettingTapeText(
            saveName,
            saveTime,
            iconIndex
        );

        if (tapeSave.Check != null)
        {
            tapeSave.Check.GetComponent<Image>().sprite =
                tapeSave.WorldIcon.sprite;
        }

        UnCheck();

        if (GameManager.Instance.NowWorldSaveFolder == Name)
        {
            OnCheck();
        }
    }

    public int TapeInOnit(string _name)
    {
        Name = _name;

        string saveName;
        string saveTime;
        int iconIndex;
        int tapeIndex;

        if (!TryReadSaveInfo(
            Name,
            true,
            out saveName,
            out saveTime,
            out iconIndex,
            out tapeIndex))
        {
            gameObject.SetActive(false);
            return -1;
        }

        GameObject _tape =
            Instantiate(
                GameManager.Instance.MaterialManager.InsertTapes[tapeIndex],
                gameObject.transform
            );

        tapeSave =
            _tape.GetComponent<TapeSave>();

        if (tapeSave == null)
        {
            Destroy(_tape);
            return -1;
        }

        SettingTapeText(
            saveName,
            saveTime,
            iconIndex
        );

        return tapeIndex;
    }

    bool TryReadSaveInfo(
        string folderName,
        bool useInsertTape,
        out string saveName,
        out string saveTime,
        out int iconIndex,
        out int tapeIndex)
    {
        if (!SaveAndLoadManager.TryParseSaveFolderName(
            folderName,
            out saveName,
            out saveTime,
            out iconIndex,
            out tapeIndex))
        {
            Debug.LogWarning(
                "[SaveUI] 올바르지 않은 세이브 폴더 이름입니다: " +
                folderName
            );
            return false;
        }

        MaterialManager materialManager =
            GameManager.Instance.MaterialManager;

        if (materialManager == null)
            return false;

        List<GameObject> tapeList =
            useInsertTape
            ? materialManager.InsertTapes
            : materialManager.Tapes;

        if (tapeList == null)
            return false;

        int tapeCount = tapeList.Count;

        if (tapeIndex < 0 ||
            tapeIndex >= tapeCount)
        {
            Debug.LogWarning(
                "[SaveUI] Tape index가 범위를 벗어났습니다: " +
                tapeIndex
            );
            return false;
        }

        if (GameManager.Instance.UIManager == null ||
            GameManager.Instance.UIManager.WorldUIIcons == null ||
            GameManager.Instance.UIManager.WorldUIIcons
                .WorldUIIconsSetting == null ||
            iconIndex < 0 ||
            iconIndex >=
                GameManager.Instance.UIManager.WorldUIIcons
                    .WorldUIIconsSetting.Count)
        {
            Debug.LogWarning(
                "[SaveUI] World icon index가 범위를 벗어났습니다: " +
                iconIndex
            );
            return false;
        }

        return true;
    }

    void SettingTapeText(
        string saveName,
        string saveTime,
        int iconIndex)
    {
        if (tapeSave == null)
            return;

        tapeSave.WorldName.text = saveName;

        tapeSave.WorldMakeYear.text =
            saveTime.Substring(0, 4) + '.';

        tapeSave.WorldMakeMonth.text =
            saveTime.Substring(4, 2) + '.';

        tapeSave.WorldMakeDay.text =
            saveTime.Substring(6, 2);

        tapeSave.WorldNum.text =
            (char)('A' + iconIndex) +
            saveTime.Substring(12, 1) +
            saveTime.Substring(10, 1) +
            saveTime.Substring(8, 1);

        tapeSave.WorldIcon.sprite =
            GameManager.Instance.UIManager.WorldUIIcons
                .WorldUIIconsSetting[iconIndex];
    }

    public void ClickWorld()
    {
        if (tapeSave == null)
            return;

        if (isCheck)
        {
            UnCheck();
        }
        else
        {
            OnCheck();
        }

        if (GameManager.Instance.UIManager.StartAdditionalUI.NowUI != null)
        {
            LoadWorldUI loadWorldUI =
                GameManager.Instance.UIManager.StartAdditionalUI
                    .NowUI
                    .GetComponent<LoadWorldUI>();

            if (loadWorldUI != null)
                loadWorldUI.CheckAllWorlds(this);
        }
    }

    public void UnCheck()
    {
        isCheck = false;

        if (tapeSave != null &&
            tapeSave.Check != null)
        {
            tapeSave.Check.SetActive(false);
        }
    }

    public void OnCheck()
    {
        if (tapeSave == null)
            return;

        isCheck = true;

        if (tapeSave.Check != null)
            tapeSave.Check.SetActive(true);

        GameManager.Instance.SoundManager.PlayEffect(
            CassetteLiftClackSoundFactory
                .CreateCassetteLiftClack_05_ClearCassetteClack()
        );
    }

    public string GetName()
    {
        return Name;
    }
}
