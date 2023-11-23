// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('.priority-label').each(function () {
        var priority = $(this).data('priority');
        switch (priority) {
            case 'High':
                $(this).addClass('high-priority');
                break;
            case 'Medium':
                $(this).addClass('medium-priority');
                break;
            case 'Low':
                $(this).addClass('low-priority');
                break;
            default:
                break;
        }
    });
});

$(document).ready(function () {
    $(".accordion").click(function () {
        var panel = $(this).next();
        if (panel.css("display") === "block") {
            panel.css("display", "none");
        } else {
            panel.css("display", "block");
        }
    });
});

