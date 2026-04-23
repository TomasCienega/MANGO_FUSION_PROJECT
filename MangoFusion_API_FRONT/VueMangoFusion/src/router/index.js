import NoAccessView from '@/views/auth/NoAccessView.vue';
import NotFoundView from '@/views/auth/NotFoundView.vue';
import SignInView from '@/views/auth/SignInView.vue';
import SignUpView from '@/views/auth/SignUpView.vue';
import ShoppingCartView from '@/views/cart/ShoppingCartView.vue';
import HomeView from '@/views/home/HomeView.vue';
import MenuItemListView from '@/views/menu-item/MenuItemListView.vue';
import MenuItemUpsertView from '@/views/menu-item/MenuItemUpsertView.vue';
import OrderConfirmationView from '@/views/order/OrderConfirmationView.vue';
import OrderHistoryListView from '@/views/order/OrderHistoryListView.vue';
import OrderManagementView from '@/views/order/OrderManagementView.vue';

import { APP_ROUTE_NAMES } from '@/constants/routeNames';

import { createRouter, createWebHistory } from 'vue-router';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: APP_ROUTE_NAMES.HOME,
      component: HomeView,
    },
    {
      path: '/no-access',
      name: APP_ROUTE_NAMES.ACCESS_DENIED,
      component: NoAccessView,
    },
    {
      path: '/sign-in',
      name: APP_ROUTE_NAMES.SIGN_IN,
      component: SignInView,
    },
    {
      path: '/sign-up',
      name: APP_ROUTE_NAMES.SIGN_UP,
      component: SignUpView,
    },
    {
      path: '/cart',
      name: APP_ROUTE_NAMES.CART,
      component: ShoppingCartView,
    },
    {
      path: '/admin/manage-menu-items',
      name: APP_ROUTE_NAMES.MENU_ITEM_LIST,
      component: MenuItemListView,
    },
    {
      path: '/admin/manage-menu-items/create',
      name: APP_ROUTE_NAMES.CREATE_MENU_ITEM,
      component: MenuItemUpsertView,
    },
    {
      path: '/admin/manage-menu-items/update/:id',
      name: APP_ROUTE_NAMES.EDIT_MENU_ITEM,
      component: MenuItemUpsertView,
      props: true,
    },
    {
      path: '/admin/order-confirmation/:orderId',
      name: APP_ROUTE_NAMES.ORDER_CONFIRM,
      component: OrderConfirmationView,
      props: true,
    },
    {
      path: '/order-list',
      name: APP_ROUTE_NAMES.ORDER_LIST,
      component: OrderHistoryListView,
    },
    {
      path: '/admin/order-management',
      name: APP_ROUTE_NAMES.MANAGE_ORDER_ADMIN,
      component: OrderManagementView,
    },
    {
      path: '/:catchAll(.*)',
      name: APP_ROUTE_NAMES.NOT_FOUND,
      component: NotFoundView,
    },
  ],
});

export default router;
