import { createApp } from "vue";
import { createPinia } from "pinia";
import "./styles/main.css";
import App from "./App.vue";
import router from "./router";
import { msalInstance } from "./auth/msal/msalConfig";
import { msalPlugin } from "./auth/msal/msalPlugin";

const app = createApp(App);
app.use(createPinia());
app.use(msalPlugin, msalInstance);
app.use(router);
await msalInstance.initialize();
app.mount("#app");
