import os
import tensorflow as tf
from tensorflow.keras.applications.resnet50 import ResNet50
from tensorflow.keras.preprocessing import image
from tensorflow.keras.applications.resnet50 import preprocess_input, decode_predictions
import numpy as np
import onnxruntime
import tf2onnx
import onnxruntime as rt

# image preprocessing
def predictImg(img_path):
    img_size = 224
    img = image.load_img(img_path, target_size=(img_size, img_size))
    x = image.img_to_array(img)
    x = np.expand_dims(x, axis=0)
    x = preprocess_input(x)

    # load keras model
    #from keras.applications.resnet50 import ResNet50
    #model = ResNet50(include_top=True, weights='imagenet')

    # convert to onnx model
    #onnx_model = keras2onnx.convert_keras(model, model.name)

    output_path = "E:\\OneDrive\\sources\\PhillipiansProxy\\python\\predictor\\model.onnx"
    # runtime prediction

    providers = ['CPUExecutionProvider']
    m = rt.InferenceSession(output_path, providers=providers)
    onnx_pred = m.run(['prediction'], {"input": x})

    for pred in onnx_pred[0][0]:
        print('%.4f' %pred)

predictImg('E:\\test\\s1.JPG')
print('-----------------')
predictImg('E:\\test\\test3.jpg')
# print(onnx_pred[0][0])
# print('ONNX Predicted:', decode_predictions(onnx_pred[0], top=3)[0])
