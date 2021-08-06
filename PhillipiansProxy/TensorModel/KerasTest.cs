using Keras.Datasets;
using Keras.Layers;
using Keras.Models;
using Keras.Utils;
using Numpy;
using System;
using System.IO;
using System.Linq;

namespace TensorModel
{
    class KerasTest
    {
        object load_model(string model_path)
        {
           
            var model = tf.keras.models.load_model(model_path, custom_objects ={
                        'KerasLayer': hub.KerasLayer
            return model
        }
    
}
}
