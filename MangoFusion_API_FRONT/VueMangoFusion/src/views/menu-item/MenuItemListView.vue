<template>
  <div class="container px-3">
    <div v-if="loading" class="d-flex justify-content-center align-items-center vh-100">
      <div class="spinner-grow text-success" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <div v-else>
      <div>
        <div
          class="card-header d-flex flex-column flex-md-row justify-content-between align-items-md-center p-3"
        >
          <div>
            <h2 class="h5 text-success">Menu Items</h2>
            <p class="mb-0 text-muted small">Manage your restaurant's offerings</p>
          </div>
          <button
            class="btn btn-success btn-sm gap-2 rounded-1 px-4 py-2"
            @click="router.push({ name: APP_ROUTE_NAMES.CREATE_MENU_ITEM })"
          >
            <i class="bi bi-plus-square"></i> &nbsp;
            <span>Add Item</span>
          </button>
        </div>
        <div class="card-body p-3">
          <div class="table-responsive">
            <table class="table table-hover align-middle mb-0">
              <thead>
                <tr>
                  <th class="ps-3 small text-muted">Item</th>
                  <th class="small text-muted">Category</th>
                  <th class="small text-muted">Price</th>
                  <th class="small text-muted">Tag</th>
                  <th class="pe-3 text-end small text-muted">Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="menuItem in menuItems" :key="menuItem.id">
                  <td class="ps-3">
                    <div class="d-flex align-items-center">
                      <img
                        :src="CONFIG_IMAGE_URL + menuItem.image"
                        alt="Item"
                        class="rounded object-fit-cover me-2"
                        style="width: 50px; height: 50px"
                      />
                      <div>
                        <div class="fw-semibold small">{{ menuItem.name }}</div>
                      </div>
                    </div>
                  </td>
                  <td>
                    <span class="badge bg-success bg-opacity-10 text-success small">
                      {{ menuItem.category }}
                    </span>
                  </td>
                  <td class="fw-semibold small">{{ menuItem.price.toFixed(2) }}</td>
                  <td>
                    <span class="badge bg-info bg-opacity-10 text-info small">
                      {{ menuItem.specialTag }}
                    </span>
                  </td>
                  <td class="pe-3 text-end">
                    <div class="d-flex gap-2 justify-content-end">
                      <button
                        class="btn btn-sm btn-outline-success"
                        @click="
                          router.push({
                            name: APP_ROUTE_NAMES.EDIT_MENU_ITEM,
                            params: { id: menuItem.id },
                          })
                        "
                      >
                        <i class="bi bi-pencil-square"></i>
                      </button>
                      <button
                        class="btn btn-sm btn-outline-danger"
                        @click="handleMenuItemDelete(menuItem.id)"
                      >
                        <i class="bi bi-trash3-fill"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import menuItemService from '@/services/menuItemService';
import { ref, onMounted, reactive } from 'vue';
import { APP_ROUTE_NAMES } from '@/constants/routeNames';
import { CONFIG_IMAGE_URL } from '@/constants/config';
import { useRouter } from 'vue-router';
import { useSwal } from '@/composables/useSwal';

const { showSuccess, showError, showConfirm } = useSwal();
const menuItems = reactive([]);
const loading = ref(false);
const router = useRouter();
const fetchMenuItems = async () => {
  loading.value = true;
  try {
    var result = await menuItemService.getMenuItems();
    menuItems.length = 0;
    menuItems.push(...result);
  } catch (error) {
    console.log('Error fetch menu items:', error);
  } finally {
    loading.value = false;
  }
};
onMounted(fetchMenuItems);

const handleMenuItemDelete = async (id) => {
  // loading.value = true; // Quita esto si quieres que el borrado sea "instantáneo" visualmente
  try {
    const confirmResult = await showConfirm('Are you sure you want to delete this MenuItem?');

    if (confirmResult.isConfirmed) {
      loading.value = true;
      // 1. Borramos en el Back-end
      await menuItemService.deleteMenuItem(id);

      // 2. Borrado local (Para REACTIVE)
      // Como es reactive, no usamos .value. Simplemente filtramos y reemplazamos el contenido.
      const index = menuItems.findIndex((item) => item.id === id);
      if (index !== -1) {
        menuItems.splice(index, 1); // Esto quita el elemento de la lista reactive correctamente
      }

      // 3. Avisamos al usuario
      await showSuccess('Menu Item deleted successfully');

      // 4. Refrescamos por si acaso
      await fetchMenuItems();
    }
  } catch (error) {
    console.error('Error deleting menu item:', error);
    await showError('Something went wrong trying to delete this item');
  } finally {
    loading.value = false;
  }
};
</script>
<style scoped></style>
