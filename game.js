// --- 1. SET UP THE 3D WORLD ---
const scene = new THREE.Scene();
scene.background = new THREE.Color(0x0f172a); // Dark sci-fi blue background

const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
const renderer = new THREE.WebGLRenderer({ antialias: true });
renderer.setSize(window.innerWidth, window.innerHeight);
document.body.appendChild(renderer.domElement);

// Add basic lighting
const light = new THREE.AmbientLight(0xffffff, 0.6);
scene.add(light);
const dLight = new THREE.DirectionalLight(0xffffff, 0.8);
dLight.position.set(5, 10, 7);
scene.add(dLight);

// Create a simple floor grid
const floorGeo = new THREE.PlaneGeometry(80, 80);
const floorMat = new THREE.MeshStandardMaterial({ color: 0x1e293b });
const floor = new THREE.Mesh(floorGeo, floorMat);
floor.rotation.x = -Math.PI / 2; // Lay it flat
scene.add(floor);

// --- 2. SPAWN FLOATING ENEMY TARGETS (CUBES) ---
const enemies = [];
const enemyGeo = new THREE.BoxGeometry(2, 2, 2);
const enemyMat = new THREE.MeshStandardMaterial({ color: 0xef4444 }); // Red targets

function spawnEnemy() {
    const enemy = new THREE.Mesh(enemyGeo, enemyMat);
    // Random position on the map
    enemy.position.set((Math.random() - 0.5) * 30, 1.5, (Math.random() - 0.5) * 30);
    scene.add(enemy);
    enemies.push(enemy);
}

// Spawn 5 initial enemies
for (let i = 0; i < 5; i++) spawnEnemy();

// --- 3. KEYBOARD & MOUSE INTERACTION ---
let moveF = false, moveB = false, moveL = false, moveR = false;
let yaw = 0, pitch = 0;
let score = 0;

const clickScreen = document.getElementById('click-screen');
clickScreen.addEventListener('click', () => {
    document.body.requestPointerLock(); // Locks your mouse into the center screen
});

document.addEventListener('pointerlockchange', () => {
    clickScreen.style.display = document.pointerLockElement === document.body ? 'none' : 'flex';
});

// Look around with mouse
document.addEventListener('mousemove', (e) => {
    if (document.pointerLockElement !== document.body) return;
    yaw -= e.movementX * 0.0025;
    pitch -= e.movementY * 0.0025;
    pitch = Math.max(-Math.PI / 2.5, Math.min(Math.PI / 2.5, pitch)); // Stop flipping upside down
    
    camera.rotation.set(0, 0, 0);
    camera.rotation.y = yaw;
    camera.rotation.x = pitch;
});

// WASD Key tracking
document.addEventListener('keydown', (e) => {
    if (e.code === 'KeyW') moveF = true;
    if (e.code === 'KeyS') moveB = true;
    if (e.code === 'KeyA') moveL = true;
    if (e.code === 'KeyD') moveR = true;
});
document.addEventListener('keyup', (e) => {
    if (e.code === 'KeyW') moveF = false;
    if (e.code === 'KeyS') moveB = false;
    if (e.code === 'KeyA') moveL = false;
    if (e.code === 'KeyD') moveR = false;
});

// --- 4. SHOOTING TARGETS ---
const raycaster = new THREE.Raycaster();
const centerMouse = new THREE.Vector2(0, 0); // Always point straight out the crosshair

document.addEventListener('mousedown', () => {
    if (document.pointerLockElement !== document.body) return;

    // Shoots an invisible math laser out your eyes to find hit items
    raycaster.setFromCamera(centerMouse, camera);
    const hits = raycaster.intersectObjects(enemies);

    if (hits.length > 0) {
        const hitTarget = hits[0].object;
        scene.remove(hitTarget); // Wipes the cube from existence
        enemies.splice(enemies.indexOf(hitTarget), 1);
        
        score += 25; // Gain rank score points!
        document.getElementById('score').innerText = score;
        
        // Spawn a replacement enemy box shortly after
        setTimeout(spawnEnemy, 1000);
    }
});

// --- 5. CORE GAME LOOP (RUNS 60 TIMES A SECOND) ---
const clock = new THREE.Clock();
camera.position.set(0, 2, 5); // Stand up at eye level on the map

function animate() {
    requestAnimationFrame(animate);
    const dt = clock.getDelta();
    
    if (document.pointerLockElement === document.body) {
        const speed = 8 * dt;
        
        // Find which way the camera is looking on the flat ground
        const forward = new THREE.Vector3(0, 0, -1).applyQuaternion(camera.quaternion);
        forward.y = 0; forward.normalize();
        const right = new THREE.Vector3(1, 0, 0).applyQuaternion(camera.quaternion);
        right.y = 0; right.normalize();

        // Adjust position based on inputs
        if (moveF) camera.position.addScaled(forward, speed);
        if (moveB) camera.position.addScaled(forward, -speed);
        if (moveL) camera.position.addScaled(right, -speed);
        if (moveR) camera.position.addScaled(right, speed);
    }
    
    // Spin the enemy cubes slightly so they look active
    enemies.forEach(enemy => {
        enemy.rotation.y += 0.01;
    });

    renderer.render(scene, camera);
}

// Window sizing auto-fix
window.addEventListener('resize', () => {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
});

animate();
