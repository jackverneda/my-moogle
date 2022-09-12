window.SetElementFocus = function (element) {
    element.focus();
    return element === true;
}

window.SetFocusByElementId = function (elementId) {
    var element = document.getElementById(elementId);
    element.focus();
    return element === true;
}