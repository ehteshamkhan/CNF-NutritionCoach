PRAGMA foreign_keys = ON;
PRAGMA synchronous = FULL;

CREATE TABLE IF NOT EXISTS Foods
(
    FoodCode INTEGER NOT NULL,
    FoodName TEXT NOT NULL,

    CONSTRAINT PK_Foods
        PRIMARY KEY (FoodCode),

    CONSTRAINT CK_Foods_FoodName_NotBlank
        CHECK (length(trim(FoodName)) > 0)
);

CREATE TABLE IF NOT EXISTS MeasureTypes
(
    MeasureTypeCode INTEGER NOT NULL,
    MeasureType TEXT NOT NULL,

    CONSTRAINT PK_MeasureTypes
        PRIMARY KEY (MeasureTypeCode),

    CONSTRAINT CK_MeasureTypes_Name_NotBlank
        CHECK (length(trim(MeasureType)) > 0)
);

CREATE TABLE IF NOT EXISTS Measures
(
    Id INTEGER NOT NULL
        CONSTRAINT PK_Measures
        PRIMARY KEY AUTOINCREMENT,

    FoodCode INTEGER NOT NULL,
    MeasureCode INTEGER NOT NULL,
    Measure TEXT NOT NULL,
    MeasureTypeCode INTEGER NOT NULL,
    MeasureType TEXT NOT NULL,
    WeightGrams NUMERIC NOT NULL,

    CONSTRAINT FK_Measures_Food
        FOREIGN KEY (FoodCode)
        REFERENCES Foods(FoodCode)
        ON UPDATE RESTRICT
        ON DELETE RESTRICT,

    CONSTRAINT FK_Measures_MeasureType
        FOREIGN KEY (MeasureTypeCode)
        REFERENCES MeasureTypes(MeasureTypeCode)
        ON UPDATE RESTRICT
        ON DELETE RESTRICT,

    CONSTRAINT UQ_Measures_Food_MeasureCode
        UNIQUE (FoodCode, MeasureCode),

    CONSTRAINT CK_Measures_Measure_NotBlank
        CHECK (length(trim(Measure)) > 0),

    CONSTRAINT CK_Measures_MeasureType_NotBlank
        CHECK (length(trim(MeasureType)) > 0),

    CONSTRAINT CK_Measures_Weight_NonNegative
        CHECK (WeightGrams >= 0)
);

CREATE TABLE IF NOT EXISTS Nutrients
(
    Id INTEGER NOT NULL
        CONSTRAINT PK_Nutrients
        PRIMARY KEY AUTOINCREMENT,

    FoodCode INTEGER NOT NULL,
    NutrientCode INTEGER NOT NULL,
    NutrientName TEXT NOT NULL,
    NutrientSymbol TEXT NOT NULL,
    NutrientUnit TEXT NOT NULL,
    AmountPer100g NUMERIC NOT NULL,
    StandardError NUMERIC NULL,
    Observations NUMERIC NULL,
    NutrientSourceCode INTEGER NOT NULL,
    NutrientLastUpdatedDate TEXT NOT NULL,

    CONSTRAINT FK_Nutrients_Food
        FOREIGN KEY (FoodCode)
        REFERENCES Foods(FoodCode)
        ON UPDATE RESTRICT
        ON DELETE RESTRICT,

    CONSTRAINT UQ_Nutrients_Food_NutrientCode
        UNIQUE (FoodCode, NutrientCode),

    CONSTRAINT CK_Nutrients_Name_NotBlank
        CHECK (length(trim(NutrientName)) > 0),

    CONSTRAINT CK_Nutrients_Symbol_NotBlank
        CHECK (length(trim(NutrientSymbol)) > 0),

    CONSTRAINT CK_Nutrients_Unit_NotBlank
        CHECK (length(trim(NutrientUnit)) > 0),

    CONSTRAINT CK_Nutrients_StandardError_NonNegative
        CHECK (
            StandardError IS NULL
            OR StandardError >= 0
        ),

    CONSTRAINT CK_Nutrients_Observations_NonNegative
        CHECK (
            Observations IS NULL
            OR Observations >= 0
        )
);

CREATE INDEX IF NOT EXISTS IX_Foods_FoodName
    ON Foods(FoodName);

CREATE INDEX IF NOT EXISTS IX_Measures_FoodCode
    ON Measures(FoodCode);

CREATE INDEX IF NOT EXISTS IX_Measures_MeasureTypeCode
    ON Measures(MeasureTypeCode);

CREATE INDEX IF NOT EXISTS IX_Nutrients_FoodCode
    ON Nutrients(FoodCode);

CREATE INDEX IF NOT EXISTS IX_Nutrients_NutrientCode
    ON Nutrients(NutrientCode);

CREATE INDEX IF NOT EXISTS IX_Nutrients_Name
    ON Nutrients(NutrientName);