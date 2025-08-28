CREATE TABLE Assets (
    Id SERIAL PRIMARY KEY,
    Symbol VARCHAR(10) NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Sector VARCHAR(50),
    Industry VARCHAR(50),
    AssetType VARCHAR(50),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Prices (
    Id SERIAL PRIMARY KEY,
    AssetId INT REFERENCES Assets(Id),
    Date DATE NOT NULL,
    Open NUMERIC(18,6),
    High NUMERIC(18,6),
    Low NUMERIC(18,6),
    Close NUMERIC(18,6),
    AdjustedClose NUMERIC(18,6),
    Volume BIGINT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Portfolios (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    Strategy VARCHAR(50),
    TargetReturn NUMERIC(18,6),
    MaxRisk NUMERIC(18,6),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE
);

CREATE TABLE PortfolioHoldings (
    PortfolioId INT REFERENCES Portfolios(Id),
    AssetId INT REFERENCES Assets(Id),
    Weight NUMERIC(18,6) CHECK (Weight >= 0 AND Weight <= 1),
    Quantity NUMERIC(18,6),
    AverageCost NUMERIC(18,6),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (PortfolioId, AssetId)
);

-- Create indexes for better performance
CREATE UNIQUE INDEX IX_Assets_Symbol ON Assets(Symbol);
CREATE INDEX IX_Prices_AssetId_Date ON Prices(AssetId, Date);
CREATE UNIQUE INDEX IX_Prices_AssetId_Date_Unique ON Prices(AssetId, Date);
