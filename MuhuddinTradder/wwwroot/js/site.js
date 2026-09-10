// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Har page par jahan bhi calendar (date input) ho, agar uski value khali ho
// to usay automatically current date pe set kar do (existing/saved date ko
// kabhi overwrite nahi karta - sirf khali fields fill hoti hain).
document.addEventListener('DOMContentLoaded', function () {
    var today = new Date();
    var yyyy = today.getFullYear();
    var mm = String(today.getMonth() + 1).padStart(2, '0');
    var dd = String(today.getDate()).padStart(2, '0');
    var todayStr = yyyy + '-' + mm + '-' + dd;

    document.querySelectorAll('input[type="date"]').forEach(function (el) {
        if (!el.value) {
            el.value = todayStr;
        }
    });
});
