

CREATE PROCEDURE [dbo].[dfc_GetCourseInstancesByCourseId]

	@CourseId int

AS
	SELECT				ci.CourseId,
						civ.VenueId,
						ci.CourseInstanceId,
						ci.ProviderOwnCourseInstanceRef,
						ci.AttendanceTypeId,
						ci.StartDateDescription,
						cisd.StartDate,
						ci.[Url],
						ci.Price,
						ci.PriceAsText,
						ci.DurationUnitId,
						ci.DurationUnit,
						ci.StudyModeId,
						ci.AttendancePatternId,
						ci.RecordStatusId,
						VN.VenueName
	  FROM				Tribal.CourseInstance ci
	  LEFT OUTER JOIN	Tribal.CourseInstanceVenue civ ON ci.CourseInstanceId = civ.CourseInstanceId
	  LEFT OUTER JOIN	Tribal.CourseInstanceStartDate cisd ON ci.CourseInstanceId = cisd.CourseInstanceId  
	   LEFT OUTER JOIN	Tribal.Venue VN ON civ.VenueId = VN.VenueId
	  WHERE				CourseId = @CourseId
	  AND				ci.RecordStatusId = 2