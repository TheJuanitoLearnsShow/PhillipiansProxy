namespace PhillipiansProxy

open Microsoft.ML
open Microsoft.ML.Transforms.Image;
open System;
open System.Collections.Generic;
open System.Drawing;
open Microsoft.ML.Data
open System.Numerics

[<CLIMutable>]
type ImageInputData = {
    [<ImageType(224, 224)>]
    Image: Bitmap 
}

[<CLIMutable>]
type ImageLabelPredictions = {
    [<ColumnName("sequential/prediction/Softmax")>]
    PredictedLabels: Microsoft.ML.Data.VBuffer<single>
}

type Classifier(modelPath: string) =
    ////Define DataViewSchema for data preparation pipeline and trained model
    //DataViewSchema modelSchema;
    
    // Load trained model
    let mlContext = new MLContext()
    let modelSchema:DataViewSchema = null
    let trainedModel = mlContext.Model.Load(modelPath, ref modelSchema);
    
    let predictor = mlContext.Model.CreatePredictionEngine<ImageInputData, ImageLabelPredictions>(trainedModel);
    

