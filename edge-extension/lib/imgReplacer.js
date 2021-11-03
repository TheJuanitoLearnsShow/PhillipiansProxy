const dummyImgUrlExt = chrome.runtime.getURL('images/dummy.png')
nsfwjs.load().then((model) => {
    Window.extModelTF = model

});
function ReplaceImage(imgElem) {
    if (imgElem.src !== dummyImgUrlExt) {
        if (Window.extModelTF) { // Classify the image.
            Window.extModelTF.classify(imgElem).then((predictions) => {
                console.log("Predictions for " + imgElem.src, predictions);
                imgElem.src = dummyImgUrlExt
            });
        } else { // add them to queue??

            //imgElem.src = dummyImgUrlExt
        }
    }

}
