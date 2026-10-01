import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  inject,
  signal,
  ViewChild
} from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { Router, RouterLink } from '@angular/router';

import {
  Subject,
  catchError,
  debounceTime,
  distinctUntilChanged,
  map,
  of,
  switchMap,
  tap
} from 'rxjs';

import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { CnfApiService } from '../../core/services/cnf-api.service';
import { FoodSearchResult, FoodSearchResponse } from '../../core/models/food.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent {

  private readonly api = inject(CnfApiService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  @ViewChild('searchInput')
  private readonly searchInput?: ElementRef<HTMLInputElement>;

  readonly query = signal('');
  readonly results = signal<FoodSearchResult[]>([]);
  readonly resultCount = signal(0);

  readonly loading = signal(false);
  readonly searched = signal(false);
  readonly error = signal('');

  readonly activeIndex = signal(-1);

  readonly minimumQueryLength = 2;
  readonly maxResults = 20;

  readonly quickSearches = [
    'Cheese souffle',
    'apple',
    'chicken',
    'salmon',
    'rice',
    'bread'
  ];

  private readonly searchTerms$ = new Subject<string>();

  constructor() {

    this.searchTerms$
      .pipe(
        map(value => value.trim()),
        debounceTime(250),
        distinctUntilChanged(),

        tap(term => {

          if (term.length < this.minimumQueryLength) {

            this.loading.set(false);
            this.searched.set(false);
            this.results.set([]);
            this.resultCount.set(0);
            this.error.set('');
            this.activeIndex.set(-1);

            return;
          }

          this.loading.set(true);
          this.searched.set(true);
          this.error.set('');
          this.activeIndex.set(-1);
        }),

        switchMap(term => {

          if (term.length < this.minimumQueryLength) {
            return of<FoodSearchResponse | null>(null);
          }

          return this.api.searchFoods(term, this.maxResults).pipe(
            catchError(() => {

              this.error.set(
                'We could not reach the nutrition database. Please check that the API is running and try again.'
              );

              return of<FoodSearchResponse | null>(null);
            })
          );
        }),

        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(response => {

        this.loading.set(false);

        if (response === null) {
          return;
        }

        this.results.set(response.items ?? []);
        this.resultCount.set(response.count ?? response.items?.length ?? 0);
        this.activeIndex.set(-1);
      });
  }

  onQueryInput(event: Event): void {

    const input = event.target as HTMLInputElement;

    this.query.set(input.value);

    this.searchTerms$.next(input.value);
  }

  searchNow(): void {

    const term = this.query().trim();

    if (term.length < this.minimumQueryLength) {

      this.focusSearch();

      return;
    }

    this.searchTerms$.next(term);
  }

  useQuickSearch(term: string): void {

    this.query.set(term);
    this.searchTerms$.next(term);

    queueMicrotask(() => {
      this.focusSearch();
    });
  }

  clearSearch(): void {

    this.query.set('');
    this.results.set([]);
    this.resultCount.set(0);
    this.loading.set(false);
    this.searched.set(false);
    this.error.set('');
    this.activeIndex.set(-1);

    queueMicrotask(() => {
      this.focusSearch();
    });
  }

  onSearchKeydown(event: KeyboardEvent): void {

    const items = this.results();

    if (event.key === 'ArrowDown') {

      if (items.length === 0) {
        return;
      }

      event.preventDefault();

      const next =
        this.activeIndex() < items.length - 1
          ? this.activeIndex() + 1
          : 0;

      this.activeIndex.set(next);

      return;
    }

    if (event.key === 'ArrowUp') {

      if (items.length === 0) {
        return;
      }

      event.preventDefault();

      const previous =
        this.activeIndex() > 0
          ? this.activeIndex() - 1
          : items.length - 1;

      this.activeIndex.set(previous);

      return;
    }

    if (event.key === 'Enter') {

      event.preventDefault();

      const index = this.activeIndex();

      if (index >= 0 && index < items.length) {

        this.openFood(items[index]);

        return;
      }

      this.searchNow();

      return;
    }

    if (event.key === 'Escape') {

      event.preventDefault();

      this.activeIndex.set(-1);

      return;
    }
  }

  openFood(food: FoodSearchResult): void {

    if (!food) {
      return;
    }

    this.router.navigate([
      '/food',
      food.foodCode
    ]);
  }

  isActive(index: number): boolean {
    return this.activeIndex() === index;
  }

  getActiveDescendant(): string | null {

    const index = this.activeIndex();

    if (index < 0) {
      return null;
    }

    return this.resultId(index);
  }

  resultId(index: number): string {
    return `food-result-${index}`;
  }

  highlightName(name: string): string {

    const term = this.query().trim();

    if (!term) {
      return this.escapeHtml(name);
    }

    const escapedName = this.escapeHtml(name);
    const escapedTerm = this.escapeRegExp(term);

    const expression = new RegExp(
      `(${escapedTerm})`,
      'gi'
    );

    return escapedName.replace(
      expression,
      '<mark>$1</mark>'
    );
  }

  trackFood(
    index: number,
    food: FoodSearchResult
  ): number {

    return food.foodCode ?? index;
  }

  private focusSearch(): void {

    this.searchInput?.nativeElement.focus();

    this.searchInput?.nativeElement.select();
  }

  private escapeHtml(value: string): string {

    return value
      .replaceAll('&', '&amp;')
      .replaceAll('<', '&lt;')
      .replaceAll('>', '&gt;')
      .replaceAll('"', '&quot;')
      .replaceAll("'", '&#039;');
  }

  private escapeRegExp(value: string): string {

    return value.replace(
      /[.*+?^${}()|[\]\\]/g,
      '\\$&'
    );
  }
}



