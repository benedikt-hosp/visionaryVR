using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.Media;
//using OpenCVForUnity.VideoioModule;
//using OpenCVForUnity.CoreModule;
//using OpenCVForUnity.UtilsModule;
//using OpenCVForUnity.ImgcodecsModule;

using Debug = UnityEngine.Debug;
//using OpenCVForUnity.UnityUtils;
using System.Collections.Concurrent;
using Source.ExperimentManagement.Management;

public class SaveThreadObject
{
    //public int scalefactorRGB = 1;
    //public int scalefactorDEPTH = 1;
/*
    public bool depth = true;
    public bool isRunning = false;
    public VideoWriter videoWriter;
    Mat result;
    private MonoBehaviour _mb; // The surrogate MonoBehaviour that we'll use to manage this coroutine.

    double vidWidth;
    double vidHeight;
    int fourcc = VideoWriter.fourcc('H', '2', '6', '4');
    //int fourcc = VideoWriter.fourcc('M', 'J', 'P', 'G');
    //int fourcc = VideoWriter.fourcc('D', 'I', 'V', 'X');

    public string userPath;
    string filename;
    ConcurrentQueue<Mat> queue_d;
    ConcurrentQueue<Mat> queue_rgb;


    public void initialize(string userPath, GameObject _qrt, bool depth)
    {
        this.depth = depth;

        if (this.depth)
        {
            this.filename = "/depth.mp4";
            this.queue_d = _qrt.GetComponent<QueueRenderTexture>()._queueDepth;
            
            this.vidWidth = ExperimentController.resolutionX / ExperimentController.scalefactorDEPTH;
            this.vidHeight = ExperimentController.resolutionY / ExperimentController.scalefactorDEPTH;

            //this.vidWidth = (double)Screen.width / scalefactorDEPTH;
            //this.vidHeight = (double)Screen.height / scalefactorDEPTH;


            this.result = new Mat((int)vidWidth, (int)vidHeight, CvType.CV_8UC4);

            Size size = new Size(this.vidWidth, this.vidHeight);
            this.videoWriter = new VideoWriter();
            this.videoWriter.open(userPath + filename, fourcc, 30, size, true);

        }
        else
        {
            this.filename = "/RGB.mp4";
            this.queue_rgb = _qrt.GetComponent<QueueRenderTexture>()._queueRGB;

            this.vidWidth = ExperimentController.resolutionX / ExperimentController.scalefactorRGB;
            this.vidHeight = ExperimentController.resolutionY / ExperimentController.scalefactorRGB;


            //this.vidWidth = (double)Screen.width / scalefactorRGB;
            //this.vidHeight = (double)Screen.height / scalefactorRGB;
            this.result = new Mat((int)vidWidth, (int)vidHeight, CvType.CV_8UC4);

            Size size = new Size(this.vidWidth, this.vidHeight);
            this.videoWriter = new VideoWriter();
            this.videoWriter.open(userPath + filename, fourcc, 30, size, true);


        }
        this._mb = GameObject.FindObjectOfType<MonoBehaviour>();


        this.userPath = userPath;



    }



    public void writeDepthRecordingToVideoFile()
    {
        if (!this.videoWriter.isOpened())
        {
            Debug.Log("VideoWriter not opened");
        }

        bool success = false;

        while (isRunning)
        {

            if (this.depth)
            {
                success = this.queue_d.TryDequeue(out result);
                if (success)
                {
                    if (this.result != null)
                        this.videoWriter.write(result);
                }
            }
            else
            {
                success = this.queue_rgb.TryDequeue(out result);
                if (success)
                {
                    if (this.result != null)
                        this.videoWriter.write(result);
                }
            }




            if (!isRunning)
            {
                this.videoWriter.release();
                return;
            }

        }




    }*/

}

