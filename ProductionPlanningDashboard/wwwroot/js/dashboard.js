
// Utility Functions
function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toastContainer');
    const toastId = 'toast-' + Date.now();
    
    const toastHtml = `
        <div class="toast" id="${toastId}" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header">
                <i class="bi bi-${getToastIcon(type)} text-${type} me-2"></i>
                <strong class="me-auto">Notification</strong>
                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;
    
    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    
    const toast = new bootstrap.Toast(document.getElementById(toastId));
    toast.show();
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        const toastElement = document.getElementById(toastId);
        if (toastElement) {
            toastElement.remove();
        }
    }, 5000);
}

function getToastIcon(type) {
    switch (type) {
        case 'success': return 'check-circle-fill';
        case 'danger': return 'exclamation-triangle-fill';
        case 'warning': return 'exclamation-triangle-fill';
        default: return 'info-circle-fill';
    }
}

// Dashboard Functions
function refreshDashboard() {
    const form = document.getElementById('filterForm');
    if (form) {
        form.submit();
    } else {
        location.reload();
    }
}

function clearFilters() {
    document.getElementById('CompanyId').value = '';
    document.getElementById('DepartmentId').value = '';
    document.getElementById('StartDate').value = '';
    document.getElementById('EndDate').value = '';
    document.getElementById('PeriodType').value = 'Weekly';
    
    // Submit form to refresh with cleared filters
    document.getElementById('filterForm').submit();
}

function exportToExcel() {
    const table = document.getElementById('summaryTable');
    if (!table) {
        showToast('No data to export', 'warning');
        return;
    }
    
    // Simple CSV export (you can enhance this with a proper Excel library)
    let csv = '';
    const rows = table.querySelectorAll('tr');
    
    rows.forEach(row => {
        const cols = row.querySelectorAll('td, th');
        const rowData = Array.from(cols).map(col => {
            return '"' + col.textContent.trim().replace(/"/g, '""') + '"';
        });
        csv += rowData.join(',') + '\n';
    });
    
    // Download CSV
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'production-summary-' + new Date().toISOString().split('T')[0] + '.csv';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
    
    showToast('Data exported successfully', 'success');
}

// Auto-submit filters on change
document.addEventListener('DOMContentLoaded', function() {
    const filterElements = ['CompanyId', 'DepartmentId', 'PeriodType'];
    
    filterElements.forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            element.addEventListener('change', function() {
                // Add small delay for better UX
                setTimeout(() => {
                    document.getElementById('filterForm').submit();
                }, 300);
            });
        }
    });
    
    // Handle date changes differently (wait for both dates to be selected)
    const startDate = document.getElementById('StartDate');
    const endDate = document.getElementById('EndDate');
    
    if (startDate && endDate) {
        let dateTimeout;
        
        function handleDateChange() {
            clearTimeout(dateTimeout);
            dateTimeout = setTimeout(() => {
                if (startDate.value && endDate.value) {
                    document.getElementById('filterForm').submit();
                }
            }, 1000);
        }
        
        startDate.addEventListener('change', handleDateChange);
        endDate.addEventListener('change', handleDateChange);
    }
});

// Initialize tooltips and popovers
document.addEventListener('DOMContentLoaded', function() {
    // Initialize Bootstrap tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Initialize Bootstrap popovers
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    const popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
});
