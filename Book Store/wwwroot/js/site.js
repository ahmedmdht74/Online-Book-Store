//All Pages
//Get Cart Count
function updateCartCount() {
        $.ajax({
            url: '/Home/CartCount', 
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                $("#cartCount").text(`(${data})`); 
            },
            error: function () {
                console.log("Error fetching cart count");
            }
        });
    }

updateCartCount();











