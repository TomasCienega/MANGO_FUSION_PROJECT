import api from './api';

export default {
  async getMenuItems() {
    try {
      const response = await api.get('/MenuItem');
      if (response.data.isSuccess) {
        return response.data.result;
      } else {
        throw new Error('Failed to fetch menu items ');
      }
    } catch (error) {
      console.error('Error fetching menu items:', error);
      throw error;
    }
  },
  async getMenuItemById(id) {
    try {
      const response = await api.get(`/MenuItem/${id}`);
      if (response.data.isSuccess) {
        return response.data.result;
      } else {
        throw new Error('Failed to fetch menu item ');
      }
    } catch (error) {
      console.error('Error fetching menu item:', error);
      throw error;
    }
  },
  async createMenuItem(data) {
    try {
      const response = await api.post('/MenuItem', data);
      if (response.data.isSuccess) {
        return response.data.result;
      } else {
        throw new Error('Failed to create menu items ');
      }
    } catch (error) {
      console.error('Error creating menu items:', error);
      throw error;
    }
  },
  async updateMenuItem(id, data) {
    try {
      const response = await api.put(`/MenuItem/${id}`, data);
      if (response.data.isSuccess) {
        return response.data.result;
      } else {
        throw new Error('Failed to update menu item ');
      }
    } catch (error) {
      console.error('Error updating menu item:', error);
      throw error;
    }
  },
  async deleteMenuItem(id) {
    try {
      const response = await api.delete(`/MenuItem/${id}`);
      if (response.data.isSuccess) {
        return response.data.result;
      } else {
        throw new Error('Failed to delete menu item ');
      }
    } catch (error) {
      console.error('Error deleting menu item:', error);
      throw error;
    }
  },
};
