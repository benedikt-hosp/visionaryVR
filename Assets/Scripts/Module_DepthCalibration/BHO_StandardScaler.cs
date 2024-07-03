using SharpLearning.Containers.Extensions;
using SharpLearning.Containers.Matrices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;



/// <summary>
/// Standardize features by removing the mean and scaling to unit variance.
/// more : https://scikit-learn.org/stable/modules/generated/sklearn.preprocessing.StandardScaler.html
/// </summary>
public class BHO_StandardScaler
{
    private List<double> _mean;
    private List<double> _standardDeviation;


    /// <summary>
    /// fit then transform
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public F64Matrix FitTransform(SharpLearning.Containers.Matrices.F64Matrix trainSamples)
    {
        return Fit(trainSamples).Transform(trainSamples);
    }

    public BHO_StandardScaler Fit(SharpLearning.Containers.Matrices.F64Matrix trainSamples)
    {
        _mean = new List<double>();                 // to save all mean values
        _standardDeviation = new List<double>();    // to save all std devs

        if (trainSamples.RowCount < 1)
            throw new Exception("no data");

        List<double[]> dt = createListOfColumns(trainSamples);

        for (int i = 0; i < dt.Count; i++)
        {
            _mean.Add(dt[i].Average());
            _standardDeviation.Add(getStandardDeviation(dt[i]));
        }

        return this;
    }

    private List<double[]> createListOfColumns(F64Matrix samples)
    {

        // contains a list of columns, represented by a double array
        List<double[]> listOfColumns = new List<double[]>(samples.ColumnCount);

        // represents 1 columns as double array of values from each row
        double[] tmpCol = new double[samples.RowCount];


        for (int i = 0; i < samples.ColumnCount; i++)
        {

            //for (int j = 0; j < samples.RowCount; j++)
            //{
            // tmpCol[i] = samples.Column(i);
            // tmpCol[i] = samples.Data()
            //}
            //listOfColumns.Add(tmpCol);

            listOfColumns.Add(samples.Column(i));
        }

        return listOfColumns;
    }

    private double getStandardDeviation(double[] column)
    {
        double average = column.Average();
        double sumOfDerivation = 0;
        foreach (double value in column)
        {
            sumOfDerivation += (value) * (value);
        }
        double sumOfDerivationAverage = sumOfDerivation / (column.Length - 1);
        return Math.Sqrt(sumOfDerivationAverage - (average * average));
    }

    public F64Matrix Transform(F64Matrix testSamples)
    {
        if (_mean == null)
            throw new Exception("This StandardScaler instance is not fitted yet. Call 'Fit' with appropriate arguments before using this estimator.");

        // double[,] dt = createArraysOfColumns(testSamples);
        List<double[]> dtOut = createListOfColumns(testSamples);
        // dt = list of columns. Each column is represented as a double array
        // dtOut[0] = 1. Column, all values

        // c = columns
        for (int c = 0; c < dtOut.Count; c++)
        {
            // r = rows
            for (int r = 0; r < dtOut[c].Length; r++)
            {
                dtOut[c][r] = (dtOut[c][r] - _mean[c]) / _standardDeviation[c];
            }
        }

        // var list = ToListOfObject(dt);
        return dtOut.ToF64Matrix();
    }

    public double[] TransformSample(double[] sample)
    {
        //List<double> res = new List<double>();
        double[] res = new double[sample.Length];

        if (_mean == null)
            throw new Exception("This StandardScaler instance is not fitted yet. Call 'Fit' with appropriate arguments before using this estimator.");

        for (int c = 0; c < sample.Length; c++)
        {
            res[c] = (sample[c] - _mean[c]) / _standardDeviation[c];
        }
        return res;
    }


    /*
    private List<double> ToListOfObject(double[,] arr)
    {
        var res = new List<double>();

        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(double));
        var ObjectsCount = GetColumn(arr, 0).Length;

        for (int i = 0; i < ObjectsCount; i++)
        {
            double o = new double();
            for (int j = 0; j < properties.Count; j++)
                properties[j].SetValue(o, arr[j, i]);
            res.Add(o);
        }
        return res;

    }

    /// <summary>
    /// Reset then 
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public BHO_StandardScaler Fit(List<double[]> listOfOSamples)
    {
        _mean = new List<double>();                 // to save all mean values
        _standardDeviation = new List<double>();    // to save all std devs

        if (listOfOSamples.Count < 1)
            throw new Exception("no data");

        double[,] dt = createArraysOfColumns(listOfOSamples);

        for (int i = 0; i < dt.GetLength(1); i++)
        {
            _mean.Add((GetColumn(dt, i)).Average());
            _standardDeviation.Add(getStandardDeviation(GetColumn(dt, i)));
        }

        return this;
    }

    /// <summary>
    /// Get 
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public List<double> Transform(List<double[]> listOfObjects)
    {
        if (_mean == null)
            throw new Exception("This StandardScaler instance is not fitted yet. Call 'Fit' with appropriate arguments before using this estimator.");

        //var dt = listOfObjects.ToArraysOfColumns<T>();
        double[,] dt = createArraysOfColumns(listOfObjects);


        for (int c = 0; c < dt.GetLength(1); c++)
        {
            for (int r = 0; r < (GetColumn(dt, c)).Length; r++)
            {
                dt[c, r] = (dt[c, r] - _mean[c]) / _standardDeviation[c];
            }
        }
        return ToListOfObject(dt);
    }

        public double[] GetColumn(double[,] matrix, int columnNumber)
    {
        return Enumerable.Range(0, matrix.GetLength(0))
                .Select(x => matrix[x, columnNumber])
                .ToArray();
    }


        private double[,] createArraysOfColumns(List<double[]> listOfSamples)
    {
        int rows = listOfSamples[0].Length;


        double[,] arrayOfColumns = new double[listOfSamples.Count, rows];

        for (int i = 0; i < listOfSamples.Count; i++)
        {
            for (int j = 0; j < listOfSamples[i].Length; j++)
            {
                arrayOfColumns[i, j] = listOfSamples[i][j];
            }
        }

        return arrayOfColumns;
    }

    */

}





