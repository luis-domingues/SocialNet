const connection = new signalR.HubConnectionBuilder().withUrl("/notificationHub").build();

connection.on("ReceiveNotification", function (message) {
    console.log("Notificação recebida no frontend:", message);
    addNotificationToDropdown(message);
});

connection.start()
    .then(() => {
        console.log("Conectado ao SignalR com sucesso.");
    })
    .catch(function (err) {
        console.error("Erro ao conectar ao SignalR:", err.toString());
    });

function addNotificationToDropdown(message) {
    console.log("Tentando adicionar notificação ao dropdown:", message); // Log de verificação

    const notificationList = document.getElementById("notificationList");
    if (!notificationList) {
        console.error("Elemento notificationList não encontrado no DOM.");
        return;
    }

    if (notificationList.children[0] && notificationList.children[0].textContent === "Nenhuma notificação") {
        notificationList.innerHTML = "";
    }

    const notificationItem = document.createElement("li");
    notificationItem.innerHTML = `<span class="dropdown-item">${message}</span>`;
    notificationList.prepend(notificationItem);
}