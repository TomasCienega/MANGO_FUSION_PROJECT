import authService from '@/services/authService';
import { defineStore } from 'pinia';
import { computed, reactive, ref } from 'vue';
import { useSwal } from '@/composables/useSwal';
import Cookies from 'js-cookie';
import { APP_ROUTE_NAMES } from '@/constants/routeNames';
import router from '@/router';
export const useAuthStore = defineStore('authStore', () => {
  //STATE
  const user = reactive({
    email: '',
    password: '',
    name: '',
    id: '',
    // isLoggedIn: false,
  });

  const isAuthenticated = ref(false);

  //getter
  const getUserInfo = computed(() => {
    return isAuthenticated.value ? user : null;
  });
  const isAdmin = computed(() => {
    return isAuthenticated.value && user.role === 'Admin';
  });
  //actions

  function decodeToken(token) {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return {
      email: payload.email,
      role: payload.role,
      name: payload.fullname,
      id: payload.id,
    };
  }
  function initialize() {
    try {
      const token = Cookies.get('token_mango');
      if (token) {
        const userData = decodeToken(token);
        if (userData) {
          Object.assign(user, userData);
          isAuthenticated.value = true;
        } else {
          clearAuthData();
        }
      } else {
        clearAuthData();
      }
    } catch (error) {
      console.error('Error initializing auth store:', error);
    }
  }

  async function signUp(userData) {
    try {
      const res = await authService.signUp(userData);
      const { showSuccess } = useSwal();
      showSuccess('Registration successful!');

      return res;
    } catch (error) {
      const serverErrorResponse = error.response?.data;

      // En lugar de [0], usamos .join para juntar todos los errores del array
      const allErrors = serverErrorResponse?.errorMessage?.join('\n') || 'Registration failed.';

      console.log('Mensajes capturados:', allErrors);

      return {
        success: false,
        message: allErrors, // Ahora enviamos la lista completa
      };
    }
  }
  async function signIn(formObj) {
    try {
      const { token, user: userData } = await authService.signIn(formObj);
      localStorage.setItem('token', token);
      Object.assign(user, userData);
      // user.isLoggedIn = true;
      isAuthenticated.value = true;

      Cookies.set('token_mango', token, { expires: 7 });

      return { success: true };
    } catch (error) {
      const serverErrorResponse = error.response?.data;

      // En lugar de [0], usamos .join para juntar todos los errores del array
      const allErrors = serverErrorResponse?.errorMessage?.join('\n') || 'Login failed.';

      console.log('Mensajes capturados:', allErrors);

      return {
        success: false,
        message: allErrors, // Ahora enviamos la lista completa
      };
    }
  }
  function clearAuthData() {
    Object.assign(user, {
      email: '',
      password: '',
      name: '',
      id: '',
    });
    isAuthenticated.value = false;
    Cookies.remove('token_mango');
  }
  function signOut() {
    clearAuthData();
    router.push({ name: APP_ROUTE_NAMES.HOME });
  }

  return {
    user,
    isAuthenticated,
    getUserInfo,
    signUp,
    signIn,
    initialize,
    signOut,
    isAdmin,
  };
});
