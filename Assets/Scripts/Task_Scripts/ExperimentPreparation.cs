using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // csv writing "select"?


public class ExperimentPreparation
{
    // randomly permute elements of float array
    public static void RandPermute(float[] array) // Fisher–Yates shuffle
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int r = UnityEngine.Random.Range(0, i + 1);
            // swap array[r] and array[i]
            float temp = array[r];
            array[r] = array[i];
            array[i] = temp;
        }
    }
    // randomly permute elements of int array
    public static void RandPermute(int[] array) // Fisher–Yates shuffle
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int r = UnityEngine.Random.Range(0, i + 1);
            // swap array[r] and array[i]
            int temp = array[r];
            array[r] = array[i];
            array[i] = temp;
        }
    }

    // returns a random permutation of integers from 0 to maxIndex. (random permutation of indices)
    // This can be used in combination with 'PermuteWithInd' to shuffle multiple arrays with the 
    // same permutation (for example arrays for different trial conditions that should keep the same sampling)
    // If you have a condition which alternates between trials, then set alternateCondition to true and the
    // function will permute even with even indices and odd with odd indices
    public static int[] RandIndexPermutation(int maxIndex, bool alternateCondition)
    {
        int[] indices = new int[maxIndex];

        if (alternateCondition)
        {
            int[] tmp_odd;
            int[] tmp_even;
            tmp_odd = new int[maxIndex / 2];
            if (maxIndex % 2 == 0) // if maxIndex is even
            {
                 tmp_even = new int[maxIndex / 2];
            }
            else
            {
                tmp_even = new int[maxIndex / 2 + 1];
            }
            
            for (int i = 0; i < maxIndex / 2; i++)
            {
                tmp_even[i] = 2 * i;
                tmp_odd[i] = 2 * i + 1;
            }
            if (maxIndex % 2 == 1) // if maxIndex is odd
            {
                 tmp_even[maxIndex / 2 + 1] = maxIndex - 1;
            }
            RandPermute(tmp_even);
            RandPermute(tmp_odd);
            for (int i = 0; i < maxIndex / 2; i++)
            {
                indices[2*i] = tmp_even[i];
                indices[2*i + 1] = tmp_odd[i];
            }
            if (maxIndex % 2 == 1) // if maxIndex is odd
            {
                indices[maxIndex - 1] = tmp_even[maxIndex / 2 + 1];
            }
        }
        else
        {
            for (int i = 0; i < maxIndex; i++)
            {
                indices[i] = i;
            }
            RandPermute(indices);
        }
        return indices;
    }
    public static int[] RandIndexPermutation(int maxIndex)
    {
        int[] indices = new int[maxIndex];
        for (int i = 0; i < maxIndex; i++)
        {
            indices[i] = i;
        }
        RandPermute(indices);
        return indices;
    }

    public static void PermuteWithInd(float[] array, int[] indices)
    {
        float[] tmp = new float[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            tmp[i] = array[indices[i]];
        }
        Array.Copy(tmp,array,tmp.Length);
    }
    
    public static void PermuteWithInd(int[] array, int[] indices)
    {
        int[] tmp = new int[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            tmp[i] = array[indices[i]];
        }
        Array.Copy(tmp,array,tmp.Length);
    }

    // returns an array with the elements of 'fillValues' repeated as given by repElement and repArray:
    // First repeat each element of 'fillValues' 'repElement' times
    // Then repeat the result 'repArray' times
    // Example: 
    // fillValues = [1 2 3 4]; repElement = 3; repArray = 2;
    // retuns -> [1 1 1 2 2 2 3 3 3 4 4 4 1 1 1 2 2 2 3 3 3 4 4 4]
    public static float[] FillWithSamples(float[] fillValues, int repElement, int repArray)
    {
        int length = fillValues.Length * repElement * repArray;
        float[] output = new float[length];
        int counter = 0;
        for(int outer = 0; outer < repArray; outer++)
        {
            for(int elem = 0; elem < fillValues.Length; elem++)
            {
                for(int inner = 0; inner < repElement; inner++)
                {
                    output[counter] = fillValues[elem];
                    counter++;
                }
            }
        }
        return output;
    }
    //integer vesion of the function above
    public static int[] FillWithSamples(int[] fillValues, int repElement, int repArray)
    {
        int length = fillValues.Length * repElement * repArray;
        int[] output = new int[length];
        int counter = 0;
        for(int outer = 0; outer < repArray; outer++)
        {
            for(int elem = 0; elem < fillValues.Length; elem++)
            {
                for(int inner = 0; inner < repElement; inner++)
                {
                    output[counter] = fillValues[elem];
                    counter++;
                }
            }
        }
        return output;
    }
    public static string GetOutputPath(int subjNumber, string Cond1)
    {
        string outputPath = subjNumber.ToString("D2") + "_" + Cond1;
        if (File.Exists(outputPath + ".csv"))
        {
            int rep = 1;

            while (File.Exists(outputPath + "_" + rep.ToString("D2") + ".csv"))
            {
                rep++;
            }
            outputPath += "_" + rep.ToString("D2");
        }
        return outputPath + ".csv";
    }
    // save 1d array to file with one header name
    public static void SaveArrayToCSV(float[] arrayToSave, string csvPath, string header)
    {
        if (!File.Exists(csvPath))
        {
            var outputFile = File.CreateText(csvPath);
            outputFile.WriteLine(header);
            for (int i = 0; i < arrayToSave.Length; i++)
            {
                outputFile.WriteLine(arrayToSave[i].ToString());
            }
            outputFile.Close();
        }
        else // append to file a new column
        {
            var csv = File.ReadLines(csvPath) // not AllLines
                .Select((line, index) => index == 0
                ? line + "\t" + header
                : line + "\t" + arrayToSave[index - 1].ToString())
                .ToList(); // we should write into the same file, that´s why we materialize
            File.WriteAllLines(csvPath, csv);
        }
    }

    public static void SaveArrayToCSV(int[] arrayToSave, string csvPath, string header)
    {
        if (!File.Exists(csvPath))
        {
            var outputFile = File.CreateText(csvPath);
            outputFile.WriteLine(header);
            for (int i = 0; i < arrayToSave.Length; i++)
            {
                outputFile.WriteLine(arrayToSave[i].ToString());
            }
            outputFile.Close();
        }
        else // append to file a new column
        {
            var csv = File.ReadLines(csvPath) // not AllLines
                .Select((line, index) => index == 0
                ? line + "\t" + header
                : line + "\t" + arrayToSave[index - 1].ToString())
                .ToList(); // we should write into the same file, that´s why we materialize
            File.WriteAllLines(csvPath, csv);
        }
    }

    // save 2d array with list of header names
    public static void SaveArrayToCSV(float[,] arrayToSave, string csvPath, List<string> headers)
    {
        // call the SaveArray function for 1D array for every single column of 2D input array
        int rows = arrayToSave.GetLength(0);
        int cols = arrayToSave.GetLength(1);
        for (int c = 0; c < cols; c++)
        {
            float[] tmp = new float[rows];
            for (int r = 0; r < rows; r++)
            {
                tmp[r] = arrayToSave[r,c];
            }
            SaveArrayToCSV(tmp,csvPath,headers[c]);
        }
    }

    public static void SaveArrayToCSV(int[,] arrayToSave, string csvPath, List<string> headers)
    {
        // call the SaveArray function for 1D array for every single column of 2D input array
        int rows = arrayToSave.GetLength(0);
        int cols = arrayToSave.GetLength(1);
        for (int c = 0; c < cols; c++)
        {
            int[] tmp = new int[rows];
            for (int r = 0; r < rows; r++)
            {
                tmp[r] = arrayToSave[r,c];
            }
            SaveArrayToCSV(tmp,csvPath,headers[c]);
        }
    }
    
    // save 3d array with list of list of header names
    public static void SaveArrayToCSV(float[,,] arrayToSave, string csvPath, List<List<string>> headers)
    {
        // call the SaveArray function for 1D array for every single column of 2D input array
        int rows = arrayToSave.GetLength(0);
        int cols1 = arrayToSave.GetLength(1);
        int cols2 = arrayToSave.GetLength(2);
        for (int c1 = 0; c1 < cols1; c1++)
        {
            for (int c2 = 0; c2 < cols2; c2++)
            {
                float[] tmp = new float[rows];
                for (int r = 0; r < rows; r++)
                {
                    tmp[r] = arrayToSave[r,c1,c2];
                }
                SaveArrayToCSV(tmp,csvPath,headers[c1][c2]);
            }
        }
    }

    public static void SaveArrayToCSV(string[] arrayToSave, string csvPath, string header)
    {
        if (!File.Exists(csvPath))
        {
            var outputFile = File.CreateText(csvPath);
            outputFile.WriteLine(header);
            for (int i = 0; i < arrayToSave.Length; i++)
            {
                outputFile.WriteLine(arrayToSave[i]);
            }
            outputFile.Close();
        }
        else // append to file a new column
        {
            var csv = File.ReadLines(csvPath) // not AllLines
                .Select((line, index) => index == 0
                ? line + "\t" + header
                : line + "\t" + arrayToSave[index - 1])
                .ToList(); // we should write into the same file, that´s why we materialize
            File.WriteAllLines(csvPath, csv);
        }
    }
}