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

                // =================================================
                // PAGINATION
                // < 1 2 >
                // =================================================

                pagingType: "simple_numbers",

                language: {

                    // ==============================
                    // SEARCH
                    // ==============================

                    search: "",

                    searchPlaceholder: "Search...",

                    // ==============================
                    // ENTRIES
                    // ==============================

                    lengthMenu:
                        "_MENU_ entries per page",

                    // ==============================
                    // TABLE INFO
                    // ==============================

                    info:
                        "Showing _START_ to _END_ of _TOTAL_ records",

                    infoEmpty:
                        "Showing 0 to 0 of 0 records",

                    zeroRecords:
                        "No matching records found",

                    emptyTable:
                        "No records found",

                    // ==============================
                    // PAGINATION
                    // ==============================

                    paginate: {
                        next: ">",
                        previous: "<"
                    }
                },

                initComplete: function () {

                    this.api()
                        .columns
                        .adjust();

                    // Make search placeholder work
                    var searchInput =
                        $(this)
                            .closest('.dataTables_wrapper')
                            .find('.dataTables_filter input');

                    searchInput.attr(
                        'placeholder',
                        'Search...'
                    );

                    searchInput.attr(
                        'aria-label',
                        'Search'
                    );
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