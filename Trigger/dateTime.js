function DateTimeAdd() {

   //Pre-trigger
   var context = getContext();
   var request = context.getRequest();

   var createdItem = request.getBody();

   createdItem["DateTime"] = new Date().toLocaleString();

   request.setBody(createdItem);

}