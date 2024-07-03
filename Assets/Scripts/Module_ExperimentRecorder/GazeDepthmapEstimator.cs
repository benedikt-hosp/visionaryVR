using UnityEngine;
using OpenCVForUnity;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgprocModule;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Concurrent;
using Source.ExperimentManagement.Management;

public class GazeDepthmapEstimator
{

    List<double> estimatedDepths;
    Scalar mean;
    List<Scalar> allMeans;
    List<double> uniqueMeans;
    MonoBehaviour _mb;
    private ConcurrentQueue<Mat> depthQueue;
    private Material depthMaterial;

    public double currentEstimatedDepth = 0;
    public int saveFrameFrequency = 10;
    public int framecounter = 0;
    public int scaledWidth = 0; 
    public int scaledHeigth = 0;

    public int cropWidth = 200;
    public int cropHeight = 200;

    Material ocvMaterial;        // TESTING
    GameObject imagePlane;

    public GazeDepthmapEstimator(GameObject camObj)
    {
        depthMaterial = new Material(Shader.Find("Hidden/DepthRecording"));


        // TESTING
        ocvMaterial = new Material(Shader.Find("Diffuse"));
        imagePlane = GameObject.Find("Plane");
        imagePlane.GetComponent<Renderer>().material = ocvMaterial;


        scaledWidth = ExperimentController.resolutionX / ExperimentController.scalefactorDEPTH;
        scaledHeigth = ExperimentController.resolutionY / ExperimentController.scalefactorDEPTH;

        mean = new Scalar(-1);
        allMeans = new List<Scalar>();
        uniqueMeans = new List<double>();
        estimatedDepths = new List<double>();
        _mb = GameObject.FindObjectOfType<MonoBehaviour>();
        if(camObj.GetComponent<QueueRenderTexture>() == null)
        {
            camObj.AddComponent<QueueRenderTexture>();
            this.depthQueue = camObj.GetComponent<QueueRenderTexture>()._queueDepth;
            
        }
    }



    public List<double> getDepthOfGazedImagePatch(Vector3 gazePos)
    {
        Mat currentTextureMat;
        this.depthQueue.TryDequeue(out currentTextureMat);   

        //if (gazePos != null)
         //   return null;

        if (currentTextureMat != null && (currentTextureMat.size().width > 0) && (currentTextureMat.size().height > 0) &&
            currentTextureMat.width() >= 0 && currentTextureMat.height() >= 0 && currentTextureMat.size() != new Size(0,0))
        {
            framecounter++;
            Debug.Log("3. Rt is not null");
            if (saveFrameFrequency % framecounter == 1)
            {
                framecounter = 0;
                return calculateDepthOfGazedImagePatch(currentTextureMat, gazePos);

            }
            return null;
        }
        else
        {
            Debug.LogError("3. Rt is null");

            return null;
        }
    }
    public List<double> calculateDepthOfGazedImagePatch(Mat src, Vector3 gazePos)
    {
        return processFrame(getSubTexture(src, gazePos));

    }


    private Mat getSubTexture(Mat currentDepthMap, Vector3 gazePos)
    {

        // 1. scale GazePosition from 4k to RendertextureSize
        int x_scaled = (int)Mathf.Abs(gazePos.x);
        x_scaled /= ExperimentController.scalefactorDEPTH;

        int y_scaled = (int)Mathf.Abs(gazePos.y);
        y_scaled /= ExperimentController.scalefactorDEPTH;

        // 2. Define window to cut off of size cropWidth x cropHeight
        // with x/y as center

        // window start point
        //int x_start = x_scaled - (cropWidth / 2);      
        //int y_start = y_scaled - (cropHeight / 2);

        //// window end point
        //int x_end = x_scaled + (cropWidth / 2);
        //int y_end = y_scaled + (cropHeight / 2);

        int x_start = x_scaled;
        int y_start = y_scaled;

        // window end point
        int x_end = x_scaled + cropWidth;
        int y_end = y_scaled + cropHeight;

        // if to small, we simply start at 0/0
        if (x_start < 0)
            x_start = 0;

        if (y_start < 0)
            y_start = 0;

        if(x_end > cropWidth)
            x_end = cropWidth;


        if(y_end > cropHeight)
            y_end = cropHeight;

        Debug.Log("4. Real gaze point x:" + gazePos.x.ToString() + " y: " + gazePos.y.ToString() + " z: " + gazePos.z.ToString()); // + " width: " + currentDepthMap.width().ToString() + " height: " + currentDepthMap.height().ToString());
        Debug.Log("5. Scaled window is x: " + x_scaled.ToString() + " y: " + y_scaled.ToString() + " width: " + x_end.ToString() + " height: " + y_end.ToString());
        Debug.Log("6. Start point x: " + x_start.ToString() + " y: " + y_start.ToString() + " End point x: " + x_end.ToString() + " y:" + y_end.ToString());

        Mat cropped = new Mat(currentDepthMap.rows(), currentDepthMap.cols(), CvType.CV_8UC4);
        Utils.setDebugMode(true);
        if (0 <= y_start && y_start <= y_end && y_end <= currentDepthMap.height() && 0 <= x_start && x_start <= x_end && x_end <= currentDepthMap.width())
        if (0 <= y_start && y_start <= y_end && y_end <= currentDepthMap.height() && 0 <= x_start && x_start <= x_end && x_end <= currentDepthMap.width())
        { 
            cropped = currentDepthMap.submat(x_start, cropHeight, y_start, cropWidth);
        }
        //cropped = currentDepthMap.submat(0, 100, 0, 100);
        Utils.setDebugMode(false);
        //Mat cropped = currentDepthMap;


        return cropped;
    }


    public List<double> processFrame(Mat src)
    {
        // 1. create mat
        //Mat src = Texture2DToMat(inputTex);

        // TODO BHO: SHOW IMAGE OF CANNY IN UNITY
        Texture2D texture2D = new Texture2D(src.width(), src.height());
        Utils.matToTexture2D(src, texture2D);
        ocvMaterial.mainTexture = texture2D;

        Mat src_gray = new Mat();
        Mat cannyOut = new Mat();

        Imgproc.cvtColor(src, src_gray, Imgproc.COLOR_BGR2GRAY);
        //src_gray = src.clone();

        // 2. Blur image with 3x3 kernel
        Imgproc.blur(src_gray, src_gray, new Size(3, 3));

        // 3. Improve edges with laplacian og gaussians
        Imgproc.Laplacian(src_gray, src_gray, 0, 5);

        // 4. copy make border
        Core.copyMakeBorder(src_gray, cannyOut, 2, 2, 2, Core.BORDER_CONSTANT, 255);

        // 5. Find contours
        Mat hierarchy = new Mat();


        List<MatOfPoint> contours = new List<MatOfPoint>();
        //Imgproc.findContours(cannyOut, contours, hierarchy, Imgproc.RETR_TREE, Imgproc.CHAIN_APPROX_SIMPLE, new Point(0,0));
        Imgproc.findContours(cannyOut, Imgproc.RETR_TREE, contours, hierarchy, Imgproc.RETR_TREE, Imgproc.CHAIN_APPROX_SIMPLE);


        Debug.Log("7. Finding contours");

        // 6. cut regions and mask original to get mean value of depth

        for (int i = 0; i < contours.Count; i++)
        {
            Mat mask = Mat.zeros(cannyOut.size(), CvType.CV_8U);

            // here you can draw it if you want
            this.mean = Core.mean(src_gray, mask);
            Debug.Log("8. Mean of current contour is: " + this.mean.ToString());
            this.allMeans.Add(this.mean);
        }

        for (int i = 0; i < this.allMeans.Count; i++)
        {
            if(i != 0 && this.allMeans[i] != this.allMeans[i-1])
            { 
                var t = getMetersFromDepth(this.allMeans[i]);
                if (!this.uniqueMeans.Contains(t))
                {
                    this.uniqueMeans.Add(t);
                }
            }
            
        }

        // Sort unique means
        this.uniqueMeans.Sort();

        //Debug.Log(uniqueMeans.ToString());
        this.estimatedDepths = this.uniqueMeans;

        return this.estimatedDepths;
    }


    public double getMetersFromDepth(Scalar inputDepth)
    {

        Debug.Log("Found depth as scalar is: " + inputDepth.ToString());

        // Original
        double output_end = 100;
        double output_start = 1;
        double input_end = 256;
        double input_start = 1;

        //double output_end = 100.0;
        //double output_start = 0.0;
        //double input_end = 256.0;
        //double input_start = 0.0;

        double slope = 1.0 * (output_end - output_start) / (input_end - input_start);
        //double output = output_start + slope * (inputDepth.val[0] - input_start);
        double output = output_start + slope * (inputDepth.val[2] - input_start);
        Debug.Log("Found distance in meter in image: " + output.ToString());
        return Mathf.Abs((float)output);
    }

    public Mat Texture2DToMat(Texture2D imgTexture)
    {

        Mat imgMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC4);

        Utils.texture2DToMat(imgTexture, imgMat);
        Debug.Log("imgMat.ToString() " + imgMat.ToString());

        return imgMat;

    }

    public Texture2D MatToTexture2D(Mat imgMat)
    {
        Texture2D texture = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);

        Utils.matToTexture2D(imgMat, texture);

        return texture;
    }
}
