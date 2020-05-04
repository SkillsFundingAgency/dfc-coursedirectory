CREATE TABLE [Tribal].[CourseInstanceStartDate] (
    [CourseInstanceStartDateId] INT  NOT NULL,
    [CourseInstanceId]          INT  NOT NULL,
    [StartDate]                 DATE NOT NULL,
    [IsMonthOnlyStartDate]      BIT  NOT NULL,
    [PlacesAvailable]           INT  NULL,
    CONSTRAINT [PK_CourseInstanceStartDates] PRIMARY KEY CLUSTERED ([CourseInstanceStartDateId] ASC)
);

