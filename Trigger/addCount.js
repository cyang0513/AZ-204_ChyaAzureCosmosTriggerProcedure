function AddCount() {
   
   var context = getContext();
   var container = context.getCollection();
   var response = context.getResponse();

   var createdItem = response.getBody();
   var filterQuery = 'SELECT * FROM TestContainer r WHERE r.id = "_metadata"';
   var accept = container.queryDocuments(container.getSelfLink(), filterQuery, updateMetadataCallback);
   if (!accept) throw "Unable to find _metadata item";

   function updateMetadataCallback(err, items, responseOptions) {
      if (err) throw new Error("Error" + err.message);

      var metadataItem = items[0];

      // update metadata, this will only apply to meta data item with same partition key as operated item
      metadataItem.AddCount += 1;

      var accept = container.replaceDocument(metadataItem._self,
         metadataItem,
         function(err, itemReplaced) {
            if (err) throw new Error("Error" + err.message);
         });

      if (!accept) throw "Unable to update _metadata";
      return;
   }
}