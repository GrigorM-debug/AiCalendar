function showAlert(message, type) {
    const alertDiv = document.createElement('div'); 

    alertDiv.className = `alert alert-${type}`;
    alertDiv.style.position = 'fixed';
    alertDiv.style.top = '20px';
    alertDiv.style.right = '20px';
    alertDiv.style.padding = '15px';
    alertDiv.style.border = '1px solid #ccc';
    alertDiv.style.borderRadius = '5px';
    alertDiv.style.backgroundColor = backgroundColor(type);
    alertDiv.style.zIndex = 1000;
    alertDiv.innerText = message;
    document.body.appendChild(alertDiv);

    setTimeout(() => {
        document.body.removeChild(alertDiv);
    }, 5000);
}

function backgroundColor(type) {
    switch (type) {
        case 'success':
            return '#d4edda';
        case 'error':
            return '#f8d7da';
        case 'warning':
            return '#fff3cd';
        case 'info':
            return '#d1ecf1';
        default:
            return '#ffffff';
    }
}