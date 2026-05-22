// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showPrompt(message) {
  return prompt(message, 'Type anything here');
}
export function saveUrlHistory(urlHistoryKey) {
    // 1. Get the existing log or create a new array
    let historyLog = JSON.parse(sessionStorage.getItem(urlHistoryKey)) || [];
    const currentUrl = window.location.href;

    // 2. Only log if it's different from the last entry (prevents refresh loops)
    if (historyLog[historyLog.length - 1] !== currentUrl) {
        historyLog.push(currentUrl);
        // Keep only the last 5 entries to save space
        if (historyLog.length > 5) historyLog.shift();
        sessionStorage.setItem(urlHistoryKey, JSON.stringify(historyLog));
    }

}
export function getUrlHistory(urlHistoryKey) {
    let historyLog = JSON.parse(sessionStorage.getItem(urlHistoryKey)) || [];
    return historyLog[historyLog.length - 2] ?? "";
}
export function dispose() {

}