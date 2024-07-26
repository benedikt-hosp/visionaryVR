using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ViveSR.anipal.Eye;
//using ViveSR.anipal.Eye;



[System.Serializable]
public class ETController: MonoBehaviour
{


    [Header("Choose ET-SDK")]     // do not need to set it dynamically
    public Providers ceyeTrackingProvider;
    public string dataFolder = "D:\\SFB_Subjects\\Userfolder"; // is set by StateMainMenu


    // EyeTracking Controller
    public bool calibrated = false;
    public EyeTrackingProviderController etpc;
    public GazeTracker eventTracker;
    private Providers eyeTrackingProvider;
    Camera[] cams;
    private string userFolder;


    public EyeTrackingProviderController getSetEyetrackingProvider { get { return this.etpc; } }

    public ETController(Providers eyeTrackingProvider)
    {
        
        this.eyeTrackingProvider = eyeTrackingProvider;
        this.loadETGameObjects();
        this.etpc = new EyeTrackingProviderController(this.eyeTrackingProvider);
        

    }

    public void InitController()
    {
        Debug.LogError("InitController: Start");
        this.eyeTrackingProvider = ceyeTrackingProvider;
        Debug.LogError("InitController: Set Provider");
        this.loadETGameObjects();
        Debug.LogError("InitController: Loaded Objects");
        this.etpc = new EyeTrackingProviderController(this.eyeTrackingProvider);
        Debug.LogError("InitController: Created provider controller");
        StartCoroutine(this.etpc.eyeTrackingProviderInterface.ProcessEyeDataCoroutine());
        Debug.LogError("InitController: started coroutine");

    }




    public bool isCalibrated()
    {
        return this.calibrated;
    }

    private void loadETGameObjects()
    {
        GameObject _gameobject;
        switch (this.eyeTrackingProvider)
        {


            case Providers.TobiiPro:
                
                //_gameobject = null;
                // BHO WORKAROUND TO NOT REIMPLEMENT CONTROLLER STUFF AGAIN
                _gameobject = loadGameobject("EyeTrackingProviders/XTAL/vrgineers/", "Controllers");

                break;


            case Providers.HTCViveSranipal:

                // 1
                _gameobject = loadGameobject("SteamVR/Prefabs/", "[CameraRig]");
                this.cams = _gameobject.GetComponentsInChildren<Camera>();
                if (this.cams.Length > 0)
                {
                    this.cams[0].tag = "MainCamera";
                    this.cams[0].enabled = true;
            
                Debug.Log("Main Camera is set");
                }
                else
                {
                    Debug.LogError("There are multiple cameras. Could not decide which to set to main camera.");
                }

                Debug.LogError("Loading SR Anipal");

                // 2
                _gameobject = loadGameobject("EyeTrackingProviders/SuperRealityAnipal/ViveSR/Prefabs/", "SRanipal Eye Framework");
                _gameobject.GetComponent<SRanipal_Eye_Framework>().EnableEye = true;
                _gameobject.GetComponent<SRanipal_Eye_Framework>().EnableEyeDataCallback = true;
                _gameobject.GetComponent<SRanipal_Eye_Framework>().EnableEyeVersion = SRanipal_Eye_Framework.SupportedEyeVersion.version2;


                //_gameobject = null;
                break;


            case Providers.XTAL:
                _gameobject = loadGameobject("EyeTrackingProviders/XTAL/vrgineers/", "CameraOrigin");

                this.cams = _gameobject.GetComponentsInChildren<Camera>();

                if (this.cams.Length >= 0)
                {
                    this.cams[1].tag = "MainCamera";
                    this.cams[1].enabled = true;

                    // Condition 4
                    //rvCameraAddComponent<QueueRenderTexture>();
                    Debug.Log("Main Camera is set");
                }
                else
                {
                    Debug.LogError("There are multiple cameras. Could not decide which to set to main camera.");
                }
                _gameobject = loadGameobject("EyeTrackingProviders/XTAL/vrgineers/", "CameraOrigin");


                // 2
                _gameobject = loadGameobject("EyeTrackingProviders/XTAL/vrgineers/", "Controllers");


                //cameraObject.transform.Rotate(new Vector3(0, 0, 0));
                //cameraObject.transform.Translate(new Vector3(0f, 0f, 0f));

            


                break;
            default:

                Debug.Log("No Eye-Tracking Vendor picked or detected. Loading SteamVR.");
                // 1
                _gameobject = loadGameobject("SteamVR/Prefabs/", "[CameraRig]");
                Debug.LogWarning(_gameobject);
                this.cams = _gameobject.GetComponentsInChildren<Camera>();
                if (this.cams.Length > 0)
                {
                    this.cams[0].tag = "MainCamera";
                    this.cams[0].enabled = true;

                    Debug.Log("Main Camera is set");
                }
                else
                {
                    Debug.LogError("There are multiple cameras. Could not decide which to set to main camera.");
                }
                break;

        }
    }

 
    public GameObject loadGameobject(string path, string name)
    {
        GameObject instance = null;
        if (!GameObject.Find(name))
        {
            Debug.Log(path + name);
            instance = GameObject.Instantiate(Resources.Load(path + name, typeof(GameObject))) as GameObject;
            instance.name = name;
            Object.DontDestroyOnLoad(instance);
        }
            

        
        
        return instance;
    }


    public void startET()
    {
        this.etpc.SubscribeToGaze();

        // writes into a Queue
        this.etpc.startETThread();
        ///this.eventTracker.startGazeWriting();
        
    }

    public bool setPositionAndIPD()
    {
        this.etpc?.CalibratePositionAndIPD();
        return true;
    }

    public void stop()
    {
        this.etpc?.stop();
        this.etpc?.UnsubscribeToGaze();

    }

    public void close()
    {
        
        this.etpc?.Close();
       
    }



    public string GetUserFolder()
    {
        return userFolder;
    }

    public string GetDataFolder()
    {
        if (dataFolder == null)
        {
            dataFolder = "D:\\";
        }
        Debug.LogError("DataFolder is set to " + dataFolder);

        return dataFolder;
    }

    public void UpdateUserFolder(string _userId, string _userAge, string _gender, latinsquaregroups _selectedLatinGroupId, string _etEx, string _vrEx)
    {

        userFolder = createUserFolder(_userId, _userAge, _gender, _selectedLatinGroupId, _etEx, _vrEx);

    }

    private string createUserFolder(string userID, string _userAge, string _gender, latinsquaregroups _selectedLatinGroupId, string _etEx, string _vrEx)
    {
        string oldName;
        string userFolder;
        int fileCounter = 1;
        userFolder = this.GetDataFolder() + userID;

        Debug.Log("Userfolder in ZERO is set to " + userFolder.ToString());

        oldName = userFolder;

        while (Directory.Exists(userFolder))
        {
            userFolder = oldName + fileCounter.ToString();
            fileCounter += 1;
        }

        Directory.CreateDirectory(userFolder);


        var userFilePath = userFolder + "/" + userID + ".txt";
        using (StreamWriter sw = File.AppendText(userFilePath))
        {
            sw.WriteLine("Name\tAge\tGender\tGroupID\tET-Experience?\tVR-Experience?");
            sw.Flush();
            sw.WriteLine(userID + "\t" + _userAge + "\t" + _gender + "\t" + _selectedLatinGroupId.ToString() + "\t" + _etEx + "\t" + _vrEx);
            sw.Flush();
        }

        return userFolder;
    }

}
