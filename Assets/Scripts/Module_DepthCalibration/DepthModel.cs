using SharpLearning.Containers.Matrices;
using SharpLearning.InputOutput.Csv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using SharpLearning.CrossValidation.TrainingTestSplitters;
using SharpLearning.Metrics.Regression;
using SharpLearning.Containers;
using SharpLearning.Containers.Extensions;
using SharpLearning.XGBoost.Learners;
using SharpLearning.XGBoost.Models;
using SharpLearning.Optimization;
using SkLearn.Base;
using SkLearn.Core.Libraries.Numpy;
using System.Globalization;

public class DepthModel
{

    string pathToFile;
    F64Matrix samples;
    double[] labels;

    TrainingTestSetSplit trainingTestSplit;
    ObservationTargetSet trainSet;
    ObservationTargetSet testSet;
    RegressionXGBoostLearner m_learner;
    RegressionXGBoostModel model;
    double prediction;
    double[] predictions;

    private MeanSquaredErrorRegressionMetric metric;
    private MeanAbsolutErrorRegressionMetric absoluteError;

    private Stopwatch watch;
    BHO_StandardScaler scaler;
    int sampleCutOffPrecision = 8;

    int featureCount = 6; 

    public DepthModel(string folder)
    {
        this.scaler = new BHO_StandardScaler();
        watch = Stopwatch.StartNew();

        this.pathToFile = folder + "depthCalibration.csv";
        UnityEngine.Debug.Log("Depth model saving at " + this.pathToFile);

        metric = new MeanSquaredErrorRegressionMetric();
        absoluteError = new MeanAbsolutErrorRegressionMetric();
        //this.m_learner = new RegressionXGBoostLearner(maximumTreeDepth: 7, estimators: 965, learningRate: 0.0272991039790105, dropoutRate: 0.0122157953723374, l1Regularization: 0.00228384094041451, l2Reguralization: 0.0174324770524641);
        this.m_learner = new RegressionXGBoostLearner(maximumTreeDepth: 50, subSample: 0.7999999999999999, colSampleByTree: 0.99, estimators: 1000, learningRate: 0.22, colSampleByLevel: 0.7, l1Regularization: 5);
    }

    /* Call this method when calibration procedure is done */
    public bool trainOnline()
    {
        (F64Matrix samples, double[] labels, List<double[]> samplesList) = readInFileAsync();            // 1. import csv file

        if (samples.RowCount <= 0 || labels.Length <= 0)
            return false;

        // Square root transformation
        //double[] labels_transformed = transform_SQRT_ROOT(labels);
        double[] labels_transformed = labels;

        // train test split
        (ObservationTargetSet trainSet, ObservationTargetSet testSet) = splitDatasets(samples, labels_transformed);  // 2. split data into training/eval and testing

        // scale w standard scaler
       
        // Fit and transform training data
        //F64Matrix ttsO = this.scaler.FitTransform(trainSet.Observations);
        //var ttsT = trainSet.Targets;
        //ObservationTargetSet trainset_transformed = new ObservationTargetSet(ttsO, ttsT);

        ObservationTargetSet trainset_transformed = trainSet;


        // Transform test data
        //F64Matrix testset_transformed_observations = this.scaler.Transform(testSet.Observations);
        //double[] testset_labels = testSet.Targets;

        //ObservationTargetSet testSet_transformed = new ObservationTargetSet(testset_transformed_observations, testset_labels);
        ObservationTargetSet testSet_transformed = testSet;

        // train
        trainModelOffline(trainset_transformed);

        // predict
        predictOffline(testSet_transformed);

        // save model

        return true;
    }

    private double[] transform_SQRT_ROOT(double[] labels)
    {
        double[] squaredArray = labels.Select(x => Math.Sqrt(x)).ToArray();
        return squaredArray;
    }

    /* Call this function for every new sample after training to predict depth value */
    public double PredictOnline(F64Matrix testingSamples, double gtDepth)
    {

        // 1. gtDepth needs to be transformed to fit to the training data
        double label_transformed = Math.Sqrt(gtDepth);

        // 2. Create ObservationTargetSet
        ObservationTargetSet curSample = new ObservationTargetSet(testingSamples, new double[testingSamples.RowCount]);
        
        // 3. Evtl scale it.
        // double[] scaled = this.scaler.TransformSample(csamples);
        //double[] scaled = testingSamples;

        // 4. Predict on observed sample
        predictions = this.model.Predict(curSample.Observations);

        // 5. Inverse transformation of estimated depth to fit to real value
        //prediction = Math.Pow(predictions[0], 2);

        prediction = Math.Abs(Math.Log10(predictions[0]));
        prediction = Math.Abs(Math.Log10((double)prediction) * 10);
        // prediction = Math.Abs(predictions[0]) / 2;   //eher nicht
        // prediction = Math.Abs(Math.Sqrt(Math.Abs(Math.Log10(predictions[0]))));

        //UnityEngine.Debug.Log("Prediction is: " + prediction.ToString(CultureInfo.InvariantCulture));
        //UnityEngine.Debug.Log("Predictions[0] is: " + predictions[0].ToString(CultureInfo.InvariantCulture));
        //UnityEngine.Debug.Log("Prediction SQRT is: " + Math.Sqrt(prediction).ToString(CultureInfo.InvariantCulture));
        //UnityEngine.Debug.Log("GT Depth is: " + gtDepth.ToString(CultureInfo.InvariantCulture));
        //UnityEngine.Debug.Log("GT Depth transformed is: " + label_transformed.ToString(CultureInfo.InvariantCulture));

        UnityEngine.Debug.Log("Predictions is: " + prediction.ToString(CultureInfo.InvariantCulture));
        UnityEngine.Debug.Log("GT Depth: " + label_transformed.ToString(CultureInfo.InvariantCulture));

        // 6. Measure the error on current sample
        var liveRMSE = label_transformed - prediction;
        //var liveAE = Math.Abs(label_transformed - predictions[0]);
        UnityEngine.Debug.Log("Sample RMSE: " + liveRMSE);


        return prediction;

        //return predictions[0];
    }

    private void predictOffline(ObservationTargetSet ctestSet)
    {
        var testPredictions = this.model.Predict(ctestSet.Observations);

        var testMSE = metric.Error(ctestSet.Targets, testPredictions);
        var absoluteE = absoluteError.Error(ctestSet.Targets, testPredictions);

        UnityEngine.Debug.Log("Test MSE: " + testMSE);
        UnityEngine.Debug.Log("Test MAE: " + absoluteE);

    }








    private List<double[]> mat2list(F64Matrix cobservations)
    {
        List<double[]> observationsList = new List<double[]>();

        for (int i = 0; i < cobservations.RowCount; i++)
        {
            observationsList.Add(cobservations.Row(i));
        }

        return observationsList;
    }

    // OFFLINE =====================================================================================================================
    private void offlineStart()
    {

        /* OFFLINE PATH */
        (F64Matrix samples, double[] labels, List<double[]> samplesList) = readInFileAsync();                                       // 1. import csv file
        (ObservationTargetSet trainSet, ObservationTargetSet testSet) = splitDatasets(samples, labels);  // 2. split data into training/eval and testing

        // Find optimal settings
        trainModelAndOptimize(trainSet, testSet);

        //trainModelOffline(trainSet);                                                                     // 3. train model
        //predictOffline(testSet);                                                                         // 4. Predict on test set

    }

    /* XGBoost.predict */

    /* XGBoost.Fit */
    private void trainModelOffline(ObservationTargetSet curTrainSet)
    {

        this.model = this.m_learner.Learn(curTrainSet.Observations, curTrainSet.Targets);

        // predict the training and test set.
        var trainPredictions = this.model.Predict(trainSet.Observations);

        // measure the error on training and test set.
        var trainError = metric.Error(curTrainSet.Targets, trainPredictions);
        var trainAError = absoluteError.Error(curTrainSet.Targets, trainPredictions);
        UnityEngine.Debug.Log("Train MSE: " + trainError);
        UnityEngine.Debug.Log("Train MAE: " + trainAError);
        UnityEngine.Debug.Log("Target 0: " + curTrainSet.Targets[0] + " and Pred 0: " + trainPredictions[0]);
        UnityEngine.Debug.LogError("Depth calibration model trained.");


        //return;
        //Dispose();


    }

    private (ObservationTargetSet trainSet, ObservationTargetSet testSet) splitDatasets(F64Matrix csamples, double[] clabels)
    {
        // NoShuffleTrainingTestIndexSplitter - Keeps the data in the original order before splitting.
        // RandomTrainingTestIndexSplitter - Randomly shuffles the data before splitting.Usually used for regression.
        // StratifiedTrainingTestIndexSplitter - Ensures that the distribution of unique target values are similar between training and test set.Usually used for classification.

        // 85 % of the data is used for the test set. 
        var splitter = new RandomTrainingTestIndexSplitter<double>(trainingPercentage: 0.85, seed: 69);

        trainingTestSplit = splitter.SplitSet(csamples, clabels);
        trainSet = trainingTestSplit.TrainingSet;
        testSet = trainingTestSplit.TestSet;

        return (trainSet: trainSet, testSet);
    }

    private (F64Matrix samples, double[] labels, List<double[]> samplesList) readInFileAsync()
    {
        List<double> labelsList = new List<double>();
        List<double[]> samplesList = new List<double[]>();

        string fileData = System.IO.File.ReadAllText(this.pathToFile);

        //string[] lines = fileData.Split("\n"[0]);
        string[] lines = fileData.Split("\n"[0]);



        Dictionary<string, int> map = new Dictionary<string, int>();
        //map.Add("gt depth", 0);
        map.Add("WorldGPX", 0);
        map.Add("WorldGPY", 1);
        map.Add("WorldGPZ", 2);

        map.Add("WorlGORX", 3);
        map.Add("WorldGORZ", 4);

        map.Add("WorlGOLX", 5);
        map.Add("WorldGOLZ", 6);

        map.Add("WorlGRX", 7);
        map.Add("WorldGDRY", 8);
        map.Add("WorldGDRZ", 9);

        map.Add("WorlGLX", 10);
        map.Add("WorldGDLY", 11);
        map.Add("WorldGDLZ", 12);


        samples = new F64Matrix(lines.Length, 13);


        foreach (string line in lines.Skip(1))
        {
            // 3 = gt depth
            // 4 - 16 = features

            string[] lineData = (line.Trim()).Split("\t"[0]);

            if (lineData.Length > 12)
            {

                double gtdepth = Convert.ToDouble(lineData[3], CultureInfo.InvariantCulture);                       // gt depth
                //UnityEngine.Debug.Log("depth " + gtdepth);

                double worldgazex = Convert.ToDouble(lineData[4], CultureInfo.InvariantCulture);                  // world gaze point x
                double worldgazey = Convert.ToDouble(lineData[5], CultureInfo.InvariantCulture);                   // world gaze point y
                double worldgazez = Convert.ToDouble(lineData[6], CultureInfo.InvariantCulture);                   // world gaze point z

                double worldgazeoriginRX = Convert.ToDouble(lineData[7], CultureInfo.InvariantCulture);            // world gaze origin rx
                double worldgazeoriginRZ = Convert.ToDouble(lineData[8], CultureInfo.InvariantCulture);            // world gaze origin rz

                double worldgazeoriginLX = Convert.ToDouble(lineData[9], CultureInfo.InvariantCulture); ;            // world gaze origin lx
                double worldgazeoriginLZ = Convert.ToDouble(lineData[10], CultureInfo.InvariantCulture);            // world gaze origin lz

                double worldgazeDirectionRX = Convert.ToDouble(lineData[11], CultureInfo.InvariantCulture);         // world gaze direction rx
                double worldgazeDirectionRY = Convert.ToDouble(lineData[12], CultureInfo.InvariantCulture);         // world gaze direction ry
                double worldgazDirectionRZ = Convert.ToDouble(lineData[13], CultureInfo.InvariantCulture);         // world gaze direction rz

                double worldgazeDirectionLX = Convert.ToDouble(lineData[14], CultureInfo.InvariantCulture);         // world gaze direction lx
                double worldgazeDirectionLY = Convert.ToDouble(lineData[15], CultureInfo.InvariantCulture);         // world gaze direction y
                double worldgazeDirectionLZ = Convert.ToDouble(lineData[16], CultureInfo.InvariantCulture);         // world gaze direction lz

                double[] allVal;
                if (featureCount == 6)
                    allVal = new double[] {worldgazeDirectionRX, worldgazeDirectionRY, worldgazDirectionRZ, worldgazeDirectionLX, worldgazeDirectionLY, worldgazeDirectionLZ };
                else
                    allVal = new double[] { worldgazex, worldgazey, worldgazez, worldgazeoriginRX, worldgazeoriginRZ, worldgazeoriginLX, worldgazeoriginLZ, worldgazeDirectionRX, worldgazeDirectionRY, worldgazDirectionRZ, worldgazeDirectionLX, worldgazeDirectionLY, worldgazeDirectionLZ };


                // allVal = cutOffPrecision(allVal);

                samplesList.Add(allVal);
                labelsList.Add(gtdepth);

            }

        }

        if (samplesList.Count > 0 || samplesList.Count > 0)
        { 
            labels = labelsList.ToArray();
            samples = samplesList.ToF64Matrix();

            UnityEngine.Debug.Log("labels count: " + labels.Length);
            UnityEngine.Debug.Log("samples count: " + samples.RowCount);


            //UnityEngine.Debug.Log("Done");
        }
        return (samples: samples, labels: labels, samplesList: samplesList);


    }

    private double[] cutOffPrecision(double[] sample)
    {
        double[] result = new double[sample.Length];
        for (int i = 0; i < sample.Length; i++)
        {
            // TODO need to cut precision?
            //result[i] = Math.Round(sample[i], sampleCutOffPrecision); 
            result[i] = sample[i];
        }
        return result;
    }

    private void trainModelAndOptimize(ObservationTargetSet trainSet, ObservationTargetSet testSet)
    {

        // create the metric
        var metric = new MeanSquaredErrorRegressionMetric();

        // Parameter specs for the optimizer
        var parameters = new IParameterSpec[]
        {
            new MinMaxParameterSpec(min: 2, max: 100,
                transform: SharpLearning.Optimization.Transform.Linear, parameterType: ParameterType.Discrete), // max tree depth

            new MinMaxParameterSpec(min: 2, max: 1000,
                transform: SharpLearning.Optimization.Transform.Linear, parameterType: ParameterType.Discrete), // estimators

             new MinMaxParameterSpec(min: 0.001, max: 0.1,
                transform: SharpLearning.Optimization.Transform.Log10, parameterType: ParameterType.Continuous), // learning rate

                new MinMaxParameterSpec(min: 0.001, max: 0.1,
                transform: SharpLearning.Optimization.Transform.Log10, parameterType: ParameterType.Continuous), // drop out rate

             new MinMaxParameterSpec(min: 0.001, max: 0.1,
                transform: SharpLearning.Optimization.Transform.Log10, parameterType: ParameterType.Continuous), // l1 regularization

             new MinMaxParameterSpec(min: 0.001, max: 0.1,
                transform: SharpLearning.Optimization.Transform.Log10, parameterType: ParameterType.Continuous), // l2 regularization
    };

        // CountPoisson         1.9


        // Further split the training data to have a validation set to measure
        // how well the model generalizes to unseen data during the optimization.
        var validationSplit = new RandomTrainingTestIndexSplitter<double>(trainingPercentage: 0.7, seed: 24).SplitSet(trainSet.Observations, trainSet.Targets);


        // Define optimizer objective(function to minimize)
        Func<double[], OptimizerResult> minimize = p =>
        {
            // create the candidate learner using the current optimization parameters.
            var candidateLearner = new RegressionXGBoostLearner(
                                 maximumTreeDepth: (int)p[0],
                                 estimators: (int)p[1],
                                 learningRate: (double)p[2],
                                 dropoutRate: (double)p[3],
                                 l1Regularization: (double)p[4],
                                 l2Reguralization: (double)p[5]
                                 );

            var candidateModel = candidateLearner.Learn(validationSplit.TrainingSet.Observations, validationSplit.TrainingSet.Targets);

            var validationPredictions = candidateModel.Predict(validationSplit.TestSet.Observations);
            var candidateError = metric.Error(validationSplit.TestSet.Targets, validationPredictions);

            candidateModel.Dispose();
            UnityEngine.Debug.Log("Run: " + candidateError);

            return new OptimizerResult(p, candidateError);
        };


        // create optimizer
        //var optimizer = new RandomSearchOptimizer(parameters, iterations: 30, runParallel: true);
        var optimizer = new BayesianOptimizer(parameters, iterations: 30, runParallel: false);
        //var optimizer = new 

        // find best hyperparameters
        OptimizerResult result = optimizer.OptimizeBest(minimize);          // bei dieser funktion stürzt Unity direkt ab

        UnityEngine.Debug.Log("Result: " + result.Error);

        var best = result.ParameterSet;

        UnityEngine.Debug.Log("Best parameterset: ");

        for (int i = 0; i < best.Length; i++)
        {
            UnityEngine.Debug.Log("maximumTreeDepth: " + best[0]);
            UnityEngine.Debug.Log("estimators: " + best[1]);
            UnityEngine.Debug.Log("learningRate: " + best[2]);
            UnityEngine.Debug.Log("dropoutRate: " + best[3]);
            UnityEngine.Debug.Log("l1Regularization: " + best[4]);
            UnityEngine.Debug.Log("l2Regularization: " + best[5]);

        }


        UnityEngine.Debug.Log("Done");

    }

    public void Dispose()
    {
        if (model != null)
        {
            model.Dispose();
        }
    }
}
