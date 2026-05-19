using UnityEngine;
using UnityEngine.UI;

public class RadarChart : MaskableGraphic
{
    public float[] values;
    public float radius;

    public void SetValues(float[] newValues)
    {
        this.values = newValues;
        SetVerticesDirty(); // 驅動 UGUI 重新繪製
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (values == null || values.Length < 3) return;

        //設定邊數和角度
        int side = values.Length;
        float angle = 360f / side;

        float cosFactor = Mathf.Cos((angle / 2f) * Mathf.Deg2Rad);
        float compensationFactor = (cosFactor > 0.001f) ? (1f / cosFactor) : 1f;
        float adjustedMaxRadius = radius * compensationFactor;
        // 1. 添加中心點 (Index: 0)
        UIVertex center = UIVertex.simpleVert;
        center.color = this.color;
        center.position = Vector3.zero;
        vh.AddVert(center);

        // 2. 計算並添加各個屬性的頂點
        for (int i = 0; i < side; i++)
        {
            float currentAngle = (i * angle + (angle / 2f)) * Mathf.Deg2Rad;

            // 依據屬性數值比例計算實際半徑
            float currentRadiu = adjustedMaxRadius * Mathf.Clamp01(values[i]);

            float x = Mathf.Sin(currentAngle) * currentRadiu;
            float y = Mathf.Cos(currentAngle) * currentRadiu;

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = this.color;
            vertex.position = new Vector2(x, y);
            vh.AddVert(vertex);
        }

        // 3. 根據頂點順序連接成三角形面
        for (int i = 1; i <= side; i++)
        {
            int next = (i == side) ? 1 : i + 1;
            vh.AddTriangle(0, i, next); // 中心點、當前點、下一個點
        }
    }
}