/****** Object:  StoredProcedure [dbo].[dfc_GetUserAuthorisationDetailsByEmail]    Script Date: 29/04/2019 12:17:00 ******/
CREATE PROCEDURE [dbo].[dfc_GetUserAuthorisationDetailsByEmail]
	@Email nvarchar (256)

AS
	SELECT			usr.id AS UserId,
					usr.Email,
					usr.UserName,
					usr.Name AS NameOfUser,
					roles.Id AS RoleId,
					roles.Name AS RoleName,
					claims.ClaimValue AS UKPRN,
					usr.LockoutEnabled,
					usr.LockoutEndDateUtc
	FROM			[Identity].AspNetUsers usr 
	LEFT OUTER JOIN	[Identity].AspNetUserRoles usrrole
	ON				usr.Id = usrrole.UserId
	LEFT OUTER JOIN [Identity].AspNetRoles roles
	ON				usrrole.RoleId = roles.Id
	LEFT OUTER JOIN [Identity].AspNetUserClaims claims
	ON				usr.Id = claims.UserId
	LEFT OUTER JOIN Tribal.ProviderUser prusr
	ON				usr.Id = prusr.UserId
	LEFT OUTER JOIN Tribal.Provider pr
	ON				prusr.ProviderId = pr.ProviderId
	WHERE			usr.Email = @Email
GO


