const isBackground = true;
console.log('Background', isBackground);

chrome.runtime.onMessage.addListener(
    function (request, sender, sendResponse)
    {
        if (request.type == 'fetchUrl') {
            var url = request.url;
            console.log('background fetchUrl', url);
            try {

                fetch(url)
                    .then(httpResponse =>
                    {
                        // console.log('background fetchUrl response', response);
                        if (httpResponse.ok) {
                            return httpResponse.text();
                        } else {
                            throw { 'ok': false, 'status': httpResponse.status, 'statusText': httpResponse.statusText };
                        }
                    })
                    .then(text =>
                    {
                        // console.log('background fetchUrl text', text);
                        return sendResponse({ 'ok': true, 'data': text });
                    })
                    .catch(ex =>
                    {
                        // console.log('background fetchUrl ex', ex);
                        return sendResponse({ 'ok': false, 'status': ex.status, 'statusText': ex.statusText });
                    }
                    );

            } catch (ex) {
                // console.log('background fetchUrl catch', ex);
                return sendResponse({ 'ok': false, 'status': ex.status, 'statusText': ex.statusText });
            }
        }
        return true;
    }
);