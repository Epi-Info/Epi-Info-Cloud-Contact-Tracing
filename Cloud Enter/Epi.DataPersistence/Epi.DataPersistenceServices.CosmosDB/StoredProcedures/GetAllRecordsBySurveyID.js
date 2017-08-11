function orderBy(filterQuery) {
    // HTTP error codes sent to our callback funciton by CosmosDB server.
    var ErrorCode = {
        REQUEST_ENTITY_TOO_LARGE: 413,
    }

    var collection = getContext().getCollection();
    var collectionLink = collection.getSelfLink();
    var result = new Array();
    tryQuery({});

    function tryQuery(options) {
        var isAccepted = (filterQuery && filterQuery.length) ?
            collection.queryDocuments(collectionLink, filterQuery, options, callback) :
            collection.readDocuments(collectionLink, options, callback)

        if (!isAccepted) throw new Error("Source dataset is too large to complete the operation.");
    }

    /**
    * queryDocuments callback.
    * @param {Error} err - Error object in case of error/exception.
    * @param {Array} queryFeed - array containing results of the query.
    * @param {ResponseOptions} responseOptions.
    */
    function callback(err, queryFeed, responseOptions) {
        if (err) {
            throw err;
        }

        // Iterate over document feed and store documents into the result array.
        queryFeed.forEach(function (element, index, array) {
            result[result.length] = element;
        });

        fillResponse();
    }

    // Compare two objects(documents) using field specified by the orderByFieldName parameter.


    // This is called in the very end on an already sorted array.
    // Sort the results and set the response body.
    function fillResponse() {
        // Main script is called with continuationToken which is the index of 1st item to start result batch from.       
        // Get/initialize the response.
        var response = getContext().getResponse();
        response.setBody(null);

        // Take care of response body getting too large:
        // Set Response iterating by one element. When we fail due to MAX response size, return to the client requesting continuation.
        var i = 0;
        // Finally, set response body.
        response.setBody({ result: result });
    }
}
