const connection = new signalR.HubConnectionBuilder().withUrl("/notificationHub").build();

connection.on("ReceiveNotification", function (message) {
    addNotificationToDropdown(message);
});

connection.start().catch(function (err) {
    console.error("Erro ao conectar ao SignalR:", err.toString());
});

function addNotificationToDropdown(message) {
    const notificationList = document.getElementById("notificationList");

    if (notificationList.children[0] && notificationList.children[0].textContent === "Nenhuma notificação") {
        notificationList.innerHTML = "";
    }

    const notificationItem = document.createElement("li");
    notificationItem.innerHTML = `<span class="dropdown-item">${message}</span>`;
    notificationList.prepend(notificationItem);
}