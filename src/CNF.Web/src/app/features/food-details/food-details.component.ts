import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal
} from '@angular/core';

import {
  ActivatedRoute,
  Router
} from '@angular/router';

import {
  MatButtonModule
} from '@angular/material/button';

import {
  MatProgressSpinnerModule
} from '@angular/material/progress-spinner';

import {
  CnfApiService
} from '../../core/services/cnf-api.service';

import {
  Food,
  FoodNutrientsResponse,
  Measure,
  Nutrient
} from '../../core/models/food.model';

@Component({
  selector: 'app-food-details',
  imports: [
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './food-details.component.html',
  styleUrl: './food-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FoodDetailsComponent {

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(CnfApiService);

  readonly food = signal<Food | null>(null);
  readonly nutrientsResponse =
    signal<FoodNutrientsResponse | null>(null);
  readonly measures = signal<Measure[]>([]);

  readonly loading = signal(true);
  readonly error = signal('');

  readonly nutrients = computed(
    () => this.nutrientsResponse()?.nutrients ?? []
  );

  readonly energy = computed(() =>
    this.findNutrient(208)
  );

  readonly protein = computed(() =>
    this.findNutrient(203)
  );

  readonly fat = computed(() =>
    this.findNutrient(204)
  );

  readonly carbs = computed(() =>
    this.findNutrient(205)
  );

  readonly fibre = computed(() =>
    this.findNutrient(291)
  );

  readonly calcium = computed(() =>
    this.findNutrient(301)
  );

  readonly sodium = computed(() =>
    this.findNutrient(307)
  );

  readonly macros = computed(() =>
    this.nutrients().filter(n =>
      [203, 204, 205, 291].includes(n.nutrientCode)
    )
  );

  readonly minerals = computed(() =>
    this.nutrients().filter(n =>
      [
        301,
        303,
        304,
        305,
        306,
        307,
        309,
        312,
        315,
        317
      ].includes(n.nutrientCode)
    )
  );

  readonly vitamins = computed(() =>
    this.nutrients().filter(n =>
      [
        319,
        320,
        321,
        323,
        324,
        325,
        328,
        334,
        337,
        338,
        401,
        404,
        405,
        406,
        410,
        415,
        417,
        418,
        430,
        431,
        432,
        435
      ].includes(n.nutrientCode)
    )
  );

  readonly aminoAcids = computed(() =>
    this.nutrients().filter(n =>
      [
        501,
        502,
        503,
        504,
        505,
        506,
        507,
        508,
        509,
        510,
        511,
        512,
        513,
        514,
        515,
        516,
        517,
        518
      ].includes(n.nutrientCode)
    )
  );

  readonly fattyAcids = computed(() =>
    this.nutrients().filter(n =>
      n.nutrientCode >= 600
    )
  );

  constructor() {

    const foodCode = Number(
      this.route.snapshot.paramMap.get('foodCode')
    );

    if (!Number.isInteger(foodCode) || foodCode <= 0) {
      this.error.set('Invalid food code.');
      this.loading.set(false);
      return;
    }

    this.loadFood(foodCode);
  }

  private loadFood(foodCode: number): void {

    this.loading.set(true);
    this.error.set('');

    this.api.getFood(foodCode).subscribe({
      next: food => {
        this.food.set(food);
        this.loadNutrients(foodCode);
        this.loadMeasures(foodCode);
      },
      error: error => {
        console.error(error);
        this.loading.set(false);
        this.error.set(
          'The requested food could not be loaded.'
        );
      }
    });
  }

  private loadNutrients(foodCode: number): void {

    this.api.getNutrients(foodCode).subscribe({
      next: response => {
        this.nutrientsResponse.set(response);
        this.loading.set(false);
      },
      error: error => {
        console.error(error);
        this.loading.set(false);
        this.error.set(
          'The food was found, but nutrient data could not be loaded.'
        );
      }
    });
  }

  private loadMeasures(foodCode: number): void {

    this.api.getMeasures(foodCode).subscribe({
      next: response => {
        this.measures.set(response.measures);
      },
      error: error => {
        console.error(error);
      }
    });
  }

  findNutrient(code: number): Nutrient | null {

    return this.nutrients().find(
      nutrient => nutrient.nutrientCode === code
    ) ?? null;
  }

  formatAmount(nutrient: Nutrient | null): string {

    if (!nutrient) {
      return '—';
    }

    return `${this.formatNumber(nutrient.amountPer100g)} ${this.unitLabel(nutrient.nutrientUnit)}`;
  }

  formatNumber(value: number): string {

    if (!Number.isFinite(value)) {
      return '—';
    }

    if (value === 0) {
      return '0';
    }

    if (Math.abs(value) >= 100) {
      return value.toLocaleString(
        'en-CA',
        {
          maximumFractionDigits: 3
        }
      );
    }

    return value.toLocaleString(
      'en-CA',
      {
        maximumFractionDigits: 5
      }
    );
  }

  unitLabel(unit: string): string {

    switch (unit.toLowerCase()) {
      case 'gram':
        return 'g';

      case 'milligram':
        return 'mg';

      case 'microgram':
        return 'µg';

      case 'kilocalorie':
        return 'kcal';

      case 'kilojoule':
        return 'kJ';

      case 'international unit':
        return 'IU';

      case 'niacin equivalents':
        return 'NE';

      default:
        return unit;
    }
  }

  goBack(): void {
    void this.router.navigate(['/']);
  }

  trackByCode(
    _index: number,
    nutrient: Nutrient
  ): number {
    return nutrient.nutrientCode;
  }

  trackByMeasure(
    _index: number,
    measure: Measure
  ): number {
    return measure.measureCode;
  }
}
