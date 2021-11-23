
const tf = require('@tensorflow/tfjs-node')
const nsfw = require('nsfwjs')
const fs = require('fs')
const jpeg = require('jpeg-js')
// const { consoleLogToFile } = require("console-log-to-file/dist/index.cjs.js")

// consoleLogToFile({
//   logFilePath: "./default.log",
// });

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

export async function predictFromFileContents(buffer, writer) {
  console.log("going to predict")
  const image = await convert(buffer)
  const predictions = await _model.classify(image)
  image.dispose()
  console.log(predictions)
  writer?.write(predictions)
}

const filename = 'E:\\test\\s1.JPG'
const filename2 = 'E:\\test\\th2.JPG'

load_model().then(() => {

  fs.readFile( filename, (err, imgContents) => {

    predictFromFileContents(imgContents, null)
    fs.readFile( filename2, (err, imgContents2) => {

      predictFromFileContents(imgContents2, null)
      console.log("Done with first two tests")
    })
  })
  
  // const readline = require('readline');
  
  // console.log(process.argv[2])
  
  // console.log(process.argv[3])

  // const reader = fs.createReadStream(null, {fd: process.argv[2]});
  // const writer = fs.createWriteStream(null, {fd: process.argv[3]});

  // reader.on('data', data => predictFromFileContents( data, writer));

  // setInterval(()=> {}, 1000 * 60 * 60);
}
)
