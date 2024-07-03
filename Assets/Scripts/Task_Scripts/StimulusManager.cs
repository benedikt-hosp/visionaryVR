using System.Collections;
using System.Collections.Generic;
//using System;
using UnityEngine;
using System.IO;


public class StimulusManager : MonoBehaviour
{
    
    public string[] stimulusName;
    public Material[] screenMaterial;
    public string texturePath = "StimuliTextures"; // relative to Resources folder
    public int nScreens;
    int[] nStimuli;
    public int[] screenWidth;
    public int[] screenHeight;
    private Object[] stimulusTextures;

    
    void Awake()
    {
        nScreens = screenMaterial.Length;
        ClearAll();
        nStimuli  = new int[nScreens];
        for (int i = 0; i < nScreens; i++)
        {
            nStimuli[i] = CheckStimulusFiles(stimulusName[i]);
        }
        stimulusTextures = Resources.LoadAll(texturePath, typeof(Texture2D)); // load all textures from ./Resources/texturePath        
    }


    // set the texture of specific screen
    public void SetStimulusTex(int screen, Texture2D screenTex)
    {
        Destroy(screenMaterial[screen].mainTexture); // destroy old texture to prevent memory leak
        screenMaterial[screen].SetTexture("_MainTex" , screenTex); // set material texture
    }
    
    // return a screen texture for the stimulus of specific screen at given relative position [xPos,yPos] (in range [0;1])
    public Texture2D GetScreenTex(int screen, int stimulus, float xPos, float yPos)
    {
        Texture2D screenTex = new Texture2D(screenWidth[screen],screenHeight[screen],TextureFormat.RGBA32,-1,true);


        // Reset all pixels color to white
        Color32 resetColor = new Color32(255, 255, 255, 255);
        Color32[] resetColorArray = screenTex.GetPixels32();

        for (int i = 0; i < resetColorArray.Length; i++) {
            resetColorArray[i] = resetColor;
        }
        screenTex.SetPixels32(resetColorArray);
        screenTex.Apply();
        Texture2D stimulusTex = GetStimulusTex(screen,stimulus);
        
        Graphics.CopyTexture(stimulusTex, 0, 0, 0, 0, stimulusTex.width, stimulusTex.height, screenTex, 0, 0, (int) (xPos*screenTex.width - stimulusTex.width/2), (int)(yPos*screenTex.height - stimulusTex.height/2));
        screenTex.Apply();
        return screenTex;
    }

    public void ClearAll()
    {
        for(int i = 0; i < nScreens; i++)
        {
            Destroy(screenMaterial[i].mainTexture); // destroy old texture to prevent memory leak
            screenMaterial[i].SetTexture("_MainTex" , null);
        }
    }

    public Texture2D GetStimulusTex(int screen, int stimulus)
    {
        //Texture2D stimulusTex = new Texture2D(64,64,TextureFormat.RGBA32,-1,true); //Texture size does not matter, since LoadImage will replace with loaded image size.
        Texture2D stimulusTex = new Texture2D(128, 128, TextureFormat.RGBA32, -1, true); //Texture size does not matter, since LoadImage will replace with loaded image size.
        string textureName;
        if (nStimuli[screen] == 1) // only one stimulus for this screen
        {
            textureName = stimulusName[screen];
        }
        else if(stimulus > nStimuli[screen]) // stimulus index is higher then the actual stimuli number -> just take maximum index
        {
            textureName = stimulusName[screen] + nStimuli[screen].ToString();
        }
        else // add the current stimulus index to stimulusName for filename
        {
            textureName = stimulusName[screen] + stimulus.ToString();
        }
        //stimulusTex.LoadImage(bytes); // write bytes to texture
        stimulusTex.SetPixels( ((Texture2D) System.Array.Find(stimulusTextures, item => item.name == textureName)).GetPixels());// find the stimulus texture in the array of loaded textures
        stimulusTex.Apply();
        return stimulusTex;
    }

    public Texture2D GetStimulusTable(int stimulusScreen1 , int stimulusScreen2, int[] permutation1, int[] permutation2)
    {
        int nStimuli = permutation1.Length;
        float padding = 0.01f;
        // TODO: read screenTex of predefined size
        Texture2D screenTex = new Texture2D(screenWidth[0],screenHeight[0],TextureFormat.RGBA32,-1,true);
        // Reset all pixels color to white
        Color32 resetColor = new Color32(255, 255, 255, 255);
        Color32[] resetColorArray = screenTex.GetPixels32();

        for (int i = 0; i < resetColorArray.Length; i++) {
            resetColorArray[i] = resetColor;
        }
      
        screenTex.SetPixels32(resetColorArray);
        screenTex.Apply();
        for (int i = 0; i < nStimuli; i++ )
        {
            /*Texture2D stimulusTex = new Texture2D(1,1,TextureFormat.RGB24,-1,true); //Texture size does not matter, since LoadImage will replace with loaded image size.
            string path = Path.Join(texturePath , stimulusName[stimulusScreen1] + permutation1[i].ToString() + ".png");
            byte[] bytes = File.ReadAllBytes(path); // read the png file to bytes
            stimulusTex.LoadImage(bytes); // write bytes to texture*/

            Texture2D stimulusTex = GetStimulusTex(stimulusScreen1,permutation1[i]);


            int texPosX = screenTex.width/2 - nStimuli/2 * stimulusTex.width - (int)((nStimuli/2 - 1.0f + 0.5f)*padding*screenTex.width) + i * (int)(stimulusTex.width + padding*screenTex.width);
            int texPosY = screenTex.height/2 - (int) (0.5f*padding*screenTex.height) - stimulusTex.height;
            Graphics.CopyTexture(stimulusTex, 0, 0, 0, 0, stimulusTex.width, stimulusTex.height, screenTex, 0, 0, texPosX,texPosY);

            /*path = Path.Join(texturePath , stimulusName[stimulusScreen2] + permutation2[i].ToString() + ".png");
            bytes = File.ReadAllBytes(path); // read the png file to bytes
            stimulusTex.LoadImage(bytes); // write bytes to texture*/

            stimulusTex = GetStimulusTex(stimulusScreen2,permutation2[i]);
            texPosY = screenTex.height/2 + (int) (0.5f*padding*screenTex.height);
            Graphics.CopyTexture(stimulusTex, 0, 0, 0, 0, stimulusTex.width, stimulusTex.height, screenTex, 0, 0, texPosX,texPosY);
        }
        screenTex.Apply();
        return screenTex;
    }

    int CheckStimulusFiles(string name)
    {
        FileInfo[] info = (new DirectoryInfo(Path.Combine("Assets/Resources",texturePath))).GetFiles("*.png"); // get info for all png files in path

        int counter = 0;
        foreach (FileInfo f in info)
        {   
            if (f.Name.StartsWith(name))
            {
                counter++;
            }
        }
        return counter;
    }    
}
