    $(document).ready(function () {
        function showToast(type, message) {
            if (type === "success") {
                $("#toastSuccessMessage").text(message);
                var toast = new bootstrap.Toast($("#toastSuccess"));
                toast.show();
            } else if (type === "error") {
                $("#toastErrorMessage").text(message);
                var toast = new bootstrap.Toast($("#toastError"));
                toast.show();
            }
        }

        function loadBooks() {
            $.ajax({
                url: "/Home/CartDetail",
                type: "GET",
                dataType: "json",
                success: function (response) {
                    $("#book-list").empty();
                    
                    if (response.booksDetails.length === 0) {
                        $("#book-list").html(`
                            <div class="col-md-8 mx-auto text-center shadow my-2">
                                <h2 class="p-4">No Elements in your cart...</h2>
                            </div>
                        `);
                        $("#checkout-btn").prop("disabled", true).addClass("disabled");
                    } else {
                        response.booksDetails.forEach(function (book) {
                            $("#book-list").append(`
                                <div class="col-md-4 shadow bg-white my-2" id="book-${book.bookId}" style="max-width: 450px;">
                                    <div class="card mb-3 p-1 border-0 h-100 w-100" style="background-color:#D3D3D3">
                                        <div class="row g-0">
                                            <div class="col-md-4 p-2">
                                                <img src="${book.bookImage}" class="img-fluid rounded-start rounded-end" alt="...">
                                            </div>
                                            <div class="col-md-8">
                                                <div class="card-body">
                                                    <h5 class="card-title fw-bold mb-0">${book.bookTitle}</h5>
                                                    <p class="card-text m-0"><small class="text-muted">By ${book.authorName}</small></p>
                                                    <h5 class="card-title fw-bold m-0">E£${book.price}</h5>
                                                </div>
                                                <div class="card-body row">
                                                    <div class="col-8">
                                                        <span>
                                                            <a href="#" class="btn btncol text-white remove-book"
                                                               data-bookid="${book.bookId}"><i class="fa-solid fa-minus"></i></a>
                                                        </span>
                                                        <span class="btn fw-bold" id="quantity-${book.bookId}">${book.quantity}</span>
                                                        <span>
                                                            <a href="#" class="btn btncol text-white add-book"
                                                               data-bookid="${book.bookId}"><i class="fa-solid fa-plus"></i></a>
                                                        </span>
                                                    </div>
                                                    <div class="col-4">
                                                        <span>
                                                            <a href="#" class="btn btncol text-white remove-all"
                                                               data-bookid="${book.bookId}"><i class="fa-solid fa-trash"></i></a>
                                                        </span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            `);
                        });

                        $("#subtotal").text("E£" + response.subTotal);
                        $("#total").text("E£" + response.total);
                        $("#checkout-btn").prop("disabled", false).removeClass("disabled");
                    }
                },
                error: function () {
                    showToast("error", "Error loading books.");
                }
            });
        }

        function updateQuantity(bookId, actionType) {
            $.ajax({
                url: "/Home/updateQuantity",
                type: "POST",
                data: { BookId: bookId, ActionType: actionType },
                success: function (response) {
                    if (response.success) {

                        showToast("success", "Cart updated successfully!");
                        updateCartCount();

                        if (response.newQuantity > 0) {
                            $("#quantity-" + bookId).text(response.newQuantity);
                        } else {
                            $("#book-" + bookId).remove();
                        }

                        $("#subtotal").text("E£" + response.subTotal);
                        $("#total").text("E£" + response.total);

                        if ($(".shadow.bg-white.my-2").length === 0) {
                            $("#book-list").html(`
                                <div class="col-md-8 mx-auto text-center shadow my-2">
                                    <h2 class="p-4">No Elements in your cart...</h2>
                                </div>
                            `);
                            $("#checkout-btn").prop("disabled", true).addClass("disabled");
                        } else {
                            $("#checkout-btn").prop("disabled", false).removeClass("disabled");
                        }
                    } else {
                        showToast("error", "Failed to update cart.");
                    }
                },
                error: function () {
                    showToast("error", "Error updating book quantity.");
                    updateCartCount();
                }
            });
        }

        $("#book-list").on("click", ".add-book", function (e) {
            e.preventDefault();
            var bookId = $(this).data("bookid");
            updateQuantity(bookId, "add");
        });

        $("#book-list").on("click", ".remove-book", function (e) {
            e.preventDefault();
            var bookId = $(this).data("bookid");
            var currentQuantity = parseInt($("#quantity-" + bookId).text());

            if (currentQuantity === 1) {
                updateQuantity(bookId, "removeAll");
            } else {
                updateQuantity(bookId, "removeOne");
            }
        });

        $("#book-list").on("click", ".remove-all", function (e) {
            e.preventDefault();
            var bookId = $(this).data("bookid");
            updateQuantity(bookId, "removeAll");
        });

        loadBooks();
    });
