// mqtt.js

// États globaux
window.mqttClient = null;
window.subscriptions = {};
let mqttConnected = false;
let mqttPingOk = false;
let pingIntervalId = null;

/**
 * Connexion MQTT + handlers
 */
window.connectMqtt = (brokerUrl, user, pass) => {
    if (window.mqttClient && mqttConnected) {
        console.warn("✅ Déjà connecté à MQTT.");
        return;
    }

    mqttConnected = false;
    mqttPingOk = false;

    console.log("🔌 Tentative de connexion MQTT :", brokerUrl);

    window.mqttClient = mqtt.connect(brokerUrl, {
        clientId: 'avalonia_' + Math.random().toString(16).substr(2, 8),
        username: user,
        password: pass,
        clean: true,
        reconnectPeriod: 0
    });

    window.mqttClient.on('connect', () => {
        mqttConnected = true;
        console.log('✅ MQTT connecté');
        if (globalThis.NotifyMqttConnected) {
            globalThis.NotifyMqttConnected();
        }
        startPingLoop();
    });

    window.mqttClient.on('close', () => {
        mqttConnected = false;
        console.log('🔌 MQTT socket fermé');
    });

    window.mqttClient.on('offline', () => {
        mqttConnected = false;
        console.log('📴 MQTT hors ligne');
    });

    window.mqttClient.on('error', err => {
        mqttConnected = false;
        console.error('❌ Erreur MQTT :', err);
        if (globalThis.NotifyMqttError) {
            globalThis.NotifyMqttError(err.message);
        }
    });

    window.mqttClient.on('message', (topic, message) => {
        const sub = window.subscriptions[topic];
        if (sub && typeof globalThis[sub.callback] === 'function') {
            globalThis[sub.callback](topic, message.toString(), sub.index);
        }
    });
};

/**
 * Déconnexion MQTT
 */
window.disconnectMqtt = () => {
    if (!window.mqttClient) {
        console.warn("MQTT déjà déconnecté.");
        return;
    }

    clearInterval(pingIntervalId);
    pingIntervalId = null;
    mqttPingOk = false;

    window.mqttClient.end(false, () => {
        console.log("🔌 MQTT déconnecté");
        if (globalThis.NotifyMqttDisconnected) {
            globalThis.NotifyMqttDisconnected();
        }
        // Reset
        window.mqttClient = null;
        window.subscriptions = {};
        mqttConnected = false;
    });
};

/**
 * Publication “momentary” (true puis false)
 */
window.publishMomentary = async (topic) => {
    if (!mqttConnected) {
        console.warn("⚠️ MQTT non connecté. Impossible de publier.");
        return;
    }
    try {
        window.mqttClient.publish(topic, "true", { qos: 1, retain: true });
        await new Promise(r => setTimeout(r, 500));
        window.mqttClient.publish(topic, "false", { qos: 1, retain: true });
    } catch (err) {
        console.error("Erreur publication momentary :", err);
    }
};

/**
 * Publication d’une valeur simple
 */
window.publishMqttValue = (topic, value) => {
    if (!mqttConnected) {
        console.warn("⚠️ MQTT non connecté. Impossible de publier.");
        return;
    }
    try {
        window.mqttClient.publish(topic, String(value), { qos: 1, retain: true });
    } catch (err) {
        console.error("Erreur publication MQTT :", err);
    }
};

/**
 * Abonnement à un topic MQTT
 */
window.subscribeToMqtt = (topic, callbackName, index) => {
    if (!mqttConnected) {
        console.warn("⚠️ MQTT non connecté. Impossible de s'abonner à :", topic);
        return;
    }
    if (!topic || typeof callbackName !== 'string') {
        console.warn("❌ Paramètres d'abonnement invalides.");
        return;
    }
    window.subscriptions[topic] = { callback: callbackName, index: index };
    window.mqttClient.subscribe(topic, { qos: 1 }, err => {
        if (err) console.error(`❌ Erreur abonnement à ${topic} :`, err);
        else console.log(`📡 Abonné à ${topic}`);
    });
};

/**
 * Ping périodique pour vérifier la santé de la connexion
 */
function startPingLoop() {
    mqttPingOk = true;
    clearInterval(pingIntervalId);

    pingIntervalId = setInterval(() => {
        if (!mqttConnected) {
            mqttPingOk = false;
            return;
        }
        window.mqttClient.publish('health/ping', 'ping', { qos: 1 }, err => {
            mqttPingOk = !err;
        });
    }, 10000);
}

/**
 * Getters pour supervision côté C#
 */
window.isMqttConnected = () => mqttConnected;
window.isMqttPingHealthy = () => mqttPingOk;
