import { Routes } from '@angular/router';

export const CATEGORIES_ROUTES: Routes = [
  {
    path: '',
    title: 'Categories',
    loadComponent: () =>
      import('./categories-page/categories-page.component').then((m) => m.CategoriesPageComponent),
  },
];
