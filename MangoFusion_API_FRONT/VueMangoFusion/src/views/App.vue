<template>
  <div class="d-flex flex-column py-2 min-vh-100 container-md">
    <AppHeader />
    <main class="flex-grow-1 mx-lg-auto w-100">
      <RouterView />
    </main>

    <AppFooter />
  </div>
</template>

<script setup>
import AppFooter from '@/components/layout/AppFooter.vue';
import AppHeader from '@/components/layout/AppHeader.vue';
import { RouterView } from 'vue-router';
</script>
<style scoped></style>
