/*
-- =============================================
-- Author:		ATUL KUMAR
-- Create date: 04 FEB 2022
-- Description:	UPDATE FORM CONTROL PERMISSION FOR ROLE
-- =============================================
*/
CREATE OR ALTER PROCEDURE FormControlPermissionUpdate 
@Permissions UT_FormControlPermission READONLY,
@RoleId INT,
@UserId NVARCHAR(10)
AS
BEGIN
	DECLARE @RolePermission TABLE(ControlId INT, FormId INT, IsVisible BIT, RowNum INT);
	DECLARE @Index INT=1, @RowCount INT=0, @TempControlId INT, @TempFormId INT, @TempIsVisible BIT;

	INSERT @RolePermission(ControlId, FormId, IsVisible, RowNum)
	SELECT ControlId,FormId, IsVisible,ROW_NUMBER() OVER(ORDER BY ControlId) AS RowNum FROM @Permissions;

	SELECT @RowCount=COUNT(1) FROM @Permissions;

	BEGIN TRAN
	BEGIN TRY
		WHILE (@Index <= @RowCount)
		BEGIN
			SELECT @TempControlId=ControlId, @TempFormId=FormId, @TempIsVisible=IsVisible FROM @RolePermission WHERE RowNum=@Index;
			IF(@TempFormId=52)
			BEGIN
				IF EXISTS(SELECT 1 FROM QuotationColumnsRole WHERE RoleId=@RoleId AND ColumnId=@TempControlId)
				BEGIN
					UPDATE QuotationColumnsRole SET
						IsVisible=@TempIsVisible,
						UpdatedBy=@UserId,
						UpdatedOn=GETDATE()
					WHERE RoleId=@RoleId AND ColumnId=@TempControlId AND IsVisible<>@TempIsVisible;
				END
				ELSE
				BEGIN
					INSERT QuotationColumnsRole (ColumnId, RoleId, IsVisible, CreatedBy, CreatedOn)
						VALUES (@TempControlId, @RoleId, @TempIsVisible, @UserId, GETDATE());
				END
			END
			ELSE
			BEGIN
				IF EXISTS(SELECT 1 FROM FormControlRoleMaster WHERE RoleId=@RoleId AND ControlId=@TempControlId AND FormId=@TempFormId)
				BEGIN
					UPDATE FormControlRoleMaster SET
						IsVisible=@TempIsVisible,
						UpdatedBy=@UserId,
						UpdatedOn=GETDATE()
					WHERE RoleId=@RoleId AND ControlId=@TempControlId AND FormId=@TempFormId AND IsVisible<>@TempIsVisible;
				END
				ELSE
				BEGIN
					INSERT FormControlRoleMaster (ControlId, FormId, RoleId, IsVisible, CreatedBy, CreatedOn)
						VALUES (@TempControlId, @TempFormId, @RoleId, @TempIsVisible, @UserId, GETDATE());
				END
			END

			SELECT @Index=@Index+1;
		END
		COMMIT;
		SELECT 1 AS Result, 'Process completed' Response;
	END TRY
	BEGIN CATCH
		ROLLBACK;
		SELECT 0 AS Result, 'Process failed ' + @@ERROR Response;
	END CATCH
END
GO
