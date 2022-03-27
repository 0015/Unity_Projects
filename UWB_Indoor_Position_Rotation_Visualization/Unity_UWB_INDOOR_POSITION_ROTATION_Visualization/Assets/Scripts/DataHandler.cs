/////////////////////////////////////////////////////////////////
/*
  ESP32 + UWB + IMU | Indoor Position & Rotation + Unity Visualization
  For More Information: https://youtu.be/fPuxcjHsfpc
  Created by Eric N. (ThatProject)
*/
/////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DataHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mLeftText;
    [SerializeField] TextMeshProUGUI mRightText;

    [SerializeField] float distanceBetweenTwoAnchors;
    [SerializeField] int samplingDataSize = 20;
    [SerializeField] float maxDistance = 4;
    private List<float> leftAnchorAVG = new List<float>();
    private List<float> rightAnchorAVG = new List<float>();
    private double[] anchor_ranges = new double[2];

    public void setData(string rawData)
    {
        string[] data = rawData.Split(',');


        if (data.Length == 2) //UWB Anchors
        {
            float leftRange = float.TryParse(data[0], out leftRange) ? leftRange : 0;
            float rightRange = float.TryParse(data[1], out rightRange) ? rightRange : 0;
            
            if (Mathf.Abs(rightRange) < maxDistance)
            {
                rightAnchorAVG.Add(rightRange);

                if (rightAnchorAVG.Count >= samplingDataSize)
                {
                    rightAnchorAVG.RemoveAt(0);
                    anchor_ranges[1] = rightAnchorAVG.Average();
                }
            }

            if (Mathf.Abs(leftRange) < maxDistance)
            {
                leftAnchorAVG.Add(leftRange);

                if (leftAnchorAVG.Count >= samplingDataSize)
                {
                    leftAnchorAVG.RemoveAt(0);
                    anchor_ranges[0] = leftAnchorAVG.Average();
                }
            }


            if (anchor_ranges[0] != 0.00f && anchor_ranges[1] != 0.00f)
            {
                mRightText.text = "Right Anchor\n" + RoundUp((float)anchor_ranges[0], 1) + " m";
                mLeftText.text = "Left Anchor\n" + RoundUp((float)anchor_ranges[1], 1) + " m";
                calcTag((float)anchor_ranges[0], (float)anchor_ranges[1], distanceBetweenTwoAnchors);
            }
        }

        if (data.Length == 4) //BNO055 Quaternion
        {
            float qw = float.TryParse(data[0], out qw) ? qw : 0;
            float qx = float.TryParse(data[1], out qx) ? qx : 0;
            float qy = float.TryParse(data[2], out qy) ? qy : 0;
            float qz = float.TryParse(data[3], out qz) ? qz : 0;
            FindObjectOfType<Player>().rotateCap(new Quaternion(0, qz, 0, qw));
        }
    }

    //Using the algorithm from Makerfabs
    //https://www.makerfabs.cc/article/esp32-uwb-indoor-positioning-test.html
    private void calcTag(float a, float b, float c)
    {
        float cos_a = (b * b + c * c - a * a) / (2 * b * c);
        float x = b * cos_a;
        float y = b * Mathf.Sqrt(1 - cos_a * cos_a);
        FindObjectOfType<Player>().movePlayer(RoundUp(x, 2), RoundUp(y, 2));
    }

    static double RoundUp(float input, int places)
    {
        double multiplier = Math.Pow(10, Convert.ToDouble(places));
        return Math.Ceiling(input * multiplier) / multiplier;
    }
}