/*
-- =============================================
-- Author:		ATUL KUMAR
-- Create date: 04 FEB 2022
-- Description:	GET CONTROL PERMISSION BY USER AND FORM
-- =============================================
*/
CREATE OR ALTER PROCEDURE FormControlPermissionByUser 
@FormId INT,
@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @UserRoleId INT;
	SELECT @UserRoleId=RoleId FROM UserMaster WHERE Id=@UserId;

    SELECT FC.ControlId, FC.ControlName, FC.FormId, COALESCE(FCR.IsVisible, FC.IsVisible) AS IsVisible FROM FormControlMaster FC 
		LEFT JOIN FormControlRoleMaster FCR ON FC.FormId=FCR.FormId AND FC.ControlId=FCR.ControlId AND FCR.RoleId=@UserRoleId
   WHERE FC.FormId=@FormId;
END
GO
