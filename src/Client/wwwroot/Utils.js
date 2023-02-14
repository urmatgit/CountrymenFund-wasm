function ShowAlert(text) {
    alert(text);
}
function ShowElement(id,showHide) {
    var element = document.getElementById(id);
    if (element !== null) {
        console.log(showHide);
        if (showHide) {
            
            element.style.display = "block";
        } else {
            element.style.display = "none";
        }
    }
}