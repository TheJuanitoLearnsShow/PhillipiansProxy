
const tf = require('@tensorflow/tfjs-node')
const nsfw = require('nsfwjs')
const fs = require('fs')
const jpeg = require('jpeg-js')


const ml_model = 'file://./model/'
//const model = await nsfw.load() // To load a local model, nsfw.load('file://./path/to/model/')

let _model
const load_model = async () => {
  _model = await nsfw.load(ml_model)
}


const convert = async (img) => {
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

// async function fn() {
//   const pic = await axios.get('url to pic', {
//     responseType: 'arraybuffer',
//   })
//   // Image must be in tf.tensor3d format
//   // you can convert image to tf.tensor3d with tf.node.decodeImage(Uint8Array,channels)
//   const image = await tf.node.decodeImage(pic.data,3)
//   const predictions = await _model.classify(image)
//   image.dispose() // Tensor memory must be managed explicitly (it is not sufficient to let a tf.Tensor go out of scope for its memory to be released).
//   console.log(predictions)
// }

export async function predictFromFileContents(buffer) {
  const image = await convert(buffer)
  const predictions = await _model.classify(image)
  image.dispose()
  console.log(predictions)
}

const filename = 'E:\\test\\DSC01731.JPG'

load_model().then(() => 
  fs.readFile( filename, (err, imgContents) => {

    predictFromFileContents(imgContents)
  })
)