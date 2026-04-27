<template>
  <div class="container mt-5">
    <div class="row justify-content-center">
      <div class="col-md-10">
        <div class="card shadow d-flex flex-row">
          <img
            src="@/assets/hero.jpg"
            class="card-img-left img-fluid"
            style="width: 50%"
            object-fit="cover"
          />
          <div class="card-body p-5" style="width: 50%">
            <h2 class="text-center mb-4">Sign In</h2>
            <form @submit.prevent="onSignInSubmit">
              <div class="mb-3">
                <label for="email" class="form-label">Email</label>
                <input type="email" class="form-control" id="email" v-model="formObj.email" />
              </div>

              <div class="mb-3">
                <label for="password" class="form-label">Password</label>
                <input
                  type="password"
                  class="form-control"
                  id="password"
                  v-model="formObj.password"
                />
              </div>

              <div class="alert alert-danger" v-if="errorList.length > 0">
                <span class="d-block" v-for="error in errorList" :key="error"> {{ error }} </span>
              </div>

              <button :disabled="isLoading" type="submit" class="btn btn-success w-100">
                <span class="spinner-border spinner-border-sm me-2" v-if="isLoading"></span>
                Login
              </button>
            </form>

            <div class="text-center mt-3">
              <router-link :to="APP_ROUTE_NAMES.SIGN_UP"
                >Don't have an account? Sign up</router-link
              >
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { APP_ROUTE_NAMES } from '@/constants/routeNames';
import { reactive, ref } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import { useRouter } from 'vue-router';

const errorList = reactive([]);
const authStore = useAuthStore();
const router = useRouter();
const formObj = reactive({
  email: '',
  password: '',
});
const isLoading = ref(false);

const onSignInSubmit = async () => {
  errorList.length = 0;
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  // 1. Validaciones
  if (!formObj.email) errorList.push('Email is required.');
  if (!formObj.password) errorList.push('Password is required.');
  if (formObj.email && !emailPattern.test(formObj.email)) {
    errorList.push('Please enter a valid email address.');
  }

  // Si hay errores, salimos de la función AQUÍ.
  // Como todavía no activamos isSubmitting, el botón sigue normal.
  if (errorList.length > 0) {
    isLoading.value = false;
    return;
  }
  isLoading.value = true;

  try {
    const response = await authStore.signIn(formObj);

    if (response.success) {
      console.log('¡Inicio de sesión exitoso!');
      // Aquí podrías redirigir si el store no lo hace
      router.push({ name: APP_ROUTE_NAMES.HOME });
    } else {
      // 1. Limpiamos por si acaso
      errorList.length = 0;

      // 2. Si el mensaje trae varios errores separados por \n, los separamos
      if (response.message) {
        const messages = response.message.split('\n');
        messages.forEach((msg) => {
          if (msg.trim()) errorList.push(msg); // Agregamos cada uno por separado
        });
      } else {
        errorList.push('Login failed.');
      }
    }
  } catch (err) {
    console.error('Error:', err);
    errorList.push('An unexpected error occurred.');
  } finally {
    isLoading.value = false;
  }
};
</script>
<style scoped></style>
