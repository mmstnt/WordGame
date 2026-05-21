using Ink.Parsed;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarChart : MaskableGraphic
{
    [Header("參數")]
    public float[] valueMax = new float[8];
    public float radiu;

    [Header("組件")]
    public Transform attributeImageGroup;

    private float[] value;

    public void setValue(int[] newValue,string[] newValueImage)
    {
        this.value = new float[newValue.Length];
        for(int i = 0; i < newValue.Length; i++) 
        {
            this.value[i] = newValue[i] / valueMax[i];
        }
        updateAttributeImage(attributeImageGroup, newValueImage);

        //重新繪製UI
        SetVerticesDirty();
    }

    private void updateAttributeImage(Transform UIGroup, string[] newValueImage)
    {
        for (int i = UIGroup.childCount - 1; i >= 0; i--)
        {
            Image attributeImage = UIGroup.GetChild(i).GetComponent<Image>();
            attributeImage.sprite = DataManager.instance.uiImageDataList.getData(newValueImage[i]);
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (value == null || value.Length < 3) return;

        UIVertex center = UIVertex.simpleVert;
        center.color = this.color;
        center.position = Vector3.zero;
        vh.AddVert(center);

        //設定邊數和角度
        int side = value.Length;
        float angle = 360f / side;

        float cosFactor = Mathf.Cos((angle / 2f) * Mathf.Deg2Rad);
        float compensationFactor = (cosFactor > 0.001f) ? (1f / cosFactor) : 1f;
        float adjustedMaxRadius = radiu * compensationFactor;

        // 2. 計算並添加各個屬性的頂點
        for (int i = 0; i < side; i++)
        {
            float currentAngle = (-90 + i * angle + (angle / 2f)) * Mathf.Deg2Rad;

            // 依據屬性數值比例計算實際半徑
            float currentRadiu = adjustedMaxRadius * Mathf.Clamp01(value[i]);

            float x = Mathf.Sin(currentAngle);
            float y = Mathf.Cos(currentAngle);

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = this.color;
            vertex.position = new Vector2(x * currentRadiu, y * currentRadiu);
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