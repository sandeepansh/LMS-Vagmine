IF NOT EXISTS (SELECT * FROM SYS.TABLE_TYPES WHERE NAME ='UT_FormControlPermission')
CREATE TYPE UT_FormControlPermission AS TABLE 
(
	ControlId INT,
	FormId INT,
	IsVisible BIT	
)
GO
