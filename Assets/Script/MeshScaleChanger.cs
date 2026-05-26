using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class MeshScaleChanger : BaseMeshEffect
{
    [Range(0.5f, 1.5f)]
    public float topScale = 1.0f;

    [Range(0.5f, 1.5f)]
    public float bottomScale = 1.0f;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        List<UIVertex> verts = new List<UIVertex>();
        vh.GetUIVertexStream(verts);

        Rect rect = graphic.rectTransform.rect;
        float yMin = rect.yMin;
        float yMax = rect.yMax;
        float centerX = rect.center.x;

        for (int i = 0; i < verts.Count; i++)
        {
            UIVertex v = verts[i];

            float y01 = Mathf.InverseLerp(yMin, yMax, v.position.y);

            // 아래쪽은 bottomScale, 위쪽은 topScale
            float scale = Mathf.Lerp(bottomScale, topScale, y01);

            Vector3 pos = v.position;
            pos.x = centerX + (pos.x - centerX) * scale;

            v.position = pos;
            verts[i] = v;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
    }

    public void SetWarp(float top, float bottom)
    {
        topScale = top;
        bottomScale = bottom;
        graphic.SetVerticesDirty();
    }

    public void TapeInMotion()
    {
        DOTween.To(
            () => bottomScale,
            x =>
            {
                bottomScale = x;
                graphic.SetVerticesDirty();
            },
            1.02f,
            0.35f
        );
    }
}
