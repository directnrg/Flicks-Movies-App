// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Auto-dismiss after 5 seconds
setTimeout(function () {
    closeBanner();
}, 5000);

// Close banner function
function closeBanner() {
    var banner = document.getElementById("successBanner");
    if (banner) {
        banner.style.display = "none";  // Hide the banner
    }
}