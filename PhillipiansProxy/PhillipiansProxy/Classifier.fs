namespace PhillipiansProxy

open System;
open System.Collections.Generic;
open System.Drawing;
open System.Numerics

//[<CLIMutable>]
//type ImageInputData = {
//    //[<ImageType(224, 224)>]
//    //Image: Bitmap 
//}

type PredictionHelper = {
    S: single
    P: single
    H: single
}

[<CLIMutable>]
type ImageLabelPredictions = {
    //[<ColumnName("sequential/prediction/Softmax")>]
    PredictedLabels: single[]
}
with
    member x.ToHelper() =
        //let arr = x.PredictedLabels.GetValues()
        let arr = x.PredictedLabels
        {
            S = arr.[0]
            P = arr.[1]
            H = arr.[3]
        }
    override x.ToString() =
        //let arr = x.PredictedLabels.GetValues()
        let arr = x.PredictedLabels
        String.Format( "S {0} P {1} N {2} H {2} D {2}" ,  arr.[0],  arr.[1], arr.[2], arr.[3], arr.[4])





//type Classifier(modelPath: string) =
//    // Load trained model
//    let mlContext = new MLContext()
//    let modelSchema:DataViewSchema = null
//    let trainedModel = mlContext.Model.Load(modelPath, ref modelSchema);
    
//    let predictor = mlContext.Model.CreatePredictionEngine<ImageInputData, ImageLabelPredictions>(trainedModel);
    

