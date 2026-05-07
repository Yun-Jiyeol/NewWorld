using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomImage : MonoBehaviour
{
    public Image targetImage;
    public List<Sprite> sprites;

    public float changeInterval = 0.1f;

    Coroutine RIC = null;

    void OnEnable()
    {
        if(RIC != null)
        {
            StopCoroutine(RIC);
        }
        RIC = StartCoroutine(ChangeImage());
    }

    IEnumerator ChangeImage()
    {
        while (true)
        {
            if (sprites.Count <= 1)
            {
                yield return null;
                continue;
            }

            Sprite current = targetImage.sprite;
            Sprite next;

            do
            {
                next = sprites[Random.Range(0, sprites.Count)];
            }
            while (next == current);

            targetImage.sprite = next;

            yield return new WaitForSeconds(changeInterval);
        }
    }

    private void OnDisable()
    {
        StopCoroutine(RIC);
        RIC = null;
    }
}
