export interface FoodSearchResult {
  foodCode: number;
  foodName: string;
}

export interface FoodSearchResponse {
  query: string;
  count: number;
  items: FoodSearchResult[];
}

export interface Food {
  foodCode: number;
  foodName: string;
}

export interface Nutrient {
  nutrientCode: number;
  nutrientName: string;
  nutrientSymbol: string;
  nutrientUnit: string;
  amountPer100g: number;
  standardError: number | null;
  observations: number | null;
  nutrientSourceCode: number;
  nutrientLastUpdatedDate: string;
}

export interface FoodNutrientsResponse {
  foodCode: number;
  foodName: string;
  nutrients: Nutrient[];
}

export interface Measure {
  measureCode: number;
  measure: string;
  measureTypeCode: number;
  measureType: string;
  weightGrams: number;
}

export interface FoodMeasuresResponse {
  foodCode: number;
  foodName: string;
  measures: Measure[];
}
