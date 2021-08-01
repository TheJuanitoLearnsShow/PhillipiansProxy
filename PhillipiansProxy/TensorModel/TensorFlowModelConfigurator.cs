using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TensorModel
{
    public class ImageInputData
    {
        [ImageType(224, 224)]
        public Bitmap Image { get; set; }
    }
    public class ImageLabelPredictions
    {
        //TODO: Change to fixed output column name for TensorFlow model
        [ColumnName("sequential/prediction/Softmax")]
        public float[] PredictedLabels;
    }
    public class ImagePredictedLabelWithProbability
    {
        public string ImageId;

        public string PredictedLabel;
        public float Probability { get; set; }

        public long PredictionExecutionTime;
    }
    public class TensorFlowModelConfigurator
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _mlModel;

        public TensorFlowModelConfigurator(string tensorFlowModelFilePath)
        {
            _mlContext = new MLContext();

            // Model creation and pipeline definition for images needs to run just once, so calling it from the constructor:
            _mlModel = SetupMlnetModel(tensorFlowModelFilePath);
        }

        public struct ImageSettings
        {
            public const int imageHeight = 224;
            public const int imageWidth = 224;
            public const float mean = 117;         //offsetImage
            public const bool channelsLast = true; //interleavePixelColors
        }

        // For checking tensor names, you can open the TF model .pb file with tools like Netron: https://github.com/lutzroeder/netron
        public struct TensorFlowModelSettings
        {
            // input tensor name
            public const string inputTensorName = "self";

            // output tensor name
            public const string outputTensorName = "sequential/prediction/Softmax";
        }

        private ITransformer SetupMlnetModel(string tensorFlowModelFilePath)
        {
            var pipeline = _mlContext.Transforms.ResizeImages(outputColumnName: TensorFlowModelSettings.inputTensorName, imageWidth: ImageSettings.imageWidth, imageHeight: ImageSettings.imageHeight, inputColumnName: nameof(ImageInputData.Image))
                .Append(_mlContext.Transforms.ExtractPixels(outputColumnName: TensorFlowModelSettings.inputTensorName, interleavePixelColors: ImageSettings.channelsLast, offsetImage: ImageSettings.mean))
                .Append(_mlContext.Model.LoadTensorFlowModel(tensorFlowModelFilePath).
                ScoreTensorFlowModel(outputColumnNames: new[] { TensorFlowModelSettings.outputTensorName },
                                    inputColumnNames: new[] { TensorFlowModelSettings.inputTensorName }, addBatchDimensionInput: false));

            ITransformer mlModel = pipeline.Fit(CreateEmptyDataView());

            return mlModel;
        }
        private IDataView CreateEmptyDataView()
        {
            //Create empty DataView ot Images. We just need the schema to call fit()
            List<ImageInputData> list = new List<ImageInputData>();
            list.Add(new ImageInputData() { Image = new System.Drawing.Bitmap(ImageSettings.imageWidth, ImageSettings.imageHeight) }); //Test: Might not need to create the Bitmap.. = null; ?
            IEnumerable<ImageInputData> enumerableData = list;

            var dv = _mlContext.Data.LoadFromEnumerable<ImageInputData>(list);
            return dv;
        }

        public PredictionEngine<ImageInputData, ImageLabelPredictions> SaveMLNetModel(string mlnetModelFilePath)
        {
            // Save/persist the model to a .ZIP file to be loaded by the PredictionEnginePool
            _mlContext.Model.Save(_mlModel, null, mlnetModelFilePath);
            var predictor = _mlContext.Model.CreatePredictionEngine<ImageInputData, ImageLabelPredictions>(_mlModel);
            return predictor;
        }
    }
}
