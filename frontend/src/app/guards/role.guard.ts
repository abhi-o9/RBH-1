import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';

export const RoleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {

  const router = inject(Router);

  const requiredRole = route.data['role'];
  const userRole = sessionStorage.getItem('role');

  // If no role stored, redirect to login
  if (!userRole) {
    router.navigate(['/login']);
    return false;
  }

  // If role matches → allow access
  if (userRole.toLowerCase() === requiredRole.toLowerCase()) {
    return true;
  }

  // Otherwise redirect to dashboard
  router.navigate(['/dashboard']);
  return false;
};
