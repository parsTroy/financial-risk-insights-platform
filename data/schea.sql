CREATE TABLE Assets (
    Id SERIAL PRIMARY KEY,
    Symbol VARCHAR(10) NOT NULL,
    Name VARCHAR(100) NOT NULL
);

CREATE TABLE Prices (
    Id SERIAL PRIMARY KEY,
    AssetId INT REFERENCES Assets(Id),
    Date DATE NOT NULL,
    Open NUMERIC,
    Close NUMERIC,
    High NUMERIC,
    Low NUMERIC,
    Volume BIGINT
);

CREATE TABLE Portfolios (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE PortfolioHoldings (
    PortfolioId INT REFERENCES Portfolios(Id),
    AssetId INT REFERENCES Assets(Id),
    Weight NUMERIC CHECK (Weight >= 0 AND Weight <= 1),
    PRIMARY KEY (PortfolioId, AssetId)
);
