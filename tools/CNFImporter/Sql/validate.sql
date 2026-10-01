PRAGMA foreign_keys = ON;

-- Expected counts
SELECT 'Foods' AS CheckName,
       COUNT(*) AS Actual,
       5993 AS Expected,
       CASE WHEN COUNT(*) = 5993 THEN 'PASS' ELSE 'FAIL' END AS Result
FROM Foods;

SELECT 'MeasureTypes' AS CheckName,
       COUNT(*) AS Actual,
       3 AS Expected,
       CASE WHEN COUNT(*) = 3 THEN 'PASS' ELSE 'FAIL' END AS Result
FROM MeasureTypes;

SELECT 'Measures' AS CheckName,
       COUNT(*) AS Actual,
       29868 AS Expected,
       CASE WHEN COUNT(*) = 29868 THEN 'PASS' ELSE 'FAIL' END AS Result
FROM Measures;

SELECT 'Nutrients' AS CheckName,
       COUNT(*) AS Actual,
       565409 AS Expected,
       CASE WHEN COUNT(*) = 565409 THEN 'PASS' ELSE 'FAIL' END AS Result
FROM Nutrients;

-- Duplicate natural keys
SELECT FoodCode, MeasureCode, COUNT(*) AS DuplicateCount
FROM Measures
GROUP BY FoodCode, MeasureCode
HAVING COUNT(*) > 1;

SELECT FoodCode, NutrientCode, COUNT(*) AS DuplicateCount
FROM Nutrients
GROUP BY FoodCode, NutrientCode
HAVING COUNT(*) > 1;

-- Orphan checks
SELECT COUNT(*) AS OrphanMeasures
FROM Measures m
LEFT JOIN Foods f
    ON f.FoodCode = m.FoodCode
WHERE f.FoodCode IS NULL;

SELECT COUNT(*) AS OrphanNutrients
FROM Nutrients n
LEFT JOIN Foods f
    ON f.FoodCode = n.FoodCode
WHERE f.FoodCode IS NULL;

SELECT COUNT(*) AS OrphanMeasureTypes
FROM Measures m
LEFT JOIN MeasureTypes mt
    ON mt.MeasureTypeCode = m.MeasureTypeCode
WHERE mt.MeasureTypeCode IS NULL;

-- Coverage
SELECT COUNT(DISTINCT m.FoodCode) AS MeasureFoodCoverage
FROM Measures m;

SELECT COUNT(DISTINCT n.FoodCode) AS NutrientFoodCoverage
FROM Nutrients n;

-- Blank required fields
SELECT COUNT(*) AS BlankFoodNames
FROM Foods
WHERE trim(FoodName) = '';

SELECT COUNT(*) AS BlankMeasureNames
FROM Measures
WHERE trim(Measure) = '';

SELECT COUNT(*) AS BlankNutrientNames
FROM Nutrients
WHERE trim(NutrientName) = '';

SELECT COUNT(*) AS BlankNutrientSymbols
FROM Nutrients
WHERE trim(NutrientSymbol) = '';

SELECT COUNT(*) AS BlankNutrientUnits
FROM Nutrients
WHERE trim(NutrientUnit) = '';

-- SQLite integrity
PRAGMA integrity_check;
PRAGMA foreign_key_check;