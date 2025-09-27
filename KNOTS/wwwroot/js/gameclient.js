console.log("gameclient.js loaded!");

// Globalus SignalR connection objektas
let gameConnection = null;
let currentPlayerId = null;
let currentRoom = null;

function setBlazorGameComponent(component) {
    window.blazorGameComponent = component;
    console.log("Blazor game component set");
}
// Inicializuoja SignalR ryšį

async function initializeGameConnection() {
    try {
        gameConnection = new signalR.HubConnectionBuilder()
            .withUrl("/gamehub")
            .build();

        // Klausytojų registracija
        setupEventListeners();

        // Pradėti ryšį
        await gameConnection.start();
        console.log("SignalR connected");
        return true;
    } catch (err) {
        console.error("SignalR connection error:", err);
        return false;
    }
}

// Registruoja event listener'ius
function setupEventListeners() {
    // Gauna žaidėjo ID
    gameConnection.on("AssignPlayerId", function (playerId) {
        currentPlayerId = playerId;
        console.log("Your player ID:", playerId);

        if (window.blazorGameComponent) {
            window.blazorGameComponent.invokeMethodAsync('OnPlayerIdAssigned', playerId);
        }
    });

    // Kambarys sukurtas
    gameConnection.on("RoomCreated", function (roomCode) {
        currentRoom = roomCode;
        console.log("Room created:", roomCode);

        if (window.blazorGameComponent) {
            window.blazorGameComponent.invokeMethodAsync('OnRoomCreated', roomCode);
        }
    });

    // Sėkmingai prisijungta prie kambario
    gameConnection.on("JoinedRoom", function (roomInfo) {
        currentRoom = roomInfo.roomCode;
        console.log("Joined room:", roomInfo);

        if (window.blazorGameComponent) {
            window.blazorGameComponent.invokeMethodAsync('OnJoinedRoom', roomInfo);
        }
    });

    // Nepavyko prisijungti prie kambario
    gameConnection.on("JoinRoomFailed", function (message) {
        console.log("Failed to join room:", message);

        if (window.blazorGameComponent) {
            window.blazorGameComponent.invokeMethodAsync('OnJoinRoomFailed', message);
        }
    });

    // Žaidėjas prisijungė prie kambario
    gameConnection.on("PlayerJoinedRoom", function (username) {
        console.log("Player joined room:", username);

        if (window.blazorGameComponent) {
            window.blazorGameComponent.invokeMethodAsync('OnPlayerJoinedRoom', username);
        }
    });

    // Žaidėjas paliko kambarį
    gameConnection.on("PlayerLeft", function (username) {
        console.log("Player left:", username);

        if (window.blazorGameComponent) {
            window.blazorGameComponent.invokeMethodAsync('OnPlayerLeft', username);
        }
    });

    // Žaidimo veiksmas
    gameConnection.on("GameAction", function (username, action, data) {
        console.log("Game action:", username, action, data);

        if (window.blazorGameComponent) {
            window.blazorGameComponent.invokeMethodAsync('OnGameAction', username, action, JSON.stringify(data));
        }
    });
}

// Prisijungti prie žaidimo
async function joinGame(username) {
    if (!gameConnection) {
        console.error("Connection not established");
        return false;
    }

    try {
        await gameConnection.invoke("JoinGame", username);
        return true;
    } catch (err) {
        console.error("Error joining game:", err);
        return false;
    }
}

// Sukurti kambarį
async function createRoom(username) {
    if (!gameConnection) {
        console.error("Connection not established");
        return false;
    }

    try {
        await gameConnection.invoke("CreateRoom", username);
        return true;
    } catch (err) {
        console.error("Error creating room:", err);
        return false;
    }
}

// Prisijungti prie kambario
async function joinRoom(roomCode, username) {
    if (!gameConnection) {
        console.error("Connection not established");
        return false;
    }

    try {
        await gameConnection.invoke("JoinRoom", roomCode, username);
        return true;
    } catch (err) {
        console.error("Error joining room:", err);
        return false;
    }
}

// Siųsti žaidimo veiksmą
async function sendGameAction(action, data) {
    if (!gameConnection || !currentRoom) {
        console.error("Connection not established or not in room");
        return false;
    }

    try {
        await gameConnection.invoke("SendGameAction", currentRoom, action, data);
        return true;
    } catch (err) {
        console.error("Error sending game action:", err);
        return false;
    }
}