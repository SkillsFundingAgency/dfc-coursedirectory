function UpdateRecordStatuses(ukprn, currentStatusMask, newStatus) {
    var collection = getContext().getCollection();
    var collectionLink = collection.getSelfLink();
    var response = getContext().getResponse();

    if (typeof ukprn !== 'number') throw new Error('ukprn must be a number');
    if (typeof currentStatusMask !== 'number') throw new Error('currentStatusMask must be a number');
    if (typeof newStatus !== 'number') throw new Error('newStatus must be a number');

    var updated = 0;

    tryQueryAndUpdate();

    function tryQueryAndUpdate(continuation) {
        var query = {
            query: "select * from apprenticeship c where c.RecordStatus & @currentStatusMask <> 0 and c.ProviderUKPRN = @ukprn",
            parameters: [
                { name: "@currentStatusMask", value: currentStatusMask },
                { name: "@ukprn", value: ukprn }
            ]
        };

        var requestOptions = { continuation: continuation };
        var isAccepted = collection.queryDocuments(collectionLink, query, requestOptions, function (err, documents, responseOptions) {
            for (var i = 0; i < documents.length; i++)
                tryUpdate(documents[i]);

            if (responseOptions.continuation) {
                tryQueryAndUpdate(responseOptions.continuation);
            } else {
                response.setBody({ updated: updated });
            }
        });
        if (!isAccepted) {
            throw new Error("Failed querying apprenticeships.");
        }
    }

    function tryUpdate(document) {
        var requestOptions = { etag: document._etag };

        document.ApprenticeshipLocations.forEach(l => {
            if ((l.RecordStatus & currentStatusMask) !== 0)
                l.RecordStatus = newStatus;
        });

        document.RecordStatus = document.ApprenticeshipLocations
            .map(function (location) {
                return location.RecordStatus;
            })
            .reduce(function (accumulator, currentValue) {
                return currentValue | accumulator;
            }, 0);

        var isAccepted = collection.replaceDocument(document._self, document, requestOptions, function (err, updatedDocument, responseOptions) {
            if (err) throw err;
            updated++;
        });
        if (!isAccepted) {
            throw new Error("Unable to update apprenticeship.");
        }
    }
}
