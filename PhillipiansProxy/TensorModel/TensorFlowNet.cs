using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Tensorflow;
using Tensorflow.Keras.Utils;
using Tensorflow.NumPy;
using static Tensorflow.Binding;

namespace TensorModel
{
    public struct TensorFlowModelSettings
    {
        // input tensor name
        public const string inputTensorName = "self";

        // output tensor name
        public const string outputTensorName = "sequential/prediction/Softmax";
    }
    class TensorFlowNet
    {

        string pbFile = @"E:\test\mobilenet_v2_140_224\frozen_graph.pb";
        string labelFile = @"E:\test\mobilenet_v2_140_224\class_labels.txt";
        public IEnumerable<string> Run(string img_file_name)
        {
            tf.compat.v1.disable_eager_execution();

            var graph = new Graph();
            //import GraphDef from pb file
            graph.Import(pbFile);

            var input_name = TensorFlowModelSettings.inputTensorName;
            var output_name = TensorFlowModelSettings.outputTensorName;

            var input_operation = graph.OperationByName(input_name);
            var output_operation = graph.OperationByName(output_name);

            var labels = File.ReadAllLines(labelFile);
            var result_labels = new List<string>();
            var sw = new Stopwatch();

            var nd = ReadTensorFromImageFile(img_file_name);
            using (var sess = tf.Session(graph))
            {
                //    foreach (var nd in file_ndarrays)
                //    {
                    sw.Restart();

                    var results = sess.run(output_operation.outputs[0], (input_operation.outputs[0], nd));
                    var resultsSqueezed = np.squeeze(results);
                    //int idx = np.argmax(resultsSqueezed);
                    for (int idx = 0; idx < resultsSqueezed.Count(); idx++)
                    {
                        Console.WriteLine($"{labels[idx]} {resultsSqueezed[idx]}", Color.Tan);
                }
                Console.WriteLine($" in {sw.ElapsedMilliseconds}ms", Color.Tan);
                //result_labels.Add(labels[idx]);
                //    }
            }

            return result_labels;
        }

        private NDArray ReadTensorFromImageFile(string file_name,
                                int input_height = 224,
                                int input_width = 224,
                                int input_mean = 117,
                                int input_std = 1)
        {
            var graph = tf.Graph().as_default();

            var file_reader = tf.io.read_file(file_name, "file_reader");
            var decodeJpeg = tf.image.decode_jpeg(file_reader, channels: 3, name: "DecodeJpeg");
            var cast = tf.cast(decodeJpeg, tf.float32);
            var dims_expander = tf.expand_dims(cast, 0);
            var resize = tf.constant(new int[] { input_height, input_width });
            var bilinear = tf.image.resize_bilinear(dims_expander, resize, true);
            var sub = tf.subtract(bilinear, new float[] { input_mean });
            var normalized = tf.divide(sub, new float[] { input_std });

            using (var sess = tf.Session(graph))
                return sess.run(normalized);
        }

    }
}
