using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AGGuiGraphLine : MonoBehaviour
{
    //Class used mainly to update data point positions for now
    public class AGGraphDataPoint
    {
        public string DataName;
        public GameObject DataGameObject;
        public float DataValue;
    }

    public GameObject m_chartContentObj;
    public Vector2 m_dataPointObjSize; //We could get this automatically, but not much win
    //string m_dataPointPrefabName = "GuiGraphDataPointPrefab";
    float m_chartHeight;
    float m_lastKnownChartHDelta;
    List<AGGraphDataPoint> m_dataPoints = new List<AGGraphDataPoint>();
    public GameObject m_dataPrefab;

    void Start()
    {
        //m_chartContentObj = VHUtils.FindChildRecursive(this.gameObject, "Content");
        //Debug.Log(m_chartContentObj.GetComponent<RectTransform>().sizeDelta.y);
        //Debug.Log(m_chartContentObj.GetComponent<RectTransform>().sizeDelta.y);
        m_lastKnownChartHDelta = m_chartContentObj.GetComponent<RectTransform>().sizeDelta.y;

        Clear();
        //AddDataPoint(0.8f);
        //AddDataPoint(0.6f);
        //AddDataPoint(0.9f);
        //AddDataPoint(0.2f);
    }

    void Update()
    {
        if (m_lastKnownChartHDelta != m_chartContentObj.GetComponent<RectTransform>().sizeDelta.y)
        {
            //Debug.Log("Chart height change detected");
            SetChartHeight(true);
        }
    }

    public GameObject AddDataPoint(float m_scoreNormalized)
    {
        GameObject newDataPoint = Instantiate(m_dataPrefab);
        newDataPoint.transform.SetParent(m_chartContentObj.transform, false);
        newDataPoint.SetActive(true);

        RectTransform newDataPointChildRect = null;
        foreach (Transform xform in newDataPoint.GetComponentsInChildren<Transform>())
        {
            if (xform.name == "Image")
            {
                newDataPointChildRect = xform.gameObject.GetComponent<RectTransform>();
                break;
            }
        }

        float dataPointPosition = m_scoreNormalized * m_chartHeight;
        //Debug.LogWarning("Setting posY to: " + dataPointPosition.ToString());
        newDataPointChildRect.localPosition = new Vector3(newDataPointChildRect.localPosition.x, dataPointPosition, newDataPointChildRect.localPosition.z);
        m_dataPoints.Add(new AGGraphDataPoint { DataName = "Data", DataGameObject = newDataPoint, DataValue = m_scoreNormalized });

        //TODO - Add call backs to each dataPoint for interactivity?
        return newDataPoint;
    }
    
    public void Clear()
    {
        VHUtils.DeleteChildren(m_chartContentObj.transform);
        m_dataPoints.Clear();
    }

    void SetChartHeight(bool updateDataPos)
    {
        //Update chart HDelta so we can keep track of if the chart size changed, necessitating an update
        m_lastKnownChartHDelta = m_chartContentObj.GetComponent<RectTransform>().sizeDelta.y;

        float chartHeight = Mathf.Abs(m_chartContentObj.GetComponent<RectTransform>().sizeDelta.y);
        m_chartHeight = chartHeight - m_dataPointObjSize.y;

        if (updateDataPos)
        {
            UpdateDataPoints();
        }
    }

    /// <summary>
    /// This currently mainly updates data point positions on the graph based on the chart's area
    /// </summary>
    void UpdateDataPoints()
    {
        foreach (AGGraphDataPoint dataPoint in m_dataPoints)
        {
            foreach (Transform xform in dataPoint.DataGameObject.GetComponentsInChildren<Transform>())
            {
                if (xform.name == "Image")
                {
                    RectTransform dataPointChildRect = null;
                    dataPointChildRect = xform.gameObject.GetComponent<RectTransform>();
                    float dataPointPosition = dataPoint.DataValue * m_chartHeight;
                    dataPointChildRect.localPosition = new Vector3(dataPointChildRect.localPosition.x, dataPointPosition, dataPointChildRect.localPosition.z);

                    break;
                }
            }
        }
    }
}
