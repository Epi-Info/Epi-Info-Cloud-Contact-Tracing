function getGridContent(query, sortKey, isSortAscending, continuationToken, skip) {
    var context = getContext();
    var response = context.getResponse();
    var collection = context.getCollection();
    var collectionLink = collection.getSelfLink();
    var responses = [];
    var nextContinuationToken;
    var currentContinuationToken;
    var responseSize = 0;
    var isMaxSizeReached = false;

    if (!sortKey) {
        sortkey = "_ts";
        isSortAscending = false;
    }

    var trace = "query:" + query + ", continuationToken:" + continuationToken + ", skip:" + skip + ", sortKey:" + sortKey + ", isSortAscending:" + isSortAscending;

    getNodes(continuationToken, skip);

    // continuationToken identifies the continuation point.
    // skip is then number of documents that were already returned from the continuation point.
    function getNodes(continuationToken, skip) {
        // trace = trace.concat(", getNodes");
        // Tune the pageSize to fit your dataset.
        var requestOptions =
        {
            continuation: continuationToken,
            pageSize: 100
        };

        // The number of documents taken from the current continuation block
        var taken = 0;

        var accepted = collection.queryDocuments(collectionLink, query, requestOptions,
          function (err, documentsRead, responseOptions) {
              trace = trace.concat(",queryDocments");
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
                          // Append response to responses.
                          responses = responses.concat(thisResponse);

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
              trace = trace.concat(",taken:" + taken);

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
                      "trace": trace,
                      "continuationToken": currentContinuationToken,
                      "result": responses,
                      "skip": taken
                  });
              }
              else {
                  // If there is no continutation token, we are done. Return the response.
                  responses = schwartzianSort(responses, sortKey);
                  response.setBody({
                      "message": "Completed",
                      "trace": trace,
                      "sortKey": sortKey,
                      "result": responses
                  });
              }
          });

        if (!accepted) {
            // If the execution limit reached; run the script again with the nextContinuationToken as a script parameter.
            response.setBody({
                "message": "Execution limit reached.",
                "continuationToken": nextContinuationToken,
                "result": responses,
                "skip": taken
            });
        }
    }

    var schwartzianSort = (function () {
        var decorate = function (sortKey) {
            return function (item) {
                switch (sortKey) {
                    case "_ts":
                        return [item["_ts"](), item];
                    case "_UserEmail":
                        return [item["UserName"](), item];
                    case "IsDraftMode":
                    case "_Mode":
                        return [item.IsDraftMode, item];
                    case "_DateCreated":
                        return [item.FirstSaveTime, item];
                    case "_DateUpdated":
                        return [item.LastSaveTime, item];
                    default:
                        return [item.ResponseQA[sortKey] ? item.ResponseQA[sortKey].toUpperCase() : "", item];
                }
            };
        };

        var compare = function (sortFunction) {
            sortFunction = sortFunction || defaultSortFunction;

            return function (a, b) {
                return sortFunction(a[0], b[0]);
            };
        };

        var defaultSortFunction = function (a, b) {
            if (isSortAscending) {
                if (a < b) return -1;
                if (a > b) return 1;
            }
            else {
                if (a < b) return 1;
                if (a > b) return -1;
            }
            return 0;
        };

        var undecorate = function (item) {
            return item[1];
        };

        return function (items, sortKey, sortFunction) {
            return items.map(decorate(sortKey))
						.sort(compare(sortFunction))
						.map(undecorate);
        };
    })();
}
