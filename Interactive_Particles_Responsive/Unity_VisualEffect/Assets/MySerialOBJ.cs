/////////////////////////////////////////////////////////////////
/*
  Interactive Particles Responsive Made With ESP32 + INMP441 & Unity
  For More Information: https://youtu.be/lRj01J-cxew
  Created by Eric N. (ThatProject)
*/
/////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.VFX;

public class MySerialOBJ : MonoBehaviour
{
    private SerialPort serial;
    public int portSpeed = 115200;
    public string portName = "/dev/cu.SLAB_USBtoUART";

    void OnEnable()
    {
        if (checkOpen())
        {
            Debug.Log("Opening serial port: " + portName + " at " + portSpeed + " bauds");
        }
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (serial != null)
        {
            checkOpen();
            StartCoroutine(ReadSerial());
        }
    }

    private bool checkOpen()
    {
        if (serial == null)
        {
            if (serial != null && serial.IsOpen)
            {
                serial.Close();
            }

            serial = new SerialPort(portName, portSpeed);
        }

        if (!serial.IsOpen)
        {
            try
            {
                serial.Open();
                serial.DtrEnable = true;
                serial.DiscardInBuffer();
            }
            catch (Exception e)
            {
                Debug.LogError("System.Exception in serial.Open(): " + e);
            }
        }

        return serial.IsOpen;
    }

    public void OnApplicationQuit()
    {
        if (serial != null)
        {
            if (serial.IsOpen)
            {
                serial.Close();
            }

            serial = null;
        }
    }

    IEnumerator ReadSerial()
    {
        while (true)
        {
            if (!enabled)
            {
                yield break;
            }

            try
            {
                while (serial.BytesToRead > 0)
                {
                    string data = serial.ReadLine();
                    Debug.Log("Sound Pressure from INMP441: " + data);

                    float soundPressure;
                    if (float.TryParse(data, out soundPressure))
                    {
                        FindObjectOfType<VisualEffect>().GetComponent<EffectController>().setForceX(soundPressure);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exception in Serial ReadLine: " + e);
            }

            if (serial.IsOpen && serial.BytesToRead == -1)
            {
                serial.Close();
            }

            yield return null;
        }
    }
}