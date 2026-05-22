// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function saveUrl() {
    // 1. Get the existing log or create a new array
    let historyLog = JSON.parse(sessionStorage.getItem(_urlHistorysessionKey)) || [];
    const currentUrl = window.location.href;

    // 2. Only log if it's different from the last entry (prevents refresh loops)
    if (historyLog[historyLog.length - 1] !== currentUrl) {
        historyLog.push(currentUrl);
        // Keep only the last 5 entries to save space
        if (historyLog.length > 5) historyLog.shift();
        sessionStorage.setItem(_urlHistorysessionKey, JSON.stringify(historyLog));
    }
}

$(document).ready(function(){
    console.log('your page is ready!');
    saveUrl();
})