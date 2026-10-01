-- ============================================================
-- CNF NUTRITIONCOACH - POST IMPORT VALIDATION
-- ============================================================
--
-- Expected validated source:
--
-- Foods:        5,993
-- MeasureTypes:     3
-- Measures:     29,868
-- Nutrients:   565,409
--
-- Expected:
--   Duplicate keys       = 0
--   Orphan records       = 0
--   FK violations        = 0
--   Missing food coverage= 0
-- ============================================================


PRAGMA foreign_keys = ON;


-- ============================================================
-- 1. BASIC RECORD COUNTS
-- ============================================================

SELECT
    'Foods' AS TableName,
    5993 AS Expected,
    COUNT(*) AS Actual,
    CASE
        WHEN COUNT(*) = 5993 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Foods;


SELECT
    'MeasureTypes' AS TableName,
    3 AS Expected,
    COUNT(*) AS Actual,
    CASE
        WHEN COUNT(*) = 3 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM MeasureTypes;


SELECT
    'Measures' AS TableName,
    29868 AS Expected,
    COUNT(*) AS Actual,
    CASE
        WHEN COUNT(*) = 29868 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Measures;


SELECT
    'Nutrients' AS TableName,
    565409 AS Expected,
    COUNT(*) AS Actual,
    CASE
        WHEN COUNT(*) = 565409 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Nutrients;


-- ============================================================
-- 2. DISTINCT KEY COUNTS
-- ============================================================

SELECT
    'Distinct Food Codes' AS CheckName,
    5993 AS Expected,
    COUNT(DISTINCT FoodCode) AS Actual,
    CASE
        WHEN COUNT(DISTINCT FoodCode) = 5993
        THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Foods;


SELECT
    'Distinct Measure Type Codes' AS CheckName,
    3 AS Expected,
    COUNT(DISTINCT MeasureTypeCode) AS Actual,
    CASE
        WHEN COUNT(DISTINCT MeasureTypeCode) = 3
        THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM MeasureTypes;


SELECT
    'Distinct Measure Food/Code pairs' AS CheckName,
    29868 AS Expected,
    COUNT(DISTINCT
        CAST(FoodCode AS TEXT) || ':' ||
        CAST(MeasureCode AS TEXT)
    ) AS Actual,
    CASE
        WHEN COUNT(DISTINCT
            CAST(FoodCode AS TEXT) || ':' ||
            CAST(MeasureCode AS TEXT)
        ) = 29868
        THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Measures;


SELECT
    'Distinct Nutrient Food/Code pairs' AS CheckName,
    565409 AS Expected,
    COUNT(DISTINCT
        CAST(FoodCode AS TEXT) || ':' ||
        CAST(NutrientCode AS TEXT)
    ) AS Actual,
    CASE
        WHEN COUNT(DISTINCT
            CAST(FoodCode AS TEXT) || ':' ||
            CAST(NutrientCode AS TEXT)
        ) = 565409
        THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Nutrients;


-- ============================================================
-- 3. DUPLICATE FOOD CODES
-- ============================================================

SELECT
    'Duplicate Food Codes' AS CheckName,
    COUNT(*) AS Violations,
    CASE
        WHEN COUNT(*) = 0 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM
(
    SELECT FoodCode
    FROM Foods
    GROUP BY FoodCode
    HAVING COUNT(*) > 1
);


-- ============================================================
-- 4. DUPLICATE MEASURE NATURAL KEYS
-- ============================================================

SELECT
    'Duplicate FoodCode + MeasureCode' AS CheckName,
    COUNT(*) AS Violations,
    CASE
        WHEN COUNT(*) = 0 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM
(
    SELECT
        FoodCode,
        MeasureCode
    FROM Measures
    GROUP BY
        FoodCode,
        MeasureCode
    HAVING COUNT(*) > 1
);


-- ============================================================
-- 5. DUPLICATE NUTRIENT NATURAL KEYS
-- ============================================================

SELECT
    'Duplicate FoodCode + NutrientCode' AS CheckName,
    COUNT(*) AS Violations,
    CASE
        WHEN COUNT(*) = 0 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM
(
    SELECT
        FoodCode,
        NutrientCode
    FROM Nutrients
    GROUP BY
        FoodCode,
        NutrientCode
    HAVING COUNT(*) > 1
);


-- ============================================================
-- 6. FOREIGN KEY VALIDATION
-- ============================================================

SELECT
    'Foreign Key Violations' AS CheckName,
    COUNT(*) AS Violations,
    CASE
        WHEN COUNT(*) = 0 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM pragma_foreign_key_check;


-- ============================================================
-- 7. ORPHAN MEASURES
-- ============================================================

SELECT
    'Orphan Measures' AS CheckName,
    COUNT(*) AS Violations,
    CASE
        WHEN COUNT(*) = 0 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Measures m
LEFT JOIN Foods f
    ON f.FoodCode = m.FoodCode
WHERE f.FoodCode IS NULL;


-- ============================================================
-- 8. ORPHAN NUTRIENTS
-- ============================================================

SELECT
    'Orphan Nutrients' AS CheckName,
    COUNT(*) AS Violations,
    CASE
        WHEN COUNT(*) = 0 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Nutrients n
LEFT JOIN Foods f
    ON f.FoodCode = n.FoodCode
WHERE f.FoodCode IS NULL;


-- ============================================================
-- 9. ORPHAN MEASURE TYPES
-- ============================================================

SELECT
    'Orphan Measure Types' AS CheckName,
    COUNT(*) AS Violations,
    CASE
        WHEN COUNT(*) = 0 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Measures m
LEFT JOIN MeasureTypes mt
    ON mt.MeasureTypeCode = m.MeasureTypeCode
WHERE mt.MeasureTypeCode IS NULL;


-- ============================================================
-- 10. FOOD COVERAGE BY MEASURES
-- ============================================================

SELECT
    'Foods represented in Measures' AS CheckName,
    5993 AS Expected,
    COUNT(DISTINCT FoodCode) AS Actual,
    CASE
        WHEN COUNT(DISTINCT FoodCode) = 5993
        THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Measures;


-- ============================================================
-- 11. FOOD COVERAGE BY NUTRIENTS
-- ============================================================

SELECT
    'Foods represented in Nutrients' AS CheckName,
    5993 AS Expected,
    COUNT(DISTINCT FoodCode) AS Actual,
    CASE
        WHEN COUNT(DISTINCT FoodCode) = 5993
        THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Nutrients;


-- ============================================================
-- 12. FOODS WITHOUT MEASURES
-- ============================================================

SELECT
    'Foods without Measures' AS CheckName,
    COUNT(*) AS Violations,
    CASE
        WHEN COUNT(*) = 0 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Foods f
LEFT JOIN Measures m
    ON m.FoodCode = f.FoodCode
WHERE m.FoodCode IS NULL;


-- ============================================================
-- 13. FOODS WITHOUT NUTRIENTS
-- ============================================================

SELECT
    'Foods without Nutrients' AS CheckName,
    COUNT(*) AS Violations,
    CASE
        WHEN COUNT(*) = 0 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Foods f
LEFT JOIN Nutrients n
    ON n.FoodCode = f.FoodCode
WHERE n.FoodCode IS NULL;


-- ============================================================
-- 14. NULL / BLANK FOOD NAMES
-- ============================================================

SELECT
    'Invalid Food Names' AS CheckName,
    COUNT(*) AS Violations,
    CASE
        WHEN COUNT(*) = 0 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Foods
WHERE FoodName IS NULL
   OR length(trim(FoodName)) = 0;


-- ============================================================
-- 15. INVALID MEASURE RECORDS
-- ============================================================

SELECT
    'Invalid Measures' AS CheckName,
    COUNT(*) AS Violations,
    CASE
        WHEN COUNT(*) = 0 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Measures
WHERE Measure IS NULL
   OR length(trim(Measure)) = 0
   OR MeasureType IS NULL
   OR length(trim(MeasureType)) = 0
   OR WeightGrams IS NULL;


-- ============================================================
-- 16. INVALID NUTRIENT RECORDS
-- ============================================================

SELECT
    'Invalid Nutrients' AS CheckName,
    COUNT(*) AS Violations,
    CASE
        WHEN COUNT(*) = 0 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Nutrients
WHERE NutrientName IS NULL
   OR length(trim(NutrientName)) = 0
   OR NutrientSymbol IS NULL
   OR length(trim(NutrientSymbol)) = 0
   OR NutrientUnit IS NULL
   OR length(trim(NutrientUnit)) = 0
   OR AmountPer100g IS NULL
   OR NutrientSourceCode IS NULL
   OR NutrientLastUpdatedDate IS NULL;


-- ============================================================
-- 17. STANDARD ERROR / OBSERVATIONS NULL HANDLING
-- ============================================================
--
-- NULL is valid.
-- Explicit zero is also valid.
--
-- This check therefore does NOT require either field to be
-- populated.
-- ============================================================

SELECT
    'StandardError / Observations NULL handling' AS CheckName,
    COUNT(*) AS Violations,
    CASE
        WHEN COUNT(*) = 0 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Nutrients
WHERE typeof(StandardError) NOT IN ('null', 'real', 'integer')
   OR typeof(Observations) NOT IN ('null', 'real', 'integer');


-- ============================================================
-- 18. MEASURE TYPE TEXT CONSISTENCY
-- ============================================================
--
-- Every MeasureTypeCode should resolve to one canonical
-- MeasureType value in the MeasureTypes table.
-- ============================================================

SELECT
    'Measure Type consistency' AS CheckName,
    COUNT(*) AS Violations,
    CASE
        WHEN COUNT(*) = 0 THEN 'PASS'
        ELSE 'FAIL'
    END AS Result
FROM Measures m
JOIN MeasureTypes mt
    ON mt.MeasureTypeCode = m.MeasureTypeCode
WHERE trim(m.MeasureType) <> trim(mt.MeasureType);


-- ============================================================
-- 19. NUTRIENT DEFINITION CONSISTENCY
-- ============================================================
--
-- Diagnostic check.
--
-- If this returns rows, the same NutrientCode appears with
-- different names, symbols, or units.
--
-- It does not invalidate the current schema because the
-- Nutrient record itself remains the authoritative row.
-- ============================================================

SELECT
    NutrientCode,
    COUNT(DISTINCT NutrientName) AS DistinctNames,
    COUNT(DISTINCT NutrientSymbol) AS DistinctSymbols,
    COUNT(DISTINCT NutrientUnit) AS DistinctUnits
FROM Nutrients
GROUP BY NutrientCode
HAVING
       COUNT(DISTINCT NutrientName) > 1
    OR COUNT(DISTINCT NutrientSymbol) > 1
    OR COUNT(DISTINCT NutrientUnit) > 1
ORDER BY NutrientCode;


-- ============================================================
-- 20. TOTAL CORE RECORD COUNT
-- ============================================================

SELECT
    'Total Core Records' AS CheckName,
    601273 AS Expected,
    (
        (SELECT COUNT(*) FROM Foods) +
        (SELECT COUNT(*) FROM MeasureTypes) +
        (SELECT COUNT(*) FROM Measures) +
        (SELECT COUNT(*) FROM Nutrients)
    ) AS Actual,
    CASE
        WHEN
            (
                (SELECT COUNT(*) FROM Foods) +
                (SELECT COUNT(*) FROM MeasureTypes) +
                (SELECT COUNT(*) FROM Measures) +
                (SELECT COUNT(*) FROM Nutrients)
            ) = 601273
        THEN 'PASS'
        ELSE 'FAIL'
    END AS Result;


-- ============================================================
-- 21. INDEX INVENTORY
-- ============================================================

SELECT
    name AS IndexName,
    tbl_name AS TableName
FROM sqlite_master
WHERE type = 'index'
ORDER BY tbl_name, name;


-- ============================================================
-- 22. FOREIGN KEY INVENTORY
-- ============================================================

SELECT
    'Measures' AS TableName,
    *
FROM pragma_foreign_key_list('Measures');

SELECT
    'Nutrients' AS TableName,
    *
FROM pragma_foreign_key_list('Nutrients');


-- ============================================================
-- 23. TABLE INVENTORY
-- ============================================================

SELECT
    name AS TableName
FROM sqlite_master
WHERE type = 'table'
ORDER BY name;


-- ============================================================
-- 24. DATABASE INTEGRITY CHECK
-- ============================================================

PRAGMA integrity_check;


-- ============================================================
-- 25. FOREIGN KEY CHECK
-- ============================================================

PRAGMA foreign_key_check;


-- ============================================================
-- END VALIDATION
-- ============================================================