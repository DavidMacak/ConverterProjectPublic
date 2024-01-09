var dropZone = document.getElementById('dropZone');
var fileInput = document.getElementById('filesToUpload');

function showDropZone() {
    dropZone.style.visibility = "visible";
}
function hideDropZone() {
    dropZone.style.visibility = "hidden";
}

function allowDrag(e) {
    if (true) {  // Test that the item being dragged is a valid one
        e.dataTransfer.dropEffect = 'copy';
        e.preventDefault();
    }
}

function handleDrop(e) {
    e.preventDefault();
    hideDropZone();

    var files = e.dataTransfer.files;
    if (files.length > 0) {
        fileInput.files = files;
    }
}

// 1
window.addEventListener('dragenter', function (e) {
    showDropZone();
});

// 2
dropZone.addEventListener('dragenter', allowDrag);
dropZone.addEventListener('dragover', allowDrag);

// 3
dropZone.addEventListener('dragleave', function (e) {
    hideDropZone();
});

// 4
dropZone.addEventListener('drop', handleDrop);