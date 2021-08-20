const tf = require('@tensorflow/tfjs-node')
const nsfw = require('nsfwjs')
const fs = require('fs');
const path = require('path');
const jpeg = require('jpeg-js')

const ml_model = 'file://./model/' //path.join(__dirname, '../model/')

let _model = null

async function load_model() {
    if (_model == null) {
        _model = await nsfw.load(ml_model)
    }
}


async function convert(img) {
  // Decoded image in UInt8 Byte array
  const image = await jpeg.decode(img, true)

  const numChannels = 3
  const numPixels = image.width * image.height
  const values = new Int32Array(numPixels * numChannels)

  for (let i = 0; i < numPixels; i++)
    for (let c = 0; c < numChannels; ++c)
      values[i * numChannels + c] = image.data[i * 4 + c]

  return tf.tensor3d(values, [image.height, image.width, numChannels], 'int32')
}

async function predictFromFileContents(context, buffer) {
    const image = await convert(buffer)
    const predictions = await _model.classify(image)
    image.dispose()
    context.log(predictions)
    return predictions
  }

module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');
    await load_model();

    var body = req.body;

    context.log("TestResult:", {
        isBuffer: Buffer.isBuffer(body),
        length: body.length
    });

    const predictions = await predictFromFileContents(context, context.req.body)

    //context.log(context.req);
    //fs.writeFileSync(path.join(__dirname, "../test.png"), context.req.body);
    //context.log('****************** end');

    context.res = {
        body: predictions 
    };

    context.done();

}