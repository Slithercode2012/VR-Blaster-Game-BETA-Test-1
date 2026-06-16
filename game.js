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

// Materials for the Human Characters
const bodyMat = new THREE.MeshStandardMaterial({ color: 0xef4444, roughness: 0.5 }); // Red clothes
const skinMat = new THREE.MeshStandardMaterial({ color: 0xffdbac, roughness: 0.6 }); // Humanoid skin tone
const bootMat = new THREE.MeshStandardMaterial({ color: 0x111111 });                 // Black shoes

// --- FUNCTION TO BUILD A COMPOSITE 3D HUMAN CHARACTER ---
function createHumanoidMesh() {
    const humanGroup = new THREE.Group();

    // 1. Torso (Body)
    const torsoGeo = new THREE.BoxGeometry(1.2, 1.6, 0.6);
    const torso = new THREE.Mesh(torsoGeo, bodyMat);
    torso.position.y = 1.6;
    humanGroup.add(torso);

    // 2. Head
    const headGeo = new THREE.BoxGeometry(0.7, 0.7, 0.7);
    const head = new THREE.Mesh(headGeo, skinMat);
    head.position.y = 2.75;
    humanGroup.add(head);

    // 3. Left Leg
    const leftLegGeo = new THREE.BoxGeometry(0.4, 1.0, 0.4);
    const leftLeg = new THREE.Mesh(leftLegGeo, bootMat);
    leftLeg.position.set(-0.35, 0.5, 0);
    // Offset center point to the hip line for realistic swinging
    leftLegGeo.translate(0, -0.4, 0);
    leftLeg.position.y = 1.2;
    humanGroup.add(leftLeg);

    // 4. Right Leg
    const rightLegGeo = new THREE.BoxGeometry(0.4, 1.0, 0.4);
    const rightLeg = new THREE.Mesh(rightLegGeo, bootMat);
    rightLeg.position.set(0.35, 0.5, 0);
    rightLegGeo.translate(0, -0.4, 0);
    rightLeg.position.y = 1.2;
    humanGroup.add(rightLeg);

    // 5. Left Arm
    const leftArmGeo = new THREE.BoxGeometry(0.35, 1.2, 0.35);
    const leftArm = new THREE.Mesh(leftArmGeo, skinMat);
    leftArm.position.set(-0.8, 2.0, 0);
    leftArmGeo.translate(0, -0.5, 0);
    humanGroup.add(leftArm);

    // 6. Right Arm
    const rightArmGeo = new THREE.BoxGeometry(0.35, 1.2, 0.35);
    const rightArm = new THREE.Mesh(rightArmGeo, skinMat);
    rightArm.position.set(0.8, 2.0, 0);
    rightArmGeo.translate(0, -0.5, 0);
    humanGroup.add(rightArm);

    // Store references to the limbs inside the object group for animations later
    humanGroup.userData = {
        leftLeg: leftLeg,
        rightLeg: rightLeg,
        leftArm: leftArm,
        rightArm: rightArm
    };

    return humanGroup;
}

// --- SPAWN AI BOTS ---
function spawnBot(id) {
    const bot = createHumanoidMesh(); // Builds full human shape instead of a cube!
    bot.position.set((Math.random() - 0.5) * 60, 0, (Math.random() - 0.5) * 60);
    
    bot.userData.id = id;
    bot.userData.hp = 40;
    bot.userData.targetX = bot.position.x;
    bot.userData.targetZ = bot.position.z;
    bot.userData.timer = 0;
    bot.userData.animTime = Math.random() * 10; // Randomize walk cycles

    scene.add(bot);
    bots.push(bot);
}

// Spawn 6 active custom moving human bots
for (let i = 0; i < 6; i++) spawnBot("Rival_Bot_" + i);

// --- MULTIPLAYER SETUP (PUBNUB NETWORKING) ---
function initMultiplayer() {
    pubnub = new PubNub({
        publishKey: "pub-c-8da6dca2-bdc4-4b95-bfdf-d9d107a6729e",
        subscribeKey: "sub-c-57d4ccf1-bf63-448e-9c71-337dbeae8488",
        uuid: myID
    });

    pubnub.addListener({
        message: function(event) {
            const data = event.message;
            if (event.publisher === myID) return;

            // Update human-shaped multiplayer avatar tracks live
            if (data.type === "move") {
                if (!networkPlayers[event.publisher]) {
                    networkPlayers[event.publisher] = createHumanoidMesh(); // Other players are human shapes too!
                    scene.add(networkPlayers[event.publisher]);
                    addSystemMessage(`${data.name} joined the active map room.`);
                }
                networkPlayers[event.publisher].position.set(data.x, 0, data.z);
                updateLobbyList();
            }

            if (data.type === "shoot_bot") {
                const targetBot = bots.find(b => b.userData.id === data.botId);
                if (targetBot) {
                    targetBot.position.set(0, -50, 0); // Hide bot
                    addSystemMessage(`💥 ${data.name} neutralized ${data.botId}!`);
                    setTimeout(() => { targetBot.position.set((Math.random() - 0.5) * 50, 0, (Math.random() - 0.5) * 50); }, 2000);
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

function broadcastMovement() {
    if (!pubnub) return;
    pubnub.publish({
        channel: ROOM_CHANNEL,
        message: { type: "move", name: myName, x: camera.position.x, z: camera.position.z },
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
    addSystemMessage(`System: Routed successfully. Connection confirmed as ${myName}.`);
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
    
    // Find intersections with child pieces inside the robot/human character groups
    const targetHits = raycaster.intersectObjects(scene.children, true);

    if (targetHits.length > 0) {
        // Trace back the clicked piece up to its main parent human group
        let hitObject = targetHits[0].object;
        while (hitObject.parent && hitObject.parent !== scene) {
            hitObject = hitObject.parent;
        }

        if (bots.includes(hitObject)) {
            hitObject.userData.hp -= 20;

            if (hitObject.userData.hp <= 0) {
                hitObject.userData.hp = 40;
                hitObject.position.set(0, -100, 0); // Drop out of view temporarily

                score += 50;
                document.getElementById('score').innerText = score;
                addSystemMessage(`🎯 Humanoid Drone Down! +50 Match Points.`);

                if(pubnub) {
                    pubnub.publish({
                        channel: ROOM_CHANNEL,
                        message: { type: "shoot_bot", name: myName, botId: hitObject.userData.id }
                    });
                }

                setTimeout(() => { hitObject.position.set((Math.random() - 0.5) * 60, 0, (Math.random() - 0.5) * 60); }, 2500);
            }
        }
    }
});

// --- CORE FRAME RENDER LOOP (60FPS CONTINUOUS REFRESH) ---
const clock = new THREE.Clock();
camera.position.set(0, 1.8, 10); // Elevated camera view to meet character eye positions

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

        if (moveF || moveB || moveL || moveR) broadcastMovement();
    }

    // --- HUMANOID BOT MOVEMENT & WALKING LIMB ANIMATION ---
    bots.forEach(bot => {
        bot.userData.timer += dt;
        bot.userData.animTime += dt * 7; // Speed of limb swing rate

        // 1. Path coordinates recalculator
        if (bot.userData.timer > 4) {
            bot.userData.targetX = bot.position.x + (Math.random() - 0.5) * 20;
            bot.userData.targetZ = bot.position.z + (Math.random() - 0.5) * 20;
            bot.userData.timer = 0;
        }

        // Keep previous spot data to compute angles
        const oldX = bot.position.x;
        const oldZ = bot.position.z;

        // Smooth translation forward
        bot.position.x += (bot.userData.targetX - bot.position.x) * dt * 0.4;
        bot.position.z += (bot.userData.targetZ - bot.position.z) * dt * 0.4;

        // Face the direction they are actively running towards
        const dx = bot.position.x - oldX;
        const dz = bot.position.z - oldZ;
        if (Math.abs(dx) > 0.001 || Math.abs(dz) > 0.001) {
            bot.rotation.y = Math.atan2(dx, dz);
            
            // 2. RUNNING ANIMATION PHYSICS: Swing legs and arms using math sine waves
            const swing = Math.sin(bot.userData.animTime) * 0.6;
            bot.children[2].rotation.x = swing;  // Left Leg forward
            bot.children[3].rotation.x = -swing; // Right Leg backward
            bot.children[4].rotation.x = -swing; // Left Arm backward
            bot.children[5].rotation.x = swing;  // Right Arm forward
        }

        // Damage Tracker: Check if any bot hunts down and touches player position boundaries
        const dist = camera.position.distanceTo(bot.position);
        if (dist < 2.5 && hp > 0) {
            hp -= 20 * dt;
            document.getElementById('hp').innerText = Math.max(0, Math.floor(hp));
            if (hp <= 0) {
                addSystemMessage("💀 Critical Failure. Respawning at starting zone...");
                setTimeout(() => { hp = 100; camera.position.set(0, 1.8, 10); }, 2000);
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
