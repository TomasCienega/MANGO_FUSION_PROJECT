import { createApp } from 'vue';
import { createPinia } from 'pinia';
import { useThemeStore } from './stores/themeStore';
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate';

import App from './views/App.vue';
import router from './router';
// Importamos el CSS de Bootstrap
import 'bootstrap/dist/css/bootstrap.min.css';
// Importamos los Iconos de Bootstrap
import 'bootstrap-icons/font/bootstrap-icons.css';
// Importamos el JS de Bootstrap (para que funcionen modales, dropdowns, etc.)
import 'bootstrap/dist/js/bootstrap.bundle.min.js';
//npm para instalar bootstrap
// npm install bootstrap bootstrap-icons @popperjs/core
const app = createApp(App);
const pinia = createPinia();
pinia.use(piniaPluginPersistedstate);
app.use(pinia);
app.use(router);
const themeStore = useThemeStore();
if (themeStore.theme) {
  document.body.setAttribute('data-bs-theme', themeStore.theme);
}
app.mount('#app');
