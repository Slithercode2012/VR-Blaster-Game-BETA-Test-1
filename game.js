// --- MULTIPLAYER CHANNEL KEYS (FREE PUBLIC TEST NETWORK) ---
let pubnub;
const ROOM_CHANNEL = "titan_rivals_global_lobby_v3";
let myID = "Player_" + Math.floor(Math.random() * 9000);
let myName = "Guest";

// --- 3D ENGINE INITIALIZATION ---
const scene = new THREE.Scene();
scene.background = new THREE.Color(0x090d16);
scene.fog = new THREE.FogExp2(0x090d16, 0.04);

const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 500);
const renderer = new THREE.WebGLRenderer({ antialias: true });
renderer.setSize(window.innerWidth, window.innerHeight);
document.body.appendChild(renderer.domElement);

// Lighting configs
scene.add(new THREE.AmbientLight(0xffffff, 0.5));
const dirLight = new THREE.DirectionalLight(0x00ffcc, 0.8);
dirLight.position.set(10, 30, 10);
scene.add(dirLight);

// Build Level Grid Floor
const floorGeo = new THREE.GridHelper(120, 60, 0x00ffcc, 0x1e293b);
floorGeo.position.y = 0;
scene.add(floorGeo);

// --- CLONE NETWORK HOOKS & BOT ARRAYS ---
const networkPlayers = {}; // Stores real people connecting online
const bots = [];           // Stores local AI enemy bots
let moveF = false, moveB = false, moveL = false, moveR = false;
let yaw = 0, pitch = 0, score = 0, hp = 100;

// Geometry layouts
const avatarGeo = new THREE.CylinderGeometry(0.8, 0.8, 2.5, 8);
const avatarMat = new THREE.MeshStandardMaterial({ color: 0x3b82f6, emissive: 0x1d4ed8 });
const botGeo = new THREE.BoxGeometry(1.8, 1.8, 1.8);
const botMat = new THREE.MeshStandardMaterial({ color: 0xef4444, roughness: 0.2 });

// --- SPAWN AI BOTS ---
function spawnBot(id) {
    const bot = new THREE.Mesh(botGeo, botMat);
    bot.position.set((Math.random() - 0.5) * 60, 0.9, (Math.random() - 0.5) * 60);
    bot.userData = { id: id, hp: 40, targetX: bot.position.x, targetZ: bot.position.z, timer: 0 };
    scene.add(bot);
    bots.push(bot);
}
// Add 6 active hunter bots to track across the level
for (let i = 0; i < 6; i++) spawnBot("Bot_" + i);

// --- MULTIPLAYER SETUP (PUBNUB NETWORKING) ---
function initMultiplayer() {
    pubnub = new PubNub({
        publishKey: "pub-c-8da6dca2-bdc4-4b95-bfdf-d9d107a6729e", // Public educational sandbox keys
        subscribeKey: "sub-c-57d4ccf1-bf63-448e-9c71-337dbeae8488",
        uuid: myID
    });

    // Handle incoming data streams from other players over the internet
    pubnub.addListener({
        message: function(event) {
            const data = event.message;
            if (event.publisher === myID) return; // Ignore data sent by ourselves

            // 1. If another player moves, update their 3D avatar clone position live
            if (data.type === "move") {
                if (!networkPlayers[event.publisher]) {
                    networkPlayers[event.publisher] = new THREE.Mesh(avatarGeo, avatarMat);
                    scene.add(networkPlayers[event.publisher]);
                    addSystemMessage(`${data.name} joined the active map room.`);
                }
                networkPlayers[event.publisher].position.set(data.x, data.y, data.z);
                updateLobbyList();
            }

            // 2. If another player shoots an object, sync the visual data
            if (data.type === "shoot_bot") {
                const targetBot = bots.find(b => b.userData.id === data.botId);
                if (targetBot) {
                    targetBot.position.set(0, -50, 0); // Hide bot
                    addSystemMessage(`💥 ${data.name} neutralized ${data.botId}!`);
                    setTimeout(() => { targetBot.position.set((Math.random() - 0.5) * 50, 0.9, (Math.random() - 0.5) * 50); }, 2000);
                }
            }
        },
        presence: function(event) {
            if (event.action === "leave" || event.action === "timeout") {
                if (networkPlayers[event.uuid]) {
                    scene.remove(networkPlayers[event.uuid]);
                    delete networkPlayers[event.uuid];
                    addSystemMessage(`Player disconnected.`);
                    updateLobbyList();
                }
            }
        }
    });

    pubnub.subscribe({ channels: [ROOM_CHANNEL], withPresence: true });
}

// Broadcasts your current coordinates over the internet cloud server channel
function broadcastMovement() {
    if (!pubnub) return;
    pubnub.publish({
        channel: ROOM_CHANNEL,
        message: { type: "move", name: myName, x: camera.position.x, y: 1.2, z: camera.position.z },
        storeInHistory: false
    });
}

// --- INTERACTIVE SYSTEM OVERLAYS ---
document.getElementById('start-btn').addEventListener('click', () => {
    const input = document.getElementById('username-input').value.trim();
    if (input) myName = input;
    document.getElementById('menu-overlay').style.display = 'none';
    document.body.requestPointerLock();
    initMultiplayer();
    updateLobbyList();
    addSystemMessage(`System: Routed successfully. Identity confirmed as ${myName}.`);
});

function updateLobbyList() {
    let listHTML = `• <b>${myName} (You)</b><br>`;
    Object.keys(networkPlayers).forEach(id => {
        listHTML += `• Active Peer User<br>`;
    });
    document.getElementById('player-list').innerHTML = listHTML;
}

function addSystemMessage(text) {
    const chat = document.getElementById('chat-box');
    const msg = document.createElement('div');
    msg.innerText = text;
    chat.appendChild(msg);
    chat.scrollTop = chat.scrollHeight;
}

// --- FPS CONTROLS & CAMERA MOVEMENT ---
document.addEventListener('mousemove', (e) => {
    if (document.pointerLockElement !== document.body) return;
    yaw -= e.movementX * 0.0022;
    pitch -= e.movementY * 0.0022;
    pitch = Math.max(-Math.PI / 2.4, Math.min(Math.PI / 2.4, pitch));
    camera.rotation.set(0, 0, 0); camera.rotation.y = yaw; camera.rotation.x = pitch;
});

document.addEventListener('keydown', (e) => {
    if (e.code === 'KeyW') moveF = true; if (e.code === 'KeyS') moveB = true;
    if (e.code === 'KeyA') moveL = true; if (e.code === 'KeyD') moveR = true;
});
document.addEventListener('keyup', (e) => {
    if (e.code === 'KeyW') moveF = false; if (e.code === 'KeyS') moveB = false;
    if (e.code === 'KeyA') moveL = false; if (e.code === 'KeyD') moveR = false;
});

// --- RAYCAST COMBAT ENGINE ---
const raycaster = new THREE.Raycaster();
const screenCenter = new THREE.Vector2(0, 0);

document.addEventListener('mousedown', () => {
    if (document.pointerLockElement !== document.body) return;

    raycaster.setFromCamera(screenCenter, camera);
    const targetHits = raycaster.intersectObjects(bots);

    if (targetHits.length > 0) {
        const hitBot = targetHits[0].object;
        hitBot.userData.hp -= 20;

        // Visual flash response on impact
        hitBot.material.color.setHex(0xffffff);
        setTimeout(() => hitBot.material.color.setHex(0xef4444), 80);

        if (hitBot.userData.hp <= 0) {
            hitBot.userData.hp = 40;
            hitBot.position.set(0, -100, 0); // Temporary drop out of world boundaries

            score += 50;
            document.getElementById('score').innerText = score;
            addSystemMessage(`🎯 Target destroyed! +50 Match Points.`);

            // Tell all other clients via network that this bot died
            if(pubnub) {
                pubnub.publish({
                    channel: ROOM_CHANNEL,
                    message: { type: "shoot_bot", name: myName, botId: hitBot.userData.id }
                });
            }

            // Respawn bot in a different location after 2.5 seconds
            setTimeout(() => { hitBot.position.set((Math.random() - 0.5) * 60, 0.9, (Math.random() - 0.5) * 60); }, 2500);
        }
    }
});

// --- CORE FRAME RENDER LOOP (60FPS CONTINUOUS REFRESH) ---
const clock = new THREE.Clock();
camera.position.set(0, 1.5, 10);

function animate() {
    requestAnimationFrame(animate);
    const dt = clock.getDelta();

    if (document.pointerLockElement === document.body) {
        const speed = 10 * dt;
        const forward = new THREE.Vector3(0, 0, -1).applyQuaternion(camera.quaternion);
        forward.y = 0; forward.normalize();
        const right = new THREE.Vector3(1, 0, 0).applyQuaternion(camera.quaternion);
        right.y = 0; right.normalize();

        if (moveF) camera.position.addScaled(forward, speed);
        if (moveB) camera.position.addScaled(forward, -speed);
        if (moveL) camera.position.addScaled(right, -speed);
        if (moveR) camera.position.addScaled(right, speed);

        // Every frame you step forward, tell everyone on the web where you are standing
        if (moveF || moveB || moveL || moveR) broadcastMovement();
    }

    // --- ENEMY BOT AI BEHAVIOR LOGIC ---
    bots.forEach(bot => {
        bot.userData.timer += dt;

        // 1. Bots wander randomly towards targets
        if (bot.userData.timer > 3) {
            bot.userData.targetX = bot.position.x + (Math.random() - 0.5) * 15;
            bot.userData.targetZ = bot.position.z + (Math.random() - 0.5) * 15;
            bot.userData.timer = 0;
        }

        // Move bot gently towards wander target coordinates
        bot.position.x += (bot.userData.targetX - bot.position.x) * dt * 0.5;
        bot.position.z += (bot.userData.targetZ - bot.position.z) * dt * 0.5;
        bot.rotation.y += 0.01;

        // 2. Damage Tracking: Check if bot gets close to player position
        const dist = camera.position.distanceTo(bot.position);
        if (dist < 2.2 && hp > 0) {
            hp -= 15 * dt; // Rapid damage over time if overlapping with hostile bots
            document.getElementById('hp').innerText = Math.max(0, Math.floor(hp));
            if (hp <= 0) {
                addSystemMessage("💀 Critical Damage. Terminal connection drop.");
                setTimeout(() => { hp = 100; camera.position.set(0, 1.5, 10); }, 3000);
            }
        }
    });

    renderer.render(scene, camera);
}

window.addEventListener('resize', () => {
    camera.aspect = window.innerWidth / window.innerHeight; camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
});

animate();
