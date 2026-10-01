import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  Food,
  FoodMeasuresResponse,
  FoodNutrientsResponse,
  FoodSearchResponse
} from '../models/food.model';

@Injectable({
  providedIn: 'root'
})
export class CnfApiService {

  private readonly http = inject(HttpClient);

  private readonly baseUrl = 'http://localhost:5285/api';

  searchFoods(
    query: string,
    limit = 20
  ): Observable<FoodSearchResponse> {

    const params = new HttpParams()
      .set('q', query)
      .set('limit', limit);

    return this.http.get<FoodSearchResponse>(
      `${this.baseUrl}/foods/search`,
      { params }
    );
  }

  getFood(foodCode: number): Observable<Food> {

    return this.http.get<Food>(
      `${this.baseUrl}/foods/${foodCode}`
    );
  }

  getNutrients(foodCode: number): Observable<FoodNutrientsResponse> {

    return this.http.get<FoodNutrientsResponse>(
      `${this.baseUrl}/foods/${foodCode}/nutrients`
    );
  }

  getMeasures(foodCode: number): Observable<FoodMeasuresResponse> {

    return this.http.get<FoodMeasuresResponse>(
      `${this.baseUrl}/foods/${foodCode}/measures`
    );
  }
}
