import Swal from 'sweetalert2';

export function useSwal() {
  const showAlert = async (options) => {
    return await Swal.fire(options);
  };
  const showSuccess = async (message) => {
    return await showAlert({
      position: 'top-end',
      icon: 'success',
      title: message,
      showConfirmButton: false,
      timer: 2000,
    });
  };
  const showError = async (message) => {
    return await showAlert({
      position: 'top-end',
      icon: 'error',
      title: message,
      showConfirmButton: false,
      timer: 2000,
    });
  };
  const showConfirm = async (message) => {
    return await showAlert({
      title: 'Are you sure?',
      text: message,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Yes, delete it!',
    });
  };
  return { showSuccess, showError, showConfirm };
}
