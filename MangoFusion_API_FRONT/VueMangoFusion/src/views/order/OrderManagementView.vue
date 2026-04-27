<template>
  <div class="container-fluid py-2">
    <!-- <h1 class="mb-4">Order Management</h1> -->
    <p class="text-success h2 pb-1">Order Management</p>
    <!-- Filters -->
    <div class="card border-0 shadow-sm p-4 mb-4">
      <div class="row">
        <div class="col-md-4 mb-3">
          <label class="form-label">Filter by Status</label>
          <select class="form-select" v-model="statusFilter">
            <option value="">All Status</option>
            <option v-for="status in ORDER_STATUS" :key="status" :value="status">
              {{ status }}
            </option>
          </select>
        </div>
        <div class="col-md-4 mb-3">
          <label class="form-label">Sort By</label>
          <select class="form-select" v-model="sortBy">
            <option value="orderHeaderId">Order ID</option>
            <option value="orderTotal">Total Amount</option>
            <option value="pickUpName">Customer Name</option>
          </select>
        </div>
        <div class="col-md-4 mb-3">
          <label class="form-label">Sort Direction</label>
          <select class="form-select" v-model="sortDirection">
            <option value="asc">Ascending</option>
            <option value="desc">Descending</option>
          </select>
        </div>
      </div>
      <div class="row mt-2">
        <div class="col-md-8 mb-3">
          <label class="form-label">Search</label>
          <input
            type="text"
            class="form-control"
            placeholder="Search by name, email or phone"
            v-model="searchQuery"
          />
        </div>
        <div class="col-md-4 mb-3 d-flex align-items-end">
          <button class="btn btn-outline-secondary w-100" @click="resetFilters">
            Reset Filters
          </button>
        </div>
      </div>
    </div>

    <div class="text-center py-4 fs-5 text-body-secondary" v-if="loading">Loading orders...</div>
    <div class="text-center py-5 card border-0 shadow-sm" v-else-if="fileredOrders.length === 0">
      <p class="mb-0">No orders found matching your criteria.</p>
    </div>
    <div>
      <div class="mb-3">
        <span class="badge bg-success">{{ fileredOrders.length }} orders found</span>
      </div>
      <div class="table-responsive card border-0 shadow-sm">
        <table class="table table-hover mb-0">
          <thead>
            <tr>
              <th style="cursor: pointer" @click="updateSort('orderHeaderId')">
                Order ID
                <span class="ms-1" v-if="sortBy === 'orderHeaderId'">
                  {{ sortDirection === 'asc' ? '↑' : '↓' }}
                </span>
              </th>
              <th style="cursor: pointer" @click="updateSort('pickUpName')">
                Pick Up Name
                <span class="ms-1" v-if="sortBy === 'pickUpName'">
                  {{ sortDirection === 'asc' ? '↑' : '↓' }}
                </span>
              </th>
              <th>Contact</th>
              <th>Number of Items</th>
              <th style="cursor: pointer" @click="updateSort('orderTotal')">
                Total
                <span class="ms-1" v-if="sortBy === 'orderTotal'">
                  {{ sortDirection === 'asc' ? '↑' : '↓' }}
                </span>
              </th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody v-for="order in paginatedOrders" :key="order.orderHeaderId">
            <tr>
              <td>#{{ order.orderHeaderId }}</td>
              <td>{{ order.pickUpName }}</td>
              <td>
                <div>{{ order.pickUpPhoneNumber }}</div>
                <div class="text-body-secondary small">{{ order.pickUpEmail }}</div>
              </td>

              <td>{{ order.totalItem }}</td>
              <td>${{ order.orderTotal }}</td>
              <td>
                <div
                  class="badge rounded-pill"
                  :class="{
                    'bg-warning-subtle text-warning-emphasis':
                      order.status === ORDER_STATUS_CONFIRMED,
                    'bg-info-subtle text-info-emphasis':
                      order.status === ORDER_STATUS_READY_FOR_PICKUP,
                    'bg-success-subtle text-success-emphasis':
                      order.status === ORDER_STATUS_COMPLETED,
                    'bg-danger-subtle text-danger-emphasis':
                      order.status === ORDER_STATUS_CANCELLED,
                  }"
                >
                  {{ order.status }}
                </div>
              </td>
              <td>
                <button class="btn btn-sm btn-success" @click="viewOrderDetails(order)">
                  <i class="bi bi-card-checklist"></i> &nbsp;View Details
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <nav aria-label="Order pagination" class="mt-4 d-flex justify-content-end">
        <ul class="pagination pagination-md">
          <!-- First page button -->
          <li class="page-item">
            <a
              class="page-link text-success border-success"
              href="#"
              aria-label="First"
              @click="changePage(1)"
            >
              <span aria-hidden="true">&laquo;</span>
              <span class="visually-hidden">First page</span>
            </a>
          </li>

          <!-- Previous button -->
          <li class="page-item">
            <a
              class="page-link text-success border-success"
              href="#"
              aria-label="Previous"
              @click="changePage(currentPage.value - 1)"
            >
              <span aria-hidden="true">&lsaquo;</span>
              <span class="visually-hidden">Previous page</span>
            </a>
          </li>

          <!-- Page numbers with limited display -->
          <template v-for="pagNum in displayedPageNumber" :key="pagNum">
            <li class="page-item disabled" v-if="pagNum === '...'">
              <span class="page-link border-success">...</span>
            </li>
            <li class="page-item" v-else>
              <a
                class="page-link border-success"
                href="#"
                @click="changePage(pagNum)"
                :class="
                  pagNum === currentPage
                    ? 'bg-success border-success text-white'
                    : 'text.success border-success'
                "
              >
                {{ pagNum }}
              </a>
            </li>
          </template>
          <!-- Next button -->
          <li class="page-item">
            <a
              class="page-link text-success border-success"
              href="#"
              aria-label="Next"
              @click="changePage(currentPage.value + 1)"
            >
              <span aria-hidden="true">&rsaquo;</span>
              <span class="visually-hidden">Next page</span>
            </a>
          </li>

          <!-- Last page button -->
          <li class="page-item">
            <a
              class="page-link text-success border-success"
              href="#"
              aria-label="Last"
              @click="changePage(totalPages.value)"
            >
              <span aria-hidden="true">&raquo;</span>
              <span class="visually-hidden">Last page</span>
            </a>
          </li>
        </ul>
      </nav>
    </div>

    <!-- Order Details Modal Component -->
    <OrderDetailsModal
      :order="selectedOrder"
      @close="closeOrderDetails"
      @status-updated="fetchOrders"
    />
  </div>
</template>

<script setup>
import OrderDetailsModal from '@/modals/OrderDetailsModal.vue';
import OrderListCard from '@/components/card/OrderListCard.vue';
import { APP_ROUTE_NAMES } from '@/constants/routeNames';
import orderService from '@/services/orderService';
import { useAuthStore } from '@/stores/authStore';
import { computed, onMounted, reactive, ref } from 'vue';
import {
  ORDER_STATUS,
  ORDER_STATUS_CONFIRMED,
  ORDER_STATUS_READY_FOR_PICKUP,
  ORDER_STATUS_COMPLETED,
  ORDER_STATUS_CANCELLED,
} from '@/constants/constants';

const orders = reactive([]);
const loading = ref(false);
const selectedOrder = ref(null);

//Filter and Sorting
const statusFilter = ref('');
const searchQuery = ref('');
const sortBy = ref('orderHeaderId');
const sortDirection = ref('desc');

//Pagination
const itemsPerPage = 5;
const currentPage = ref(1);

const resetFilters = () => {
  statusFilter.value = '';
  searchQuery.value = '';
  sortBy.value = 'orderHeaderId';
  sortDirection.value = 'desc';
  currentPage.value = 1;
};

const viewOrderDetails = (order) => {
  selectedOrder.value = { ...order };
  // Open the modal here (e.g., using a ref or an event)
};
const closeOrderDetails = () => {
  selectedOrder.value = null;
  // Open the modal here (e.g., using a ref or an event)
};

const updateSort = (field) => {
  if (sortBy.value === field) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc';
  } else {
    sortBy.value = field;
    sortDirection.value = 'asc';
  }
};

const fileredOrders = computed(() => {
  let result = [...orders];
  if (statusFilter.value) {
    result = result.filter(
      (order) => order.status.toUpperCase() === statusFilter.value.toUpperCase(),
    );
  }

  if (searchQuery.value) {
    const query = searchQuery.value.toUpperCase();
    result = result.filter(
      (order) =>
        order.pickUpName.toUpperCase().includes(query) ||
        order.pickUpEmail.toUpperCase().includes(query) ||
        order.pickUpPhoneNumber.toUpperCase().includes(query),
    );
  }
  result.sort((a, b) => {
    let compareA = a[sortBy.value];
    let compareB = b[sortBy.value];

    if (typeof compareA === 'string') {
      compareA = compareA.toUpperCase();
      compareB = compareB.toUpperCase();
    }

    if (compareA < compareB) return sortDirection.value === 'asc' ? -1 : 1;
    if (compareA > compareB) return sortDirection.value === 'asc' ? 1 : -1;
    return 0;
  });
  return result;
});

const totalPages = computed(() => Math.ceil(fileredOrders.value.length / itemsPerPage));
const paginatedOrders = computed(() => {
  const startIndex = (currentPage.value - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  return fileredOrders.value.slice(startIndex, endIndex);
});
const changePage = (page) => {
  if (page < 1 || page > totalPages.value) return;
  currentPage.value = page;
};

const displayedPageNumber = computed(() => {
  const total = totalPages.value;
  const current = currentPage.value;
  const delta = 1;
  if (total <= 5) {
    return Array.from({ length: total }, (_, i) => i + 1);
  }
  let range = [];

  range.push(1);
  const rangeStart = Math.max(2, current - delta);
  const rangeEnd = Math.min(total - 1, current + delta);
  if (rangeStart > 2) {
    range.push('...');
  }
  for (let i = rangeStart; i < rangeEnd; i++) {
    range.push(i);
  }
  if (rangeEnd < total - 1) {
    range.push('...');
  }
  if (total > 1) {
    range.push(total);
  }
  return range;
});

const fetchOrders = async () => {
  orders.length = 0;
  loading.value = true;
  try {
    var result = await orderService.getOrders();
    orders.push(...result);
    console.log(result);
  } catch (error) {
    console.log('Error fetch orders:', error);
  } finally {
    loading.value = false;
  }
};
onMounted(fetchOrders);
</script>
<style scoped></style>
