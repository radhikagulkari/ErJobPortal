//$(document).ready(function () {

//    /*
//     * Find every table having:
//     *
//     * class="master-data-table"
//     *
//     * and automatically convert it into
//     * a DataTable.
//     */

//    $('.master-data-table').each(function () {

//        var tableElement = $(this);

//        var tableId = tableElement.attr('id');


//        /*
//         * Page information ID
//         *
//         * candidateTable
//         *      ↓
//         * candidatePageInfo
//         *
//         * organizationTable
//         *      ↓
//         * organizationPageInfo
//         */

//        var pageInfoId = tableId.replace(
//            'Table',
//            'PageInfo'
//        );


//        /*
//         * Initialize DataTable
//         */

//        var masterTable = tableElement.DataTable({

//            /* =========================================
//               RECORDS PER PAGE
//               ========================================= */

//            pageLength: 5,


//            /* =========================================
//               DROPDOWN
//               ========================================= */

//            lengthMenu: [
//                [5, 10, 25, 50, 100],
//                [5, 10, 25, 50, 100]
//            ],


//            /* =========================================
//               SORT BY FIRST COLUMN
//               ========================================= */

//            order: [
//                [0, 'asc']
//            ],


//            /* =========================================
//               SEARCH
//               ========================================= */

//            searching: true,


//            /* =========================================
//               PAGINATION
//               ========================================= */

//            paging: true,


//            /* =========================================
//               INFORMATION
//               ========================================= */

//            info: true,


//            /* =========================================
//               HORIZONTAL SCROLL

//               ONLY TABLE WILL SCROLL
//               ========================================= */

//            scrollX: true,


//            /*
//             * Keep column width under control
//             */

//            autoWidth: false,


//            /*
//             * Previous + Number + Next
//             */

//            pagingType: "simple_numbers",


//            /* =========================================
//               TEXT
//               ========================================= */

//            language: {

//                search: "Search:",

//                lengthMenu: "Show _MENU_ entries",

//                info: "Showing _START_ to _END_ of _TOTAL_ records",

//                infoEmpty: "Showing 0 to 0 of 0 records",

//                zeroRecords: "No matching records found",

//                emptyTable: "No records found",

//                paginate: {

//                    next: "Next",

//                    previous: "Previous"

//                }

//            }

//        });


//        /* =========================================
//           PAGE X OF Y
//           ========================================= */

//        function updatePageInfo() {

//            var pageInfo =
//                masterTable.page.info();


//            var currentPage =
//                pageInfo.page + 1;


//            var totalPages =
//                pageInfo.pages;


//            if (totalPages === 0) {

//                $('#' + pageInfoId)
//                    .text('Page 0 of 0');

//            }
//            else {

//                $('#' + pageInfoId)
//                    .text(
//                        'Page ' +
//                        currentPage +
//                        ' of ' +
//                        totalPages
//                    );

//            }

//        }


//        /* =========================================
//           INITIAL PAGE INFORMATION
//           ========================================= */

//        updatePageInfo();


//        /* =========================================
//           UPDATE AFTER:

//           Search
//           Pagination
//           Sorting
//           ========================================= */

//        masterTable.on('draw', function () {

//            updatePageInfo();

//        });

//    });

//});





$(document).ready(function () {

    $('.master-data-table').each(function () {

        var tableElement = $(this);
        var tableId = tableElement.attr('id');

        var pageInfoId = tableId
            ? tableId.replace('Table', 'PageInfo')
            : '';

        // =====================================================
        // COUNT TABLE COLUMNS
        // =====================================================

        var columnCount =
            tableElement.find('thead th').length;

        // =====================================================
        // SCROLL ONLY WHEN MORE THAN 5 COLUMNS
        // =====================================================

        var needsHorizontalScroll =
            columnCount > 5;

        // =====================================================
        // MASTER TABLE CONTAINER
        // =====================================================

        var container =
            tableElement.closest(
                '.master-table-container'
            );

        if (needsHorizontalScroll) {
            container.addClass(
                'master-table-scrollable'
            );
        } else {
            container.removeClass(
                'master-table-scrollable'
            );
        }

        // =====================================================
        // DATATABLE
        // =====================================================

        var masterTable =
            tableElement.DataTable({

                pageLength: 5,

                lengthMenu: [
                    [5, 10, 25, 50, 100],
                    [5, 10, 25, 50, 100]
                ],

                order: [
                    [0, 'asc']
                ],

                searching: true,

                paging: true,

                info: true,

                // > 5 columns = horizontal scroll
                scrollX: needsHorizontalScroll,

                autoWidth: true,

                pagingType: "simple_numbers",

                language: {

                    search: "Search:",

                    lengthMenu:
                        "Show _MENU_ entries",

                    info:
                        "Showing _START_ to _END_ of _TOTAL_ records",

                    infoEmpty:
                        "Showing 0 to 0 of 0 records",

                    zeroRecords:
                        "No matching records found",

                    emptyTable:
                        "No records found",

                    paginate: {
                        next: "Next",
                        previous: "Previous"
                    }
                },

                initComplete: function () {

                    this.api()
                        .columns
                        .adjust();
                }
            });

        // =====================================================
        // PAGE X OF Y
        // =====================================================

        function updatePageInfo() {

            if (!pageInfoId) {
                return;
            }

            var pageInfo =
                masterTable.page.info();

            var currentPage =
                pageInfo.pages > 0
                    ? pageInfo.page + 1
                    : 0;

            var totalPages =
                pageInfo.pages;

            $('#' + pageInfoId).text(
                'Page ' +
                currentPage +
                ' of ' +
                totalPages
            );
        }

        updatePageInfo();

        // =====================================================
        // UPDATE AFTER DRAW
        // =====================================================

        masterTable.on(
            'draw',
            function () {

                updatePageInfo();

                setTimeout(function () {

                    masterTable
                        .columns
                        .adjust();

                }, 10);
            }
        );

        // =====================================================
        // WINDOW RESIZE
        // =====================================================

        $(window).on(
            'resize.masterTable_' + tableId,
            function () {

                masterTable
                    .columns
                    .adjust();
            }
        );

    });

});