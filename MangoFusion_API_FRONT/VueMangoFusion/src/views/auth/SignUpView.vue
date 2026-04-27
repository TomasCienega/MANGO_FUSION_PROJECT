<template>
  <div class="container mt-5">
    <div class="row justify-content-center">
      <div class="col-md-10">
        <div class="card shadow d-flex flex-row">
          <img
            src="@/assets/confirm.jpg"
            class="card-img-left img-fluid"
            style="width: 50%"
            object-fit="cover"
          />
          <div class="card-body p-5" style="width: 50%">
            <h2 class="text-center mb-4">Sign Up</h2>

            <form @submit.prevent="onSignUpSubmit">
              <div class="mb-3">
                <label for="name" class="form-label">Full Name</label>
                <input type="text" class="form-control" id="name" v-model="formObj.name" />
              </div>

              <div class="mb-3">
                <label for="email" class="form-label">Email</label>
                <input type="email" class="form-control" id="email" v-model="formObj.email" />
              </div>

              <div class="mb-3">
                <label for="role" class="form-label">Role</label>
                <select class="form-select" id="role" v-model="formObj.role">
                  <option value="">--Select Role--</option>
                  <option v-for="role in ROLES" :key="role" :value="role">
                    {{ role }}
                  </option>
                </select>
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
                <span class="d-block" v-for="(error, index) in errorList" :key="index">
                  {{ error }}
                </span>
              </div>

              <button :disabled="isLoading" type="submit" class="btn btn-secondary w-100">
                <span class="spinner-border spinner-border-sm me-2" v-if="isLoading"></span>
                Sign Up
              </button>
            </form>

            <div class="text-center mt-3">
              <router-link :to="APP_ROUTE_NAMES.SIGN_IN">
                Already have an account? Login
              </router-link>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ROLES } from '@/constants/constants';
import { APP_ROUTE_NAMES } from '@/constants/routeNames';
import { reactive, ref } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const errorList = reactive([]);
const router = useRouter();

const formObj = reactive({
  name: '',
  email: '',
  role: '',
  password: '',
});
const isLoading = ref(false);

const onSignUpSubmit = async () => {
  isLoading.value = true;
  errorList.length = 0;
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  // 1. Validaciones
  if (!formObj.name) errorList.push('Name is required.');
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
  try {
    const response = await authStore.signUp(formObj);

    if (response.success) {
      console.log('¡Registro exitoso!');
      // Aquí podrías redirigir si el store no lo hace
      router.push({ name: APP_ROUTE_NAMES.SIGN_IN });
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
        errorList.push('Registration failed.');
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
