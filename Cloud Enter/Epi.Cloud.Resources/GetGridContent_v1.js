function getGridContent(query, continuationToken, skip) {
    var context = getContext();
    var response = context.getResponse();
    var collection = context.getCollection();
    var collectionLink = collection.getSelfLink();
    var nodesBatch = [];
    var nextContinuationToken;
    var currentContinuationToken;
    var responseSize = 0;
    var isMaxSizeReached = false;

    getNodes(continuationToken, skip);

    // continuationToken identifies the continuation point.
    // skip is then number of documents that were already returned from the continuation point.
    function getNodes(continuationToken, skip) {
        // Tune the pageSize to fit your dataset.
        var requestOptions =
        {
            continuation: continuationToken,
            pageSize: 1000
        };

        // The number of documents taken from the current continuation block
        var taken = 0;

        var accepted = collection.queryDocuments(collectionLink, query, requestOptions,
          function (err, documentsRead, responseOptions) {
              currentContinuationToken = requestOptions.continuation;
              for(var thisResponse of documentsRead)
              {
                  if (skip > 0) {
                      skip -= 1;
                  }
                  else {
                      // The size of the current query response page.
                      var thisResponseSize = JSON.stringify(thisResponse).length;

                      // DocumentDB has a response size limit of 1 MB.
                      if (responseSize + thisResponseSize < 1024 * 1024) {
                          // Append response to nodesBatch.
                          nodesBatch = nodesBatch.concat(thisResponse);

                          // Keep track of the total response size.
                          responseSize += thisResponseSize;
                          taken += 1;
                      }
                      else {
                          isMaxSizeReached = true;
                          break;
                      }
                  }
              }

              if (!isMaxSizeReached && responseOptions.continuation) {
                  // If max response size has not been reached and there is a continuation token... 
                  // Run the query again to get the next page of results
                  nextContinuationToken = responseOptions.continuation;
                  getNodes(responseOptions.continuation);
              }
              else if (isMaxSizeReached) {
                  // If the response size limit reached; run the script again with the nextContinuationToken as a script parameter.
                  response.setBody({
                      "message": "Response size limit reached.",
                      "continuationToken": currentContinuationToken,
                      "result": nodesBatch,
                      "skip": taken
                  });
              }
              else {
                  // If there is no continutation token, we are done. Return the response.
                  response.setBody({ result: nodesBatch });
              }
          });

        if (!accepted) {
            // If the execution limit reached; run the script again with the nextContinuationToken as a script parameter.
            response.setBody({
                "message": "Execution limit reached.",
                "continuationToken": nextContinuationToken,
                "result": nodesBatch,
                "skip": taken
            });
        }
    }
}
