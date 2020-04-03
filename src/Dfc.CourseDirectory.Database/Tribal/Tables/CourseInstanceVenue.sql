CREATE TABLE [Tribal].[CourseInstanceVenue] (
    [CourseInstanceId] INT NOT NULL,
    [VenueId]          INT NOT NULL,
    CONSTRAINT [PK_CourseInstanceVenue] PRIMARY KEY CLUSTERED ([CourseInstanceId] ASC, [VenueId] ASC)
);

