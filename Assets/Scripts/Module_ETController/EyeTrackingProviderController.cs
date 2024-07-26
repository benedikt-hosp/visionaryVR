using System;
using UnityEngine;


public class EyeTrackingProviderController
{
    public EyeTrackingProviderInterface eyeTrackingProviderInterface;

    private const string SranipalProviderName = "SRanipalProvider";
    private const string TobiiXRProviderName = "TobiiXRProvider";
    private const string PupilProviderName = "PupilProvider";
    private const string TobiiProProviderName = "TobiiProProvider"; // BHO
    private const string XTALProviderName = "XTALProvider"; // BHO
    public bool ETReady = false;

    // default value
    private string _currentProviderName = TobiiProProviderName;
    private Providers providerSDK;

    public EyeTrackingProviderInterface getSetETProvider { get { return this.eyeTrackingProviderInterface; } }


    public EyeTrackingProviderController(Providers providerSDK)
    {
        this.providerSDK = providerSDK;
        if (this.providerSDK == Providers.None) 
            this.eyeTrackingProviderInterface = null;
        else
            this.InitGazeProvider();

        return;

        
        
    }

    private void InitGazeProvider()
    {
        if (this.eyeTrackingProviderInterface != null) return;
        Debug.Log("Initializing provider: " + this.providerSDK);
        UpdateCurrentProvider();

        this.eyeTrackingProviderInterface = GetProvider();



        if (this.eyeTrackingProviderInterface != null)
        {
            bool success = this.eyeTrackingProviderInterface.initializeDevice();
            if (success)
            {
                Debug.Log("Initialized device!");
                this.ETReady = true;
            }
            else
                Debug.LogError("Cannot initialize device");

        }
        else
        {
            Debug.LogError("ETPC: does not work!");
        }

    }

    private void UpdateCurrentProvider()
    {

        switch (this.providerSDK)
        {
            case Providers.HTCViveSranipal:
                _currentProviderName = SranipalProviderName;
                break;
            case Providers.PupiLabs:
                _currentProviderName = PupilProviderName;
                break;
            case Providers.TobiiXR:
                _currentProviderName = TobiiXRProviderName;
                break;
            case Providers.TobiiPro:                                        // BHO
                _currentProviderName = TobiiProProviderName;                    // BHO
                break;
            case Providers.XTAL:
                _currentProviderName = XTALProviderName;
                break;
            default:
                return;
        }

    }

    private EyeTrackingProviderInterface GetProvider()
    {
        return GetProviderFromName(_currentProviderName);
    }

    /* Searches for the choosen Implementation file of the EyeTrackingProviderInterface
   * and activates it. So we use the correct file which corresponds to the currently choosen Eye-Tracker
   */
    public EyeTrackingProviderInterface GetProviderFromName(string ProviderName)
    {
        Type providerType = Type.GetType(ProviderName);
        if (providerType == null)
        {
            Debug.Log("provider type not found");
            return null;
        }
        else
        {
            Debug.Log("Found provider " + providerType.FullName + " going to load it...");
        }

        try
        {
            var tmp = Activator.CreateInstance(providerType) as EyeTrackingProviderInterface;
            if (tmp != null)
            {
                Debug.Log("Activated instance of provider" + tmp.ToString());
            }
            return tmp;
        }
        catch (Exception)
        {
            Debug.LogError("There was an error instantiating the gaze provider: " + ProviderName);
        }
        return null;
    }

    // ======================================== Calibration
    public void CalibrateEyeTracker()
    {
        Debug.Log("EyetrackingProviderController started ET calibration");
        this.eyeTrackingProviderInterface?.calibrateET();
        
    }
    public void CalibratePositionAndIPD()
    {
        
        Debug.Log("EyetrackingProviderController started IPD calibration");
        this.eyeTrackingProviderInterface?.calibratePositionAndIPD();
            
        
    }




    public void startETThread()
    {
        this.eyeTrackingProviderInterface?.startETThread();
    }

    public void stop()
    {
        this.eyeTrackingProviderInterface?.stopETThread();
    }

    public void Close()
    {
        this.eyeTrackingProviderInterface?.close();
    }


    public bool SubscribeToGaze()
    {
        bool registrered = false;
        //Debug.Log("Now registered");
        if (this.eyeTrackingProviderInterface != null)
        { 
            registrered = this.eyeTrackingProviderInterface.subscribeToGazeData();
        }
        if (!registrered)
            Debug.LogError("Could not subscribe to gaze");

        return registrered;
            
    }

    public bool UnsubscribeToGaze()
    {
        var registrered = this.eyeTrackingProviderInterface?.UnsubscribeToGazeData() ?? false;
        
        if (!registrered)
            Debug.LogError("Could not subscribe to gaze");
        return registrered;

    }


}