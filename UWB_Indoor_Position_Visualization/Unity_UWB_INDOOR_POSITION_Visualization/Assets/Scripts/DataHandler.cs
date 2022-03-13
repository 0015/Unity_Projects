/////////////////////////////////////////////////////////////////
/*
  ESP32 + UWB | Indoor Positioning + Unity Visualization
  For More Information: https://youtu.be/c8Pn7lS5Ppg
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
    [SerializeField] String leftAnchorShortName = "aabb";
    [SerializeField] String rightAnchorShortName = "ccdd";
    
    private List<float> leftAnchorAVG = new List<float>();
    private List<float> rightAnchorAVG = new List<float>();
    private double[] anchor_ranges = new double[2];

    public void setData(string rawData)
    {
        string[] data = rawData.Split(',');

        if (data.Length == 2)
        {
            float range = float.TryParse(data[1], out range) ? range : 0;

            if (data[0] == leftAnchorShortName)
            {
                leftAnchorAVG.Add(range);

                if (leftAnchorAVG.Count >= samplingDataSize)
                {
                    leftAnchorAVG.RemoveAt(0);
                    anchor_ranges[0] = leftAnchorAVG.Average();
                }
            }
            else
            {
                rightAnchorAVG.Add(range);

                if (rightAnchorAVG.Count >= samplingDataSize)
                {
                    rightAnchorAVG.RemoveAt(0);
                    anchor_ranges[1] = rightAnchorAVG.Average();
                }
            }

            if (anchor_ranges[0] != 0.00f && anchor_ranges[1] != 0.00f)
            {
                mRightText.text = "Right Anchor\n" + RoundUp((float)anchor_ranges[0], 1) + " m";
                mLeftText.text = "Left Anchor\n" + RoundUp((float)anchor_ranges[1], 1) + " m";
                calcTag((float)anchor_ranges[0], (float)anchor_ranges[1], distanceBetweenTwoAnchors);
            }
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