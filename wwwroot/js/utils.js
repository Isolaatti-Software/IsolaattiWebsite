function getClockFormatFromSeconds(secs){
    let truncatedSecs = Math.round(secs);
    let minutes = truncatedSecs / 60;
    let seconds = truncatedSecs % 60;
    if (seconds < 10) {
        seconds = `0${seconds}`
    }
    return `${Math.trunc(minutes)}:${seconds}`;
}