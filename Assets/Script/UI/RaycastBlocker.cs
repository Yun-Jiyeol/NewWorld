using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class RaycastBlocker : Graphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); // 아무것도 안 그림
    }
}
