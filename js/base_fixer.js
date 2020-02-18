let realBase = `${window.location.protocol}//${window.location.host}/`;
let baseTag = document.querySelector('base');
if (baseTag) {
    baseTag.href = realBase;
} else {
    baseTag = document.createElement('base');
    baseTag.href = realBase;
    document.getElementsByTagName('head')[0].appendChild(baseTag);
}
