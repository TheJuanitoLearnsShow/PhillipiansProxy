console.log("content.js has been injected")

 

{ // Block used to avoid setting global variables
    const img = document.createElement('img');
    img.src = chrome.runtime.getURL('images/dummy.png');
    document.body.append(img);

    // Select the node that will be observed for mutations
    const targetNode = document.body;

    // Options for the observer (which mutations to observe)
    const config = {
        childList: true,
        subtree: true,
        attributes: true
    };

    // Callback function to execute when mutations are observed
    const callback = function (mutationsList, observer) { // Use traditional 'for loops' for IE 11
        for (const mutation of mutationsList) {
            if (mutation.type === 'childList') {
                
                for(const nodeAdded of mutation.addedNodes) {
                    if (nodeAdded.nodeName === 'IMG') {
                        //console.log('An img child node has been added. Src is ' + nodeAdded.src);
                    }
                    if (nodeAdded.nodeName === 'IFRAME') {
                        //console.log('An IFRAME child node has been added. Src is ' + nodeAdded.src);
                        nodeAdded.src = "";
                    }
                    if (nodeAdded.getElementsByTagName) {
                        const imgChildren = nodeAdded.getElementsByTagName('img')
                        for(const imgChild of imgChildren) {
                            //console.log('An img child node has been added. Src is ' + imgChild.src);
                            ReplaceImage(imgChild)
                        }
                    }
                }
            } else if (mutation.type === 'attributes') {
                //console.log('The ' + mutation.attributeName + ' attribute was modified.');
                if (mutation.attributeName === 'src') {
                    //console.log('The ' + mutation.attributeName + ' attribute was modified to ' );
                    //console.dir(mutation)
                }
            }
            const nodeTarget = mutation.target
            if (nodeTarget.getElementsByTagName) {
                const imgChildren = nodeTarget.getElementsByTagName('img')
                for(const imgChild of imgChildren) {
                    //console.log('An img child node has been added. Src is ' + imgChild.src);
                    ReplaceImage(imgChild)
                }
            }
        }
    };

    // Create an observer instance linked to the callback function
    const observer = new MutationObserver(callback);

    // Start observing the target node for configured mutations
    observer.observe(targetNode, config);

    // Later, you can stop observing
    //observer.disconnect();

    function ReplaceAllImgsInDoc () {
        Array.prototype.map.call(document.images, img => ReplaceImage(img));
    }
    ReplaceAllImgsInDoc();
    setInterval(ReplaceAllImgsInDoc, 1000 * 60 * 2);
}
