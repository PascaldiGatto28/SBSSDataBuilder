// Get the modal
//const modal = document.getElementById("modalBackground");

//// Get the button that opens the modal
//const closeElement = document.getElementById("openModal");

////function modelPopupStart
////{
////    modal = document.getElementById("modalBackground");
////    closeElement = document.getElementById("openModal");
////}();

//// Get the <span> element that closes the modal
////var span = document.getElementsByClassName("close")[0];

//// When the user clicks the button, open the modal 
//closeElement.onclick = function() {
//  var iframe = document.getElementById("tableContainer");
//  iframe.src = "file://D:/Users/Richard/Documents/Visual Studio 2022/Github Projects/SBSS/SBSSDataBuilder/Applications/SBSSData.Application.WebSiteLocal/DocHtml/SampleGtpTables.html";
//  modal.style.display = "block";
//}

// When the user clicks on <span> (x), close the modal
//span.onclick = function() {
//  modal.style.display = "none";
//}

// When the user clicks anywhere outside of the modal, close it
window.onclick = function(event) {
  if (event.target == modal) {
    modal.style.display = "none";
  }
}