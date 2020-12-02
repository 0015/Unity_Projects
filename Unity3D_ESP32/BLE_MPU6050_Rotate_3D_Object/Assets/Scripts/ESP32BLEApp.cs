using System;
using TMPro;
using UnityEngine;

public class ESP32BLEApp : MonoBehaviour
{
    private string DeviceName = "ESP32 THAT PROJECT";
    private string ServiceUUID = "FFE0";
    private string Characteristic = "FFE1";


    enum States
    {
        None,
        Scan,
        Connect,
        Subscribe,
        Unsubscribe,
        Disconnect,
        Communication,
    }

    private bool _workingFoundDevice = true;
    private bool _connected = false;
    private float _timeout = 0f;
    private States _state = States.None;
    private bool _foundID = false;
    private string _deviceAddress;

    [SerializeField] private TMP_Text stateText;
    [SerializeField] private Transform cube;
    private Quaternion inverseQt;
    private Quaternion rawQt;


    void Reset()
    {
        _workingFoundDevice =
            false; // used to guard against trying to connect to a second device while still connecting to the first
        _connected = false;
        _timeout = 0f;
        _state = States.None;
        _foundID = false;
        _deviceAddress = null;
    }

    void SetState(States newState, float timeout)
    {
        _state = newState;
        _timeout = timeout;
    }

    void setStateText(string text)
    {
        if (stateText == null) return;
        stateText.text = text;
    }

    void StartProcess()
    {
        setStateText("Initializing...");

        Reset();
        BluetoothLEHardwareInterface.Initialize(true, false, () =>
        {
            SetState(States.Scan, 0.1f);
            setStateText("Initialized");
        }, (error) => { BluetoothLEHardwareInterface.Log("Error: " + error); });
    }

    // Use this for initialization
    void Start()
    {
        StartProcess();
        inverseQt = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        if (_timeout > 0f)
        {
            _timeout -= Time.deltaTime;
            if (_timeout <= 0f)
            {
                _timeout = 0f;

                switch (_state)
                {
                    case States.None:
                        break;

                    case States.Scan:
                        setStateText("Scanning for ESP32 devices...");

                        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) =>
                        {
                            // we only want to look at devices that have the name we are looking for
                            // this is the best way to filter out devices
                            if (name.Contains(DeviceName))
                            {
                                _workingFoundDevice = true;

                                // it is always a good idea to stop scanning while you connect to a device
                                // and get things set up
                                BluetoothLEHardwareInterface.StopScan();

                                // add it to the list and set to connect to it
                                _deviceAddress = address;
                                SetState(States.Connect, 0.5f);

                                _workingFoundDevice = false;
                            }
                        }, null, false, false);
                        break;

                    case States.Connect:
                        // set these flags
                        _foundID = false;

                        setStateText("Connecting to ESP32");

                        // note that the first parameter is the address, not the name. I have not fixed this because
                        // of backwards compatiblity.
                        // also note that I am note using the first 2 callbacks. If you are not looking for specific characteristics you can use one of
                        // the first 2, but keep in mind that the device will enumerate everything and so you will want to have a timeout
                        // large enough that it will be finished enumerating before you try to subscribe or do any other operations.
                        BluetoothLEHardwareInterface.ConnectToPeripheral(_deviceAddress, null, null,
                            (address, serviceUUID, characteristicUUID) =>
                            {
                                if (IsEqual(serviceUUID, ServiceUUID))
                                {
                                    // if we have found the characteristic that we are waiting for
                                    // set the state. make sure there is enough timeout that if the
                                    // device is still enumerating other characteristics it finishes
                                    // before we try to subscribe
                                    if (IsEqual(characteristicUUID, Characteristic))
                                    {
                                        _connected = true;
                                        SetState(States.Subscribe, 2f);

                                        setStateText("Connected to ESP32");
                                    }
                                }
                            }, (disconnectedAddress) =>
                            {
                                BluetoothLEHardwareInterface.Log("Device disconnected: " + disconnectedAddress);
                                setStateText("Disconnected");
                            });
                        break;

                    case States.Subscribe:
                        setStateText("Subscribing to ESP32");

                        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(_deviceAddress,
                            ServiceUUID,
                            Characteristic, null,
                            (address, characteristicUUID, bytes) =>
                            {
                                float qx = BitConverter.ToSingle(bytes, 0);
                                float qy = BitConverter.ToSingle(bytes, 4);
                                float qz = BitConverter.ToSingle(bytes, 8);
                                float qw = BitConverter.ToSingle(bytes, 12);

                                setStateText("byte length: " + bytes.Length + ", qx: " + qx.ToString("0.00000") +
                                             ", qy: " + qy.ToString("0.00000") + ", qz: " + qz.ToString("0.00000") +
                                             ", qw: " + qw.ToString("0.00000"));

                                //rawQt = new Quaternion(0, qx, 0, qw);
                                //rawQt = new Quaternion(-qz, 0, 0, qw);
                                //rawQt = new Quaternion(0, 0, qy, qw);
                                
                                rawQt = new Quaternion(-qz, qx, qy, qw);
                                cube.rotation = rawQt * inverseQt;
                            });

                        // set to the none state and the user can start sending and receiving data
                        _state = States.None;
                        break;

                    case States.Unsubscribe:
                        BluetoothLEHardwareInterface.UnSubscribeCharacteristic(_deviceAddress, ServiceUUID,
                            Characteristic,
                            null);
                        SetState(States.Disconnect, 4f);
                        break;

                    case States.Disconnect:
                        if (_connected)
                        {
                            BluetoothLEHardwareInterface.DisconnectPeripheral(_deviceAddress, (address) =>
                            {
                                BluetoothLEHardwareInterface.DeInitialize(() =>
                                {
                                    _connected = false;
                                    _state = States.None;
                                });
                            });
                        }
                        else
                        {
                            BluetoothLEHardwareInterface.DeInitialize(() => { _state = States.None; });
                        }

                        break;
                }
            }
        }
    }

    string FullUUID(string uuid)
    {
        return "0000" + uuid + "-0000-1000-8000-00805F9B34FB";
    }

    bool IsEqual(string uuid1, string uuid2)
    {
        if (uuid1.Length == 4)
            uuid1 = FullUUID(uuid1);
        if (uuid2.Length == 4)
            uuid2 = FullUUID(uuid2);

        return (uuid1.ToUpper().Equals(uuid2.ToUpper()));
    }

    public void resetRotation()
    {
        inverseQt = Quaternion.Inverse(rawQt);
    }
}