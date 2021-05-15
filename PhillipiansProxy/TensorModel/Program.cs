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

                //Predict the image's label (The one with highest probability)
                //ImagePredictedLabelWithProbability imageBestLabelPrediction
                //                    = FindBestLabelWithProbability(imageLabelPredictions, imageInputData);
                Console.WriteLine(imageLabelPredictions);


            }
        }

        static void Main(string[] args)
        {
            var _tensorFlowModelFilePath = @"nsfw.299x299.pb";
            /////////////////////////////////////////////////////////////////
            //Configure the ML.NET model for the pre-trained TensorFlow model
            TensorFlowModelConfigurator tensorFlowModelConfigurator = new TensorFlowModelConfigurator(_tensorFlowModelFilePath);

            // Save the ML.NET model .zip file based on the TensorFlow model and related configuration
            _predictionEnginePool = tensorFlowModelConfigurator.SaveMLNetModel(_mlnetModelFilePath);

            ClassifyImage(@"test.jpg");
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
