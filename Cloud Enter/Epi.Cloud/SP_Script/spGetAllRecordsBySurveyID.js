


// Get All Recrods by Survey Id
function sample(surveyId) {
    var collection = getContext().getCollection();
    //prefix = '5c4a07f5-3868-4b8e-a526-70b1d281f26e'
    var findobject = "SELECT * FROM Objects o";
    // Query documents and take 1st item.
    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        findobject,
        function (err, feed, options) {
            if (err) throw err;

            // Check the feed and if empty, set the body to 'no docs found', 
            // else take 1st element from feed
            if (!feed || !feed.length) getContext().getResponse().setBody('no docs found');
            else getContext().getResponse().setBody(JSON.stringify(feed[0]));
        });

    if (!isAccepted) throw new Error('The query was not accepted by the server.');
}