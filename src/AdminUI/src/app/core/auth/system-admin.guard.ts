import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/**
 * Chặn truy cập nếu người dùng không có vai trò SystemAdministrator.
 */
export const systemAdminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isSystemAdministrator()) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};
