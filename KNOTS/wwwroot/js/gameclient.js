
(function (global) {
    console.log("gameclient.js loaded!");

    // --- global objects exposed to other scripts / Blazor ---
    let gameConnection = null;
    let chatConnection = null;
    let currentPlayerId = null;
    let currentRoom = null;
    let currentInGameNickname = null; // raw display name (not normalized)
    let lastKnownChatNormalized = null; // server normalized username (for verification)

    // Expose for Blazor to set the component instance for callbacks
    function setBlazorGameComponent(component) {
        window.blazorGameComponent = component;
        console.log("[client] Blazor game component set");
    }

    // Set the in-game nickname (call this when user chooses nickname or after join/create)
    // This will attempt to propagate the nickname to the chat connection.
    async function setInGameNickname(nickname) {
        if (!nickname) return false;
        currentInGameNickname = nickname;
        console.log(`[client] setInGameNickname -> '${nickname}'`);
        const ok = await ensureChatAndSetUsername(nickname);
        return ok;
    }

    // ---------------- Game connection ----------------
    async function initializeGameConnection() {
        try {
            if (gameConnection) {
                console.warn("[client] Game connection already initialized");
                return true;
            }

            gameConnection = new signalR.HubConnectionBuilder()
                .withUrl("/gamehub")
                .withAutomaticReconnect()
                .build();

            setupGameListeners();

            await gameConnection.start();
            console.log("[client] GameHub connected, connectionId:", gameConnection.connectionId || "(unknown)");
            return true;
        } catch (err) {
            console.error("[client] SignalR game connection error:", err);
            return false;
        }
    }

    function setupGameListeners() {
        if (!gameConnection) return;

        gameConnection.on("AssignPlayerId", function (playerId) {
            currentPlayerId = playerId;
            console.log("[client] AssignPlayerId:", playerId);

            if (window.blazorGameComponent) {
                window.blazorGameComponent.invokeMethodAsync('OnPlayerIdAssigned', playerId).catch(e => console.error(e));
            }
        });

        gameConnection.on("RoomCreated", function (roomCode) {
            currentRoom = roomCode;
            console.log("[client] RoomCreated:", roomCode);

            // If the server included nickname or we have the in-game nickname, propagate it to chat
            if (currentInGameNickname) {
                ensureChatAndSetUsername(currentInGameNickname).catch(e => console.error(e));
            }

            if (window.blazorGameComponent) {
                window.blazorGameComponent.invokeMethodAsync('OnRoomCreated', roomCode).catch(e => console.error(e));
            }
        });

        gameConnection.on("JoinedRoom", function (roomInfo) {
            // Normalize possible payload shapes from server
            currentRoom = roomInfo?.roomCode || roomInfo?.RoomCode || currentRoom;
            console.log("[client] JoinedRoom - Raw data:", roomInfo, "currentRoom:", currentRoom);

            // If roomInfo contains a username or if we already have a nickname, ensure chat is set
            const possibleNickname = roomInfo?.nickname || roomInfo?.username || roomInfo?.displayName;
            if (possibleNickname) {
                currentInGameNickname = possibleNickname;
            }
            if (currentInGameNickname) {
                ensureChatAndSetUsername(currentInGameNickname).catch(e => console.error(e));
            }

            if (window.blazorGameComponent) {
                const normalizedRoomInfo = {
                    RoomCode: currentRoom || "",
                    Players: roomInfo?.players || roomInfo?.Players || []
                };
                window.blazorGameComponent.invokeMethodAsync('OnJoinedRoom', JSON.stringify(normalizedRoomInfo)).catch(e => console.error(e));
            }
        });

        gameConnection.on("JoinRoomFailed", function (message) {
            console.log("[client] JoinRoomFailed:", message);
            if (window.blazorGameComponent) {
                window.blazorGameComponent.invokeMethodAsync('OnJoinRoomFailed', message).catch(e => console.error(e));
            }
        });

        gameConnection.on("PlayerJoinedRoom", function (username) {
            console.log("[client] PlayerJoinedRoom:", username);
            if (window.blazorGameComponent) {
                window.blazorGameComponent.invokeMethodAsync('OnPlayerJoinedRoom', username).catch(e => console.error(e));
            }
        });

        gameConnection.on("PlayerLeft", function (username) {
            console.log("[client] PlayerLeft:", username);
            if (window.blazorGameComponent) {
                window.blazorGameComponent.invokeMethodAsync('OnPlayerLeft', username).catch(e => console.error(e));
            }
        });

        gameConnection.on("GameAction", function (username, action, data) {
            console.log("[client] GameAction:", username, action, data);
            if (window.blazorGameComponent) {
                window.blazorGameComponent.invokeMethodAsync('OnGameAction', username, action, JSON.stringify(data)).catch(e => console.error(e));
            }
        });

        gameConnection.onclose(function (err) {
            console.warn("[client] GameHub connection closed", err);
        });
    }

    // Game actions (invoke server-side)
    async function joinGame(username) {
        if (!gameConnection) {
            console.error("[client] Game connection not established");
            return false;
        }
        try {
            await gameConnection.invoke("JoinGame", username);
            return true;
        } catch (err) {
            console.error("[client] Error joining game:", err);
            return false;
        }
    }

    async function createRoom(username) {
        if (!gameConnection) {
            console.error("[client] Game connection not established");
            return false;
        }
        try {
            await gameConnection.invoke("CreateRoom", username);
            return true;
        } catch (err) {
            console.error("[client] Error creating room:", err);
            return false;
        }
    }

    async function joinRoom(roomCode, username) {
        if (!gameConnection) {
            console.error("[client] Game connection not established");
            return false;
        }
        try {
            await gameConnection.invoke("JoinRoom", roomCode, username);
            return true;
        } catch (err) {
            console.error("[client] Error joining room:", err);
            return false;
        }
    }

    async function sendGameAction(action, data) {
        if (!gameConnection || !currentRoom) {
            console.error("[client] Connection not established or not in room");
            return false;
        }
        try {
            await gameConnection.invoke("SendGameAction", currentRoom, action, data);
            return true;
        } catch (err) {
            console.error("[client] Error sending game action:", err);
            return false;
        }
    }

    async function disconnectFromGame() {
        if (gameConnection) {
            try {
                await gameConnection.stop();
                console.log("[client] Disconnected from game");
            } catch (err) {
                console.error("[client] Error disconnecting from game:", err);
            }
        }
    }

    // ---------------- Chat connection ----------------
    async function initializeChatConnection(username) {
        try {
            if (chatConnection) {
                console.warn("[client] Chat connection already initialized");
                // If username provided, update mapping
                if (username) {
                    await ensureChatAndSetUsername(username);
                }
                return true;
            }

            chatConnection = new signalR.HubConnectionBuilder()
                .withUrl("/chatHub")
                .withAutomaticReconnect()
                .build();

            setupChatListeners();

            await chatConnection.start();
            console.log("[client] ChatHub connected, connectionId:", chatConnection.connectionId || "(unknown)");

            if (username) {
                // Ensure server receives username mapping and wait for it to complete
                try {
                    await chatConnection.invoke("SetUsername", username);
                    console.log(`[client] SetUsername invoked for '${username}'`);
                } catch (err) {
                    console.error("[client] Error invoking SetUsername during initialization:", err);
                }

                // Optional verification
                try {
                    const serverNormalized = await chatConnection.invoke("WhoAmI");
                    lastKnownChatNormalized = serverNormalized;
                    console.log(`[client] WhoAmI => '${serverNormalized}' (expected normalized: '${(username || "").trim().toLowerCase()}')`);
                } catch (err) {
                    console.warn("[client] WhoAmI call failed:", err);
                }
            }

            return true;
        } catch (err) {
            console.error("[client] ChatHub connection error:", err);
            return false;
        }
    }

    function setupChatListeners() {
        if (!chatConnection) return;

        chatConnection.on("ReceiveMessage", function (message) {
            console.log("[client] ReceiveMessage:", message);
            if (window.blazorGameComponent) {
                window.blazorGameComponent.invokeMethodAsync("OnChatMessageReceived", JSON.stringify(message)).catch(e => console.error(e));
            }
        });

        chatConnection.on("MessageSent", function (message) {
            console.log("[client] MessageSent:", message);
            if (window.blazorGameComponent) {
                window.blazorGameComponent.invokeMethodAsync("OnChatMessageSent", JSON.stringify(message)).catch(e => console.error(e));
            }
        });

        chatConnection.onclose(function (err) {
            console.warn("[client] chat connection closed", err);
        });
    }

    // Ensure chat connection exists and set username; returns boolean
    async function ensureChatAndSetUsername(nickname) {
        if (!nickname) {
            console.warn("[client] ensureChatAndSetUsername called without nickname");
            return false;
        }

        // remember the chosen display name
        currentInGameNickname = nickname;

        // initialize chat if needed (initializeChatConnection will call SetUsername if username param provided)
        if (!chatConnection) {
            const ok = await initializeChatConnection(nickname);
            return ok;
        }

        // chatConnection exists: invoke SetUsername explicitly and then verify mapping
        try {
            await chatConnection.invoke("SetUsername", nickname);
            console.log(`[client] Invoked SetUsername('${nickname}')`);

            // verify mapping
            try {
                const serverNormalized = await chatConnection.invoke("WhoAmI");
                lastKnownChatNormalized = serverNormalized;
                console.log(`[client] Server WhoAmI => '${serverNormalized}' (expected normalized: '${nickname.trim().toLowerCase()}')`);
            } catch (err) {
                console.warn("[client] WhoAmI verification failed:", err);
            }

            return true;
        } catch (err) {
            console.error("[client] Error invoking SetUsername:", err);
            return false;
        }
    }

    async function sendChatMessage(receiverId, content) {
        if (!chatConnection) {
            console.error("[client] Chat connection not established");
            return false;
        }
        try {
            console.log(`[client] Sending chat message to='${receiverId}' content='${content}'`);
            await chatConnection.invoke("SendChatMessage", receiverId, content);
            return true;
        } catch (err) {
            console.error("[client] Error sending chat message:", err);
            return false;
        }
    }

    async function whoAmI() {
        if (!chatConnection) {
            console.warn("[client] whoAmI: chat connection not established");
            return null;
        }
        try {
            const res = await chatConnection.invoke("WhoAmI");
            console.log("[client] WhoAmI:", res);
            return res;
        } catch (err) {
            console.error("[client] WhoAmI invoke failed:", err);
            return null;
        }
    }

    async function debugConnectionInfo() {
        if (!chatConnection) {
            console.warn("[client] debugConnectionInfo: chat connection not established");
            return null;
        }
        try {
            const res = await chatConnection.invoke("DebugConnectionInfo");
            console.log("[client] DebugConnectionInfo:", res);
            return res;
        } catch (err) {
            console.error("[client] DebugConnectionInfo invoke failed:", err);
            return null;
        }
    }

    // Expose functions to global/window for debugging and Blazor interop
    global.setBlazorGameComponent = setBlazorGameComponent;
    global.setInGameNickname = setInGameNickname;
    global.initializeGameConnection = initializeGameConnection;
    global.initializeChatConnection = initializeChatConnection;
    global.ensureChatAndSetUsername = ensureChatAndSetUsername;
    global.joinGame = joinGame;
    global.createRoom = createRoom;
    global.joinRoom = joinRoom;
    global.sendGameAction = sendGameAction;
    global.disconnectFromGame = disconnectFromGame;
    global.sendChatMessage = sendChatMessage;
    global.whoAmI = whoAmI;
    global.debugConnectionInfo = debugConnectionInfo;

    // Also export some state for quick inspection
    global._gameClientState = {
        getCurrentPlayerId: () => currentPlayerId,
        getCurrentRoom: () => currentRoom,
        getCurrentInGameNickname: () => currentInGameNickname,
        getLastKnownChatNormalized: () => lastKnownChatNormalized
    };

    console.log("gameclient.js: functions exposed: initializeGameConnection, initializeChatConnection, setInGameNickname, sendChatMessage, whoAmI, debugConnectionInfo, etc.");

})(window);