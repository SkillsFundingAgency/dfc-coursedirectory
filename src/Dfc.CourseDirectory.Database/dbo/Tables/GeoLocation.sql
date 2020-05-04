CREATE TABLE [dbo].[GeoLocation] (
    [Postcode] NVARCHAR (8) NOT NULL,
    [Lat]      FLOAT (53)   NOT NULL,
    [Lng]      FLOAT (53)   NOT NULL,
    [Northing] FLOAT (53)   NOT NULL,
    [Easting]  FLOAT (53)   NOT NULL,
    PRIMARY KEY CLUSTERED ([Postcode] ASC)
);

