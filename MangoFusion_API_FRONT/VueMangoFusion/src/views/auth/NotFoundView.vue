<template>
  <div class="container mt-3">
    <h1>NOT FOUND</h1>
  </div>
</template>

<script setup>
import { useRoute } from 'vue-router'; // 1. Importamos la función

const route = useRoute(); // 2. Obtenemos la ruta actual
const rutaErronea = route.params.catchAll;

console.log('El usuario intentó entrar a: ' + rutaErronea);
</script>

<style scoped></style>
