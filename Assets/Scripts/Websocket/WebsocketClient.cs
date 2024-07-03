using WebSocketSharp;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class WebsocketClient
{
    WebSocket ws;
    // create and start a Stopwatch instance
    Stopwatch stopwatch;
    ConditionController conditionController;
    BaselineLevelController baselineController;
    String[] separator = { ";" };
    String[] x = null;

    public WebsocketClient(ConditionController m_conditionController)
    {
        conditionController = m_conditionController;
        //ws = new WebSocket("ws://localhost:8765");
        ws = new WebSocket("ws://localhost:8585");
        List<string> depthValues = new List<string>();

/*
        ws.OnMessage += (sender, e) =>
        {
            
            x = e.Data.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            ParallelOptions options = new ParallelOptions()
            {
                // A max of 10 threads can access the file at one time.
                MaxDegreeOfParallelism = 10
            };

            // Start the loop and store the result, so we can check if all the threads are done.
            // The Parallel.For will do all the mutlithreading for you!
            ParallelLoopResult result = Parallel.For(0, x.Length, options, (i) =>
            {
                depthValues.Add(x[i]);
            });


            //for (int i = 0; i < x.Length; i++)
           // {
                //depthValues.Add(x[i]);
                //Debug.Log("X: " + x[i]);

           // }

            stopwatch.Stop();
            Debug.Log("Stopped time: " + stopwatch.ElapsedMilliseconds);
            conditionController.AcceptWebsocketAnswerDepths(depthValues);
            
        };
*/

    }

    public WebsocketClient(BaselineLevelController m_baselineController)
    {
        baselineController = m_baselineController;
        ws = new WebSocket("ws://localhost:8585");
        

        ws.OnMessage += (sender, e) =>
        {

            //baselineController.AcceptWebsocketAnswerDepths(e.Data);

        };

    }


    public void sendMSG(string msg)
  {
        ws.Connect();
        if (ws == null)
      {
        Debug.LogError("socket is null");
        ws.Connect();

      }
      Debug.Log("Sending msg: " + msg);
      ws.Send(msg);
  }

    public void sendImage(Texture2D texture)
    {
        stopwatch = Stopwatch.StartNew();
        //byte[] return_buff = new byte[(int)(texture.total() * texture.channels())];
        //texture.get(0, 0, return_buff);

        byte[] bArray = texture.EncodeToPNG();
        ws.Connect();
        if(ws.IsAlive)
        { 
            ws.Send(bArray);
        }
        Debug.Log("Image send");
    }

}
