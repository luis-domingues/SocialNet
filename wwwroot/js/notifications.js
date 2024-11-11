const connection = new signalR.HubConnectionBuilder().withUrl("/notificationHub").build();

connection.on("ReceiveNotification", function (message) {
    addNotificationToDropdown(message);
});

connection.start()
    .then(() => console.log("Conectado ao SignalR"))
    .catch(err => console.error("Erro ao conectar ao SignalR:", err.toString()));

function addNotificationToDropdown(message) {
    const notificationList = document.getElementById("notificationList");

    if (notificationList.children[0] && notificationList.children[0].textContent === "Nenhuma notificação") {
        notificationList.innerHTML = "";
    }

    const notificationItem = document.createElement("li");
    notificationItem.innerHTML = `<span class="dropdown-item">${message}</span>`;
    notificationList.prepend(notificationItem);
}

function markAllNotificationsAsRead() {
    fetch('/Notifications/MarkAllAsRead', { method: 'POST' })
        .then(response => {
            if (response.ok) {
                console.log("Todas as notificações foram marcadas como lidas");
            }
        })
        .catch(error => console.error("Erro ao marcar notificações como lidas:", error));
}

function clearReadNotifications() {
    fetch('/Notifications/ClearReadNotifications', { method: 'POST' })
        .then(response => {
            if (response.ok) {
                const notificationList = document.getElementById("notificationList");
                notificationList.innerHTML = "<li><span class='dropdown-item'>Nenhuma notificação</span></li>";
                console.log("Notificações lidas removidas");
            }
        })
        .catch(error => console.error("Erro ao limpar notificações lidas:", error));
}