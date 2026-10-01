import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/home/home.component')
        .then(m => m.HomeComponent)
  },
  {
    path: 'food/:foodCode',
    loadComponent: () =>
      import('./features/food-details/food-details.component')
        .then(m => m.FoodDetailsComponent)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
