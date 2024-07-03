using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthSample
{

    public DepthSample(float gtDepth, float gpX, float gpY, float gpZ, float gOriginRX, float gOriginRZ, float gOriginLX, float gOriginLZ, float gDirectionRX, float gDirectionRY, float gDirectionRZ, float gDirectionLX, float gDirectionLY, float gDirectionLZ)
    {
        this.gtDepth = gtDepth;
        this.gpX = gpX;
        this.gpY = gpY;
        this.gpZ = gpZ;
        this.gOriginRX = gOriginRX;
        this.gOriginRZ = gOriginRZ;
        this.gOriginLX = gOriginLX;
        this.gOriginLZ = gOriginLZ;
        this.gDirectionRX = gDirectionRX;
        this.gDirectionRY = gDirectionRY;
        this.gDirectionRZ = gDirectionRZ;
        this.gDirectionLX = gDirectionLX;
        this.gDirectionLY = gDirectionLY;
        this.gDirectionLZ = gDirectionLZ;
        this.eccentricityL = eccentricityL;
        this.eccentricityR = eccentricityR;
    }

    public float gtDepth { get; set; }

    public float gpX { get; set; }
    public float gpY { get; set; }
    public float gpZ { get; set; }


    public float gOriginRX { get; set; }
    public float gOriginRY { get; set; }
    public float gOriginRZ { get; set; }


    public float gOriginLX { get; set; }
    public float gOriginLY { get; set; }
    public float gOriginLZ { get; set; }

    public float gDirectionRX { get; set; }
    public float gDirectionRY { get; set; }
    public float gDirectionRZ { get; set; }

    public float gDirectionLX { get; set; }
    public float gDirectionLY { get; set; }
    public float gDirectionLZ { get; set; }

    public float eccentricityL { get; set; }
    public float eccentricityR { get; set; }


}
