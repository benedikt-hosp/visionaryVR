[System.Serializable]
public enum BACKGROUND_COLOR_MODES
{
    BRIGHT,
    DARK
}
public enum SimulatedDepths
{
    _40cm,
    _1Meter,
    _6Meter
}
public enum CalibrationPatterns
{
    FIVE,
    NINE
}

public enum Providers
{
    None,
    HTCViveSranipal,
    TobiiXR,
    TobiiPro,
    PupiLabs,
    XTAL

}

public enum VRProdivers
{
    None,
    StarVR,
    HTCViveProEye,
    HTCVive2,
    XTAL
}


// Add VR-Glasses name
// then we can also change the field of view according to the glasses

// StarVR
// HTC Vive Pro Eye
// HTC Vive PupilLabs
// XTAL
// Varjo X3