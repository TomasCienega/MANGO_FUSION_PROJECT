<template>
  <div class="d-flex justify-content-center align-items-center" v-if="loading">
    <div class="spinner-grow text-success" style="width: 2.5rem; height: 2.5rem" role="status">
      <span class="visually-hidden">Loading...</span>
    </div>
  </div>

  <div class="container" v-else>
    <div class="mx-auto">
      <div class="mb-4 border-bottom d-flex justify-content-between align-items-center py-3">
        <h3 class="fw-semibold text-success">Add Menu</h3>
        <div class="d-flex gap-3">
          <button
            type="submit"
            form="menuForm"
            class="btn btn-success btn-sm gap-2 rounded-1 px-4 py-2"
            :disabled="isProcessing"
          >
            <span v-if="isProcessing" class="spinner-border spinner-border-sm me-2"></span>
            {{ menuItemIdForUpdate ? 'Update' : 'Create' }} Item
          </button>

          <button
            type="button"
            class="btn btn-outline border btn-sm gap-2 rounded-1 px-4 py-2"
            @click="router.push({ name: APP_ROUTE_NAMES.MENU_ITEM_LIST })"
          >
            Cancel
          </button>
        </div>
      </div>
      <div class="alert alert-danger pb-0" v-if="errorList.length > 0">
        Please fix the following errors:
        <ul>
          <li v-for="error in errorList" :key="error">{{ error }}</li>
        </ul>
      </div>
      <form
        enctype="multipart/form-data"
        class="needs-validation"
        id="menuForm"
        @submit="onFormSubmit"
      >
        <div class="row g-4">
          <div class="col-lg-7">
            <div class="d-flex flex-column g-12">
              <div class="mb-3">
                <label for="name" class="form-label">Item Name</label>
                <input
                  id="name"
                  type="text"
                  v-model="menuItemObj.name"
                  @input="clearError('Name should be at least 3 char long')"
                  class="form-control"
                  placeholder="Enter item name"
                />
              </div>

              <div class="mb-3">
                <label for="description" class="form-label">Description</label>
                <textarea
                  id="description"
                  class="form-control"
                  placeholder="Describe the menu item..."
                  rows="3"
                  v-model="menuItemObj.description"
                ></textarea>
              </div>

              <div class="mb-3">
                <label for="specialTag" class="form-label">Special Tag (Optional)</label>
                <input
                  id="specialTag"
                  type="text"
                  class="form-control"
                  placeholder="e.g., Chef's Special"
                  v-model="menuItemObj.specialTag"
                />
              </div>

              <div class="mb-3">
                <label for="category" class="form-label">Category</label>
                <select
                  id="category"
                  class="form-select"
                  v-model="menuItemObj.category"
                  @change="clearError('Category must be selected')"
                >
                  <option value="" selected disabled>--Select a Category--</option>
                  <option v-for="category in CATEGORIES" :key="category">{{ category }}</option>
                </select>
              </div>

              <div class="mb-3">
                <label for="price" class="form-label">Price</label>
                <input
                  id="price"
                  class="form-control"
                  v-model="menuItemObj.price"
                  @input="clearError('Price should be greater than 0')"
                />
              </div>
            </div>
          </div>

          <div class="col-lg-5">
            <div>
              <img
                v-if="newUploadedImage_base64 || (menuItemObj.image && menuItemObj.image !== '')"
                :src="
                  newUploadedImage_base64 !== ''
                    ? newUploadedImage_base64
                    : CONFIG_IMAGE_URL + menuItemObj.image
                "
                class="img-fluid w-100 mb-3 rounded"
              />
              <div class="mb-3">
                <label for="image" class="form-label">Item Image</label>
                <input
                  id="image"
                  type="file"
                  class="form-control"
                  accept="image/*"
                  @change="handleFileChange"
                />
                <div class="form-text">Leave empty to keep existing image</div>
              </div>
            </div>
          </div>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, reactive } from 'vue';
import { APP_ROUTE_NAMES } from '@/constants/routeNames';
import { CONFIG_IMAGE_URL } from '@/constants/config';
import { useRoute, useRouter } from 'vue-router';
import { CATEGORIES } from '@/constants/constants';
import menuItemService from '@/services/menuItemService';
import { useSwal } from '@/composables/useSwal';

const { showSuccess, showError } = useSwal();

const router = useRouter();
const route = useRoute();

const menuItemIdForUpdate = route.params.id;

const loading = ref(false);
const isProcessing = ref(false);
const errorList = reactive([]);
const newUploadedImage = ref(null);
const newUploadedImage_base64 = ref('');

const menuItemObj = reactive({
  name: '',
  description: '',
  specialTag: '',
  category: '',
  price: 0.0,
  image: '',
});
const clearError = (message) => {
  const index = errorList.indexOf(message);
  if (index > -1) {
    errorList.splice(index, 1);
  }
};
onMounted(async () => {
  if (!menuItemIdForUpdate) return;
  loading.value = true;
  try {
    const result = await menuItemService.getMenuItemById(menuItemIdForUpdate);
    Object.assign(menuItemObj, result);
  } catch (err) {
    console.log('Error while fetching menu item', err);
  } finally {
    loading.value = false;
  }
});

const handleFileChange = async (event) => {
  isProcessing.value = true;
  const file = event.target.files[0];
  newUploadedImage.value = file;
  if (file) {
    clearError('Image must be uploaded');
    const reader = new FileReader();
    reader.onload = (e) => {
      newUploadedImage_base64.value = e.target.result;
      isProcessing.value = false;
    };
    reader.readAsDataURL(file);
  }
  isProcessing.value = false;
};

const onFormSubmit = async (event) => {
  event.preventDefault();
  const formData = new FormData(); // ¡Bien! Aquí está perfecto
  isProcessing.value = true;
  errorList.length = 0;

  // --- VALIDACIONES ---
  if (menuItemObj.name.length < 3) errorList.push('Name should be at least 3 char long');
  if (menuItemObj.price < 1) errorList.push('Price should be greater than 1');
  if (menuItemObj.category == '') errorList.push('Category must be selected');

  // La imagen solo es obligatoria si estamos CREANDO
  if (!menuItemIdForUpdate && !newUploadedImage.value) {
    errorList.push('Image must be uploaded');
  }

  // --- ENVÍO ---
  if (!errorList.length) {
    if (newUploadedImage.value) {
      formData.append('File', newUploadedImage.value);
    }

    Object.entries(menuItemObj).forEach(([key, value]) => {
      // FILTRO MÁGICO: Si la llave es 'image', NO la metas al formData
      // porque 'image' solo es el texto de la ruta vieja y confunde a .NET
      if (key !== 'image') {
        formData.append(key, value);
      }
    });

    // Asegúrate de que el ID vaya también en el cuerpo si usas Update
    if (menuItemIdForUpdate) {
      formData.append('Id', menuItemIdForUpdate);
    }

    try {
      if (menuItemIdForUpdate) {
        // --- LÓGICA DE ACTUALIZAR ---
        await menuItemService.updateMenuItem(menuItemIdForUpdate, formData);
        await showSuccess('Menu item updated successfully!');
      } else {
        // --- LÓGICA DE CREAR ---
        await menuItemService.createMenuItem(formData);

        await showSuccess('Menu item created successfully!');
      }

      // Después de éxito, mandamos al usuario de regreso a la lista
      router.push({ name: APP_ROUTE_NAMES.MENU_ITEM_LIST });
    } catch (err) {
      console.error('Operation failed', err);
      // En lugar de alert, usamos tu traductor de errores
      await showError('Ups! Something went wrong while saving.');
    }
  }

  // Se apaga al final de todo, sea éxito o error
  isProcessing.value = false;
};
// const onFormSubmit = async (event) => {
//   const formData = new FormData();
//   event.preventDefault();
//   isProcessing.value = true;
//   errorList.length = 0; //Limpiamos

//   if (menuItemObj.name.length < 3) {
//     errorList.push('Name should be at least 3 char long');
//   }
//   if (menuItemObj.price <= 0) {
//     errorList.push('Price should be greater than 0');
//   }
//   if (menuItemObj.category == '') {
//     errorList.push('Category must be selected');
//   }
//   if (newUploadedImage.value) {
//     //add to formData
//     formData.append('File', newUploadedImage.value);
//   } else {
//     errorList.push('Image must be uploaded');
//   }
//   if (!errorList.length) {
//     Object.entries(menuItemObj).forEach(([key, value]) => {
//       formData.append(key, value);
//     });
//     menuItemService
//       .createMenuItem(formData)
//       .then(() => {
//         alert('Menu item created');
//       })
//       .catch((err) => {
//         isProcessing.value = false;
//         console.log('Create failed', err);
//       });
//     console.log(menuItemObj);
//   }
//   isProcessing.value = false;
// };
</script>
<style scoped></style>
