function ArchiveApprenticeshipsForProvider(ukprn) {
    var archivedStatus = 4;

    var collection = getContext().getCollection();
    var collectionLink = collection.getSelfLink();
    var response = getContext().getResponse();

    if (typeof ukprn !== 'number') throw new Error('UKPRN must be a number');

    var updated = 0;

    tryQueryAndUpdate();

    function tryQueryAndUpdate(continuation) {
        var query = {
            query: "select * from apprenticeship c where c.RecordStatus <> @archivedStatus and c.ProviderUKPRN = @ukprn",
            parameters: [
                { name: "@archivedStatus", value: archivedStatus },
                { name: "@ukprn", value: ukprn }
            ]
        };

        var requestOptions = { continuation: continuation };
        var isAccepted = collection.queryDocuments(collectionLink, query, requestOptions, function (err, documents, responseOptions) {
            if (documents.length > 0) {
                for (var i = 0; i < documents.length; i++)
                    tryUpdate(documents[i]);
                tryQueryAndUpdate(responseOptions.continuation);
            } else if (responseOptions.continuation) {
                tryQueryAndUpdate(responseOptions.continuation);
            } else {
                response.setBody({ updated: updated });
            }
        });
        if (!isAccepted) {
            throw new Error("The stored procedure timed out.");
        }
    }
    
    function tryUpdate(document) {
        var requestOptions = { etag: document._etag };

        document.RecordStatus = archivedStatus;
        document.ApprenticeshipLocations.forEach(l => l.RecordStatus = archivedStatus);

        var isAccepted = collection.replaceDocument(document._self, document, requestOptions, function (err, updatedDocument, responseOptions) {
            if (err) throw err;
            updated++;
        });
        if (!isAccepted) {
            throw new Error("The stored procedure timed out.");
        }
    }
}