IF NOT EXISTS (SELECT * FROM SYS.TABLE_TYPES WHERE NAME ='UT_RolePermissions')
CREATE TYPE UT_RolePermissions AS TABLE 
(
	RoleId INT,
	FormId INT,
	FormView BIT,
	FormAdd BIT,
	FormEdit BIT
	
	
)
GO
