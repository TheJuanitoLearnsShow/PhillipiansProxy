using Microsoft.ML;
using System;
using System.Drawing;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace TensorModel
{
    class Program
    {
        private static readonly string _mlnetModelFilePath = "nsfw_net.zip";
        private static PredictionEngine<ImageInputData, ImageLabelPredictions> _predictionEnginePool;

        public static void ClassifyImage(string imageFilePath)
        {
            using (var fs = File.OpenRead(imageFilePath))
            {

                //Convert to Bitmap
                Bitmap bitmapImage = (Bitmap)Image.FromStream(fs);

                //Set the specific image data into the ImageInputData type used in the DataView
                ImageInputData imageInputData = new ImageInputData { Image = bitmapImage };

                //Predict code for provided image
                ImageLabelPredictions imageLabelPredictions = _predictionEnginePool.Predict(imageInputData);
                //labels 
                // 0  -> Block > 0.70
                // 1   -> Block > 0.70
                // 2 
                // 3  -> Block > 0.70
                // 4 Drawing
                //Predict the image's label (The one with highest probability)
                //ImagePredictedLabelWithProbability imageBestLabelPrediction
                //                    = FindBestLabelWithProbability(imageLabelPredictions, imageInputData);
                Console.WriteLine(imageLabelPredictions);


            }
        }

        
        static void Main(string[] args)
        {
            var _tensorFlowModelFilePath = @"E:\test\mobilenet_v2_140_224\frozen_graph.pb";
            var imgFile = @"E:\test\s1.JPG";
            //var imgFile = @"E:\test\th2.jpg";
            /////////////////////////////////////////////////////////////////
            //Configure the ML.NET model for the pre-trained TensorFlow model
            //ensorFlowModelConfigurator tensorFlowModelConfigurator = new TensorFlowModelConfigurator(_tensorFlowModelFilePath);

            // Save the ML.NET model .zip file based on the TensorFlow model and related configuration
            //_predictionEnginePool = tensorFlowModelConfigurator.SaveMLNetModel(_mlnetModelFilePath);

            //ClassifyImage(@"E:\test\th2.jpg");

            var m = new TensorFlowNet();
            m.Run(imgFile);
            Console.ReadLine();
        }
    }
}
