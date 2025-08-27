// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', () => {
    const input = document.getElementById("fileInput");
    const form = document.getElementById("fileForm");
    RegisterFileFormEvent(form);
    if (input)
    {
        input.addEventListener("change", () =>
        {
            if (validateFileInput(input))
            {
                const ev = new Event('submit', { bubbles: true, cancelable: true });
                form.dispatchEvent(ev);
            }
        });
    }
});
function validateFileInput(uploadedFile)
{
    const file = uploadedFile.files[0];
    const allowedExtensions = uploadedFile.dataset.allowedExt || ".csv";

    if (!file) return false;

    if (file.size == 0)
    {
        alert("File is empty!");
        return false;
    }

    if (!file.name.endsWith(allowedExtensions))
    {
        alert("Only .csv files are allowed.");
        uploadedFile.value = "";
        return false;
    }
    return true;
}
function RegisterFileFormEvent(fileForm)
{
    fileForm.addEventListener("submit", function (event)
    {
        event.preventDefault();
        const formData = new FormData(fileForm);
        fetch("/Task/ReadFile", {
            method: "POST",
            body: formData
        })
        .then(response => { return response.json(); })
        .then(data =>
        {
            GenerateTable(data);
        }).catch(error =>
        {
            alert("Processing failed:", error);
        });
    });
}

function GenerateTable(data)
{
    const container = document.getElementById("pairsTable");
    const table = document.createElement("table");

    container.innerHTML = '';
    table.classList.add("table", "table-bordered", "table-striped");

    const headerRow = table.insertRow();
    const headers = ["EmployeeID#1", "EmployeeID#2", "ProjectID", "Days Worked"];
    headers.forEach(headerText =>
    {
        const headerCell = document.createElement("th");
        headerCell.textContent = headerText;
        headerRow.appendChild(headerCell);
    });

    data.forEach(pair =>
    {
        const row = table.insertRow();
        Object.values(pair).forEach(classVar =>
        {
            const td = row.insertCell();
            td.textContent = classVar;
        })

    })
    container.appendChild(table);
}
