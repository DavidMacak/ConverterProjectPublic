document.addEventListener("DOMContentLoaded", function () {
    var dropZone = document.getElementById("dropZone");
    var fileInput = document.getElementById("filesToUpload");
    var fileList = document.getElementById("uploadedFiles");

    dropZone.addEventListener("dragover", function (e) {
        e.preventDefault();
        dropZone.classList.add("drag-over");
    });

    dropZone.addEventListener("dragleave", function (e) {
        e.preventDefault();
        dropZone.classList.remove("drag-over");
    });

    dropZone.addEventListener("drop", function (e) {
        e.preventDefault();
        dropZone.classList.remove("drag-over");

        var files = e.dataTransfer.files;
        if (files.length > 0) {
            fileInput.files = files;
            updateFileList();
        }
    });

    fileInput.addEventListener("change", function () {
        updateFileList();
    });

    function updateFileList() {
        fileList.innerHTML = "";
        var files = fileInput.files;
        for (var i = 0; i < files.length; i++) {
            var listItem = document.createElement("li");
            listItem.textContent = files[i].name;
            fileList.appendChild(listItem);
        }
    }
});