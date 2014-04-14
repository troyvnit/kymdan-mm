$(document).ready(function () {
    $("#RoleSelector").multiselect({
        includeSelectAllOption: true,
        onChange: function() {
            $("#Roles").val($("#RoleSelector").val());
        }
    });
});