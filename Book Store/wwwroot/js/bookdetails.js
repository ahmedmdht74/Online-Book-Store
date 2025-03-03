$(document).ready(function () {
    const mainBookId = document.getElementById("mainid").value;

    //Favorite
    document.getElementById("heartCheckbox").addEventListener("change", function () {
        let isChecked = this.checked;
        let itemId = this.getAttribute("data-item-id");
        console.log(isChecked);
        console.log(itemId);
    
        fetch(`/Home/AddRemoveFavourite?itemId=${itemId}&isChecked=${isChecked}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'text/json'
            },
        })
            .then(response => response.json())
            .then(data => console.log(data))
            .catch(error => console.error('Error:', error));
    });
    




    //Make review
    $(".star").hover(function () {
        let value = $(this).data("value");
    $(".star").each(function () {
        $(this).toggleClass("selected", $(this).data("value") <= value);
            });
        });

    $(".star").click(function () {
        let value = $(this).data("value");
    $("#selectedRating").val(value);
        });

    $("#submitRating").click(function () {
        let rating = $("#selectedRating").val();
    let message = $("#ratingMessage").val();
    let productId = mainBookId;

    if (rating == "0") {
        $("#ratingResponse").html("<div class='text-danger h4'>Please select a rating.</div>");
    return;
            }

    console.log(productId);
    console.log(rating);
    console.log(message);

    $.ajax({
        url: '/Home/MakeReview',
    type: 'POST',
    data: {productId: productId, stars: rating, review: message },
    success: function (response) {
        $("#ratingResponse").html("<div class='text-success h4'>Thank you for your rating!</div>");
                },
    error: function (err) {
        console.log(err);
    $("#ratingResponse").html(`<div class='text-danger h4'>${err.responseText}.</div>`);
                }
            });
        });





    //Increase button
    $("#increaseBtn").click(function () {
        let input = $("#cartQuantity");
        let value = parseInt(input.val());
        console.log(mainBookId);
        if (value < input.attr("max")) {
            input.val(value + 1);
        }
    });


    //Decrease button
    $("#decreaseBtn").click(function () {
        let input = $("#cartQuantity");
    let value = parseInt(input.val());

            if (value > input.attr("min")) {
        input.val(value - 1);
            }
        });




    //Add Book
    $("#addToCartBtn").click(function () {
        let quantity = $("#cartQuantity").val();

        $.ajax({
            url: `/Home/AddNewBook?BookId=${mainBookId}&Quantity=${quantity}`,
    type: "Get",
    success: function (response) {
                    if (response.result) {
        $("#toastSuccessMessage").text(response.message); // Update message
    showToast("toastSuccess",2000);
    updateCartCount();
                    } else {
        $("#toastErrorMessage").text(response.message); // Update message
    showToast("toastError",2000);
                    }
                },
    error: function () {
        $("#toastErrorMessage").text("An unexpected error occurred.");
    showToast("toastError",2000);
                }
            });
        });

    // Function to show toast
    function showToast(toastId, duration = 5000) {
        let toastElement = document.getElementById(toastId);
    let toast = new bootstrap.Toast(toastElement, {delay: duration });
    toast.show();
        }



    //Add in Related Books
    $(".addToCartAjax").click(function (event) {
            event.preventDefault(); // Prevent default link action

        let bookId = $(this).data("book-id"); // Get Book ID from the button

        $.ajax({
            url: `/Home/AddNewBook?BookId=${bookId}&Quantity=1`,
        type: "Get",
        success: function (response) {
                        if (response.result) {
            $("#toastSuccessMessage").text(response.message); // Update message
        showToast("toastSuccess",2000);
        updateCartCount();
                        } else {
            $("#toastErrorMessage").text(response.message); // Update message
        showToast("toastError",2000);
                        }
        },
        error: function () {
            $("#toastErrorMessage").text("An unexpected error occurred.");
        showToast("toastError",2000);
                    }
        });
    });




});
