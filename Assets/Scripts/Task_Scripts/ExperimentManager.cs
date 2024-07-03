using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // write to file

using Random=UnityEngine.Random;

public class ExperimentManager : MonoBehaviour
{
    public int nTrials;
    public string csvPath;
    StimulusManager screen;
    //Autofocal autofocal;

    public int[,] trialStimulus;
    int[] trialAnswer;
    float[] trialTimes;
    public int trial;
    public float matchRate; // how many of the trials should have a match
    DateTime stopwatch;
    
    void Start()
    {
        // get dependent coponents
        screen = GetComponent<StimulusManager>();
        //autofocal = GameObject.Find("Camera").GetComponent<autofocal_Controller>();
        // set autofocal condition
        // autofocal.mode = ...
        trialAnswer = new int[nTrials];
        trialTimes = new float[nTrials];
        trialStimulus = new int[nTrials,3];
        SetStimuli(8);
        StartTimer();
    }

    // Update is called once per frame
    void Update()
    {
        if (CheckForAnswer())
        {
            EndTimer();
            SaveData(csvPath);
            trial++;
            if (trial < nTrials)
            {
                SetStimuli(8);
                StartTimer(); // restart timer
            }
            else // end of experiment
            {
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            }
        }
    }

    bool CheckForAnswer() // return if subject answered
    {
        if (Input.GetKeyDown("f")) // no match - false
        {
            trialAnswer[trial] = -1;
            return true;
        }
        if (Input.GetKeyDown("j")) // it'sa match - true
        {
            trialAnswer[trial] = 1;
            return true;
        }
        return false;
    }

    void StartTimer()
    {
        stopwatch = DateTime.Now;
    }

    void EndTimer()
    {
        trialTimes[trial] = (float) (DateTime.Now - stopwatch).TotalSeconds;
    }

    // set the stimulus params for next trial
    void SetStimuli(int stimulusMaxInd)
    {
        // get two random permutations for the "stimulus table" shown on screen 0
        int[] permutation1 =  ExperimentPreparation.RandIndexPermutation(stimulusMaxInd, false);
        int[] permutation2 =  ExperimentPreparation.RandIndexPermutation(stimulusMaxInd, false);
        
        // get random order of screens -> randomly change which screen shows which stimulus
        int[] screenPermutation = ExperimentPreparation.RandIndexPermutation(3, false);

        // define stimulus position on screen TODO: depending on easy/hard condition place in center or on of the four corners
        float stim1_posx = 0.5f;
        float stim1_posy = 0.5f;
        float stim2_posx = 0.5f;
        float stim2_posy = 0.5f;

        bool match = (Random.Range(0.0f,1.0f) <= matchRate); // random match vs. non-match, following the defined matchRate
        Debug.Log("match trial");
        int stimulusIndex1 = Random.Range(0,stimulusMaxInd); // random index for screen 1
        int stimulusIndex2;
        if (match) // for matching stimuli, choose the two indices to have matching permutations
        {
            stimulusIndex2 = 0; // start at zero and loop through indices until permutation matches
            while(!MatchInPermutation(stimulusIndex1, stimulusIndex2, permutation1, permutation2))
            {
                stimulusIndex2++;
            }
        }
        else // stimuli should not match -> they should have different permutation index
        {
            stimulusIndex2 = Random.Range(0,stimulusMaxInd); // random index for stimulus on screen 2
            while(MatchInPermutation(stimulusIndex1, stimulusIndex2, permutation1, permutation2)) // in case they match get new index2
            {
                stimulusIndex2 = Random.Range(0,stimulusMaxInd);
            }
        }

        // get textures and show on screens
        screen.SetStimulusTex(screenPermutation[0], screen.GetStimulusTable(1 , 2, permutation1, permutation2));
        screen.SetStimulusTex(screenPermutation[1], screen.GetScreenTex(1, stimulusIndex1, stim1_posx, stim1_posy));
        screen.SetStimulusTex(screenPermutation[2], screen.GetScreenTex(2, stimulusIndex2, stim2_posx, stim2_posy));

        trialStimulus[trial,0] = permutation1[0];
        trialStimulus[trial,1] = stimulusIndex1;
        trialStimulus[trial,2] = stimulusIndex2;
        AddLineIntArray(csvPath, permutation1,permutation2); //TODO: add stimulus pos and screenPermutation
    }
    
    // Check if for the given two permutations p1 and p2 there is a x so that p1(x)=stimulusIndex1 AND p2(x)=stimulusIndex2 -> maticng stimuli
    bool MatchInPermutation(int stimulusIndex1, int stimulusIndex2, int[] permutation1, int[] permutation2)
    {
        int x = 0;
        while(permutation1[x]!=stimulusIndex1)
        {
            x++;
        }
        // now p1[x] = stimulusIndex1
        return (permutation2[x]==stimulusIndex2);
    }
    void AddLineIntArray(string path,int[] array1, int[] array2)
    {
        using (StreamWriter sw = File.AppendText(path))
        {
            sw.WriteLine(String.Join(";", new List<int>(array1).ConvertAll(i => i.ToString()).ToArray())
                       + "-" + String.Join(";", new List<int>(array2).ConvertAll(i => i.ToString()).ToArray()));
        }	
    }
    /*int[,,] ReadPermutationFile(string path)
    {
        List<List<int>> intList = new List<List<int>>();
        foreach (string line in File.ReadLines(path))
        {
            var tmp = line.Split(',');
            if (tmp.Length  == 0) // check if line was empty
            {
                intList.Add(tmp.Select(Int32.Parse)?.ToList());
            }
        }
        int nPermutations = intList.Count/2; // divide by 2, because there are two lines of permutations for each permutation combination
        int nStimuli = intList[0].Count;
        int[,,] permutations = new int[nPermutations,2,nStimuli]; // new int array with number of permutations/2 x 2 x number of stimuli elements
        lineCounter = 0;
        for (int i = 0; i < nPermutations; i++)
        {
            for (int j = 0; j < nStimuli; j++)
            {
                permutations[i,0,j] = intList[lineCounter][j];
            }
            lineCounter++;
            for (int j = 0; j < nStimuli; j++)
            {
                permutations[i,1,j] = intList[lineCounter][j];
            }
            lienCounter++;
        }
        return permutations;
    }*/

    void SaveData(string path)
    {
        // delete file to overwrite it
        File.Delete(path);
        // save trial angles and answers
        ExperimentPreparation.SaveArrayToCSV(trialStimulus, path, new List<string> {"screen1","screen2","screen3"});
        ExperimentPreparation.SaveArrayToCSV(trialAnswer, path, "answer");
        ExperimentPreparation.SaveArrayToCSV(trialTimes, path, "duration");
    }
}