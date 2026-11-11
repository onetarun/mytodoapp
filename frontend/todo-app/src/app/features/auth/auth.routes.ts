import { Routes } from '@angular/router';

export const LOGIN_ROUTES: Routes = [
  {
    path: '',
    title: 'Log in',
    loadComponent: () =>
      import('./login-page/login-page.component').then((m) => m.LoginPageComponent),
  },
];

export const REGISTER_ROUTES: Routes = [
  {
    path: '',
    title: 'Register',
    loadComponent: () =>
      import('./register-page/register-page.component').then((m) => m.RegisterPageComponent),
  },
];
