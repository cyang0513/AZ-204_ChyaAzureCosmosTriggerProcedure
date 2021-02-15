function createMetaData(items) {

   if (!items) throw new Error("The array is undefined or null.");

   var collection = getContext().getCollection();
   var collectionLink = collection.getSelfLink();

   var options = { disableAutomaticIdGeneration: true };

   var isAccepted = collection.createDocument(collectionLink, items[0], options, callback);

   if (isAccepted) getContext().getResponse().setBody("Created!");

   //First parameter is the error
   function callback(err, item, options) {
      if (err) throw err;
   }

}
